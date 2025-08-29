using System;
using gaton.WebSockets;
using System.Collections.Generic;
using gaton.Model;

namespace gaton.WebSockets;
/*
Gestion del estado de las rooms
*/
public class RoomManager
{
    private static readonly Dictionary<string, RoomModel> ActiveRooms = new();
    private static readonly Dictionary<string, DateTime> EmptyRoomTimers = new();

    public static void HandleCreate(string name)
    {
        if (IsPlayerInRoom(name) || HasEmptyRoomCreatedBy(name))
        {
            WebSocketHandler.SendTo(name, new
            {
                action = "error",
                msg = "Ya has creado una sala. Únete o abandónala antes de crear otra."
            });
            return;
        }
        if (IsPlayerInRoom(name))
        {
            WebSocketHandler.SendTo(name, new
            {
                action = "error",
                msg = "Ya estás en una sala."
            });
            return;
        }
        string roomId = $"room-{Guid.NewGuid().ToString("N")[..6]}"; // Genera un ID 
        var newRoom = new RoomModel
        {
            RoomId = roomId,
            CreatedBy = name
        };
        ActiveRooms[roomId] = newRoom; // Agrega la sala al diccionario
        //HandleJoin(name, roomId);
        WebSocketHandler.SendTo(name, new
        {
            action = "room-created",
            roomId,
            msg = $"Sala creada con ID {roomId}. Esperando otro jugador..."
        });
        WebSocketHandler.Broadcast($"{name} ha creado una nueva sala: {roomId}");
        HandleJoin(name, roomId);
        BroadcastRoomList();
        Console.WriteLine($"Sala creada: {roomId} por {name}");
    }
    public static void HandleJoin(string name, string roomId)
    {
        if (!ActiveRooms.ContainsKey(roomId))
        {
            WebSocketHandler.SendTo(name, new
            {
                action = "error",
                msg = "Sala no encontrada."
            });
            return;
        }
        if (IsPlayerInRoom(name))
        {
            WebSocketHandler.SendTo(name, new
            {
                action = "error",
                msg = "Ya estás en una sala, no puedes unirte a otra."
            });
            return;
        }
        var room = ActiveRooms[roomId]; // Obtiene la sala
        if (room.Players.Count >= 2)
        {
            WebSocketHandler.SendTo(name, new
            {
                action = "join-failed",
                msg = "La sala está llena."
            });
            return;
        }
        room.Players.Add(name); // Agrega el jugador a la sala
        EmptyRoomTimers.Remove(roomId);
        foreach (var jugador in room.Players)
        {
            WebSocketHandler.SendTo(jugador, new
            {
                action = "room-joined",
                roomId,
                msg = $"{name} se ha unido a la sala. ¡La partida puede comenzar!",
                players = room.Players
            });
        }
        Console.WriteLine($"{name} se ha unido a la sala: {roomId}");
        // Inicar el juego
        if (room.Players.Count == 2)
        {
            GameSessionManager.StartGame(roomId, room.Players);
        }
        BroadcastRoomList();
    }
    public static void HandleLeave(string name, string roomId)
    {
        if (!ActiveRooms.ContainsKey(roomId))
        {
            WebSocketHandler.SendTo(name, new
            {
                action = "leave-failed",
                msg = "La sala no existe."
            });
            return;
        }
        var room = ActiveRooms[roomId];
        if (!room.Players.Contains(name))
        {
            WebSocketHandler.SendTo(name, new
            {
                action = "leave-failed",
                msg = "No estás dentro de esta sala."
            });
            return;
        }
        Task.Run(async () =>
        {
            await Task.Delay(500);
            GameSessionManager.ForceVictory(name, roomId);
        });
        room.Players.Remove(name); // Elimina el jugador de la sala

        Console.WriteLine($"{name} ha abandonado la sala: {roomId}");
        foreach (var jugador in room.Players)
        {
            Console.WriteLine($"Enviando room-left a: {jugador}");
            WebSocketHandler.SendTo(jugador, new
            {
                action = "room-left",
                roomId,
                msg = $"{name} ha abandonado la sala.",
                players = room.Players
            });
            //SendRoomListTo(name);
        }
        WebSocketHandler.SendTo(name, new
        {
            action = "room-left",
            roomId,
            msg = "Has abandonado la sala."
        });
        Console.WriteLine($"{name} ha abandonado la sala: {roomId}, y se le aviso a los demas");
        BroadcastRoomList();
        SendRoomListTo(name);
        // Si la sala queda vacía, inicia temporizador de eliminación
        if (room.Players.Count == 0)
        {
            EmptyRoomTimers[roomId] = DateTime.UtcNow;
            Task.Run(async () =>
            {
                await Task.Delay(20000);
                if (ActiveRooms.ContainsKey(roomId) &&
                    ActiveRooms[roomId].Players.Count == 0 &&
                    EmptyRoomTimers.ContainsKey(roomId) &&
                    (DateTime.UtcNow - EmptyRoomTimers[roomId]).TotalSeconds >= 20)
                {
                    ActiveRooms.Remove(roomId);
                    EmptyRoomTimers.Remove(roomId);
                    Console.WriteLine($"Sala {roomId} eliminada por inactividad.");
                    BroadcastRoomList();
                }
            });
        }
    }
    //* Metodos auxiliares
    public static void SendRoomListTo(string playerName)
    {
        var roomList = ActiveRooms.Select(r => new
        {
            r.Value.RoomId,
            players = r.Value.Players,
            isFull = r.Value.Players.Count >= 2
        }).ToList();

        WebSocketHandler.SendTo(playerName, new
        {
            action = "room-list",
            rooms = roomList
        });
        /*Console.WriteLine($"Lista de salas enviada a {playerName}, y contiene: {roomList.Count} sala(s).");
        foreach (var sala in roomList)
        {
            Console.WriteLine($"Sala ID: {sala.RoomId}, Jugadores: {string.Join(", ", sala.players)}, Llena: {sala.isFull}");
        }*/

    }
    public static void BroadcastRoomList()
    {
        foreach (var player in WebSocketHandler.GetConnectedPlayerNames())
        {
            SendRoomListTo(player);
        }
    }
    public static bool IsPlayerInRoom(string playerName)
    {
        //return ActiveRooms.Values.Any(r => r.Players.Contains(playerName));
        return ActiveRooms
        .Where(r => !EmptyRoomTimers.ContainsKey(r.Key)) // excluye salas en proceso de eliminación
        .Any(r => r.Value.Players.Contains(playerName));
    }

    //busca en todas las salas activas y devuelve el RoomId si el jugador está en alguna
    public static string? GetRoomIdOfPlayer(string playerName)
    {
        foreach (var kvp in ActiveRooms)
        {
            if (kvp.Value.Players.Contains(playerName))
                return kvp.Key;
        }
        return null;
    }
    // saber si la sala esta vacia 
    private static bool HasEmptyRoomCreatedBy(string playerName)
    {
        return ActiveRooms.Values.Any(r =>
            r.CreatedBy == playerName &&
            r.Players.Count == 0
        );
    }

}
