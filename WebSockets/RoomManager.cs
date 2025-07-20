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
    public static void HandleCreate(string name)
    {
        if (PlayerInRoom(name))
        {
            WebSocketServerLauncher.SendTo(name, new
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
        };
        HandleJoin(name, roomId); // Agrega el jugador a la sala
        ActiveRooms[roomId] = newRoom; // Agrega la sala al diccionario
        WebSocketServerLauncher.SendTo(name, new
        {
            action = "room-created",
            roomId,
            msg = $"Sala creada con ID {roomId}. Esperando otro jugador..."
        });
        WebSocketServerLauncher.Broadcast($"{name} ha creado una nueva sala: {roomId}");
        BroadcastRoomList();
        Console.WriteLine($"Sala creada: {roomId} por {name}");
    }
    public static void HandleJoin(string name, string roomId)
    {
        if (!ActiveRooms.ContainsKey(roomId))
        {
            WebSocketServerLauncher.SendTo(name, new
            {
                action = "error",
                msg = "Sala no encontrada."
            });
            return;
        }
        if (PlayerInRoom(name))
        {
            WebSocketServerLauncher.SendTo(name, new
            {
                action = "error",
                msg = "Ya estás en una sala, no puedes unirte a otra."
            });
            return;
        }
        var room = ActiveRooms[roomId]; // Obtiene la sala
        if (room.Players.Count >= 2)
        {
            WebSocketServerLauncher.SendTo(name, new
            {
                action = "join-failed",
                msg = "La sala está llena."
            });
            return;
        }
        room.Players.Add(name); // Agrega el jugador a la sala
        foreach (var jugador in room.Players)
        {
            WebSocketServerLauncher.SendTo(jugador, new
            {
                action = "room-joined",
                roomId,
                msg = $"{name} se ha unido a la sala. ¡La partida puede comenzar!",
                players = room.Players
            });
        }
        BroadcastRoomList();
        Console.WriteLine($"{name} se ha unido a la sala: {roomId}");
        // Inicar el juego
        if (room.Players.Count == 2)
        {
            GameSessionManager.StartGame(roomId, room.Players);
            Console.WriteLine($"Partida iniciada en la sala: {roomId}");
            WebSocketServerLauncher.SendTo(name, new
            {
                action = "game-started",
                msg = "Partida iniciada. ¡Buena suerte!"
            });
        }
    }
    public static void HandleLeave(string name, string roomId)
    {
        if (!ActiveRooms.ContainsKey(roomId))
        {
            WebSocketServerLauncher.SendTo(name, new
            {
                action = "leave-failed",
                msg = "La sala no existe."
            });
            return;
        }
        var room = ActiveRooms[roomId];
        if (!room.Players.Contains(name))
        {
            WebSocketServerLauncher.SendTo(name, new
            {
                action = "leave-failed",
                msg = "No estás dentro de esta sala."
            });
            return;
        }
        room.Players.Remove(name); // Elimina el jugador de la sala
        Console.WriteLine($"{name} ha abandonado la sala: {roomId}");
        foreach (var jugador in room.Players)
        {
            WebSocketServerLauncher.SendTo(jugador, new
            {
                action = "room-left",
                roomId,
                msg = $"{name} ha abandonado la sala.",
                players = room.Players
            });
        }
        WebSocketServerLauncher.SendTo(name, new
        {
            action = "room-left",
            roomId,
            msg = "Has abandonado la sala."
        });
        BroadcastRoomList();
        Console.WriteLine($"{name} ha abandonado la sala: {roomId}, y se le aviso a los demas");
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

        WebSocketServerLauncher.SendTo(playerName, new
        {
            action = "room-list",
            rooms = roomList
        });
    }
    public static void BroadcastRoomList()
    {
        foreach (var player in WebSocketServerLauncher.GetConnectedPlayerNames())
        {
            SendRoomListTo(player);
        }
    }
    private static bool PlayerInRoom(string playerName)
    {
        foreach (var room in ActiveRooms.Values) // Recorre todas las salas
        {
            if (room.Players.Contains(playerName)) // Si el jugador está en la sala
            {
                return true;
            }
        }
        return false;
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

}
