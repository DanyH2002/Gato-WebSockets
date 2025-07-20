using Fleck;
using System.Collections.Generic;
using Newtonsoft.Json;
using gaton.Model;

namespace gaton.WebSockets;

/*
Inia el servidor, maneja conexion base, y enruta mensajes.
*/
public static class WebSocketServerLauncher
{
    private static WebSocketServer? server;
    private static readonly Dictionary<string, IWebSocketConnection> ConnectedPlayers = new();
    public static void Start()
    {
        server = new WebSocketServer("ws://0.0.0.0:9001"); // Lee todas las interfaces de red
        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                string playerName = socket.ConnectionInfo.Path.Trim('/');
                Console.WriteLine($"Conexión entrante: {playerName}");

                if (string.IsNullOrWhiteSpace(playerName))
                {
                    socket.Close();
                    return;
                }

                if (!ConnectedPlayers.ContainsKey(playerName))
                {
                    ConnectedPlayers[playerName] = socket;
                    Console.WriteLine($"Jugador conectado: {playerName}");
                    Broadcast($"{playerName} se ha unido al sistema.");
                }
            };
            socket.OnClose = () =>
            {
                string playerName = socket.ConnectionInfo.Path.Trim('/');
                Console.WriteLine($"Jugador desconectado: {playerName}");

                if (ConnectedPlayers.ContainsKey(playerName))
                {
                    ConnectedPlayers.Remove(playerName);
                    Broadcast($"{playerName} ha salido.");
                }
            };
            socket.OnMessage = message =>
            {
                Console.WriteLine($"Mensaje recibido de {socket.ConnectionInfo.Path.Trim('/')}: {message}");
                try
                {
                    var entry = JsonConvert.DeserializeObject<EntryModel>(message);
                    var playerName = entry.Value?.Name;
                    switch (entry.Action_Type)
                    {
                        case "identify":
                            Console.WriteLine($"Identificado como: {playerName}");
                            SendTo(playerName, new
                            {
                                action = "welcome",
                                msg = $"¡Bienvenido al Juego del Gato, {playerName}!"
                            });
                            RoomManager.SendRoomListTo(playerName);
                            break;
                        case "create":
                            RoomManager.HandleCreate(playerName);
                            break;

                        case "join":
                            RoomManager.HandleJoin(playerName, entry.Value.RoomId);
                            break;

                        case "leave":
                            RoomManager.HandleLeave(playerName, entry.Value.RoomId);
                            break;

                        case "move":
                            if (entry.Value?.Casilla is int casilla)
                            {
                                GameSessionManager.HandleMove(playerName, casilla);
                            }
                            break;
                        case "rematch-request":
                            GameSessionManager.RequestRematch(entry.Value.Name);
                            break;
                        case "rematch-decline":
                            GameSessionManager.RejectRematch(entry.Value.Name);
                            break;
                        case "request-room-list":
                            RoomManager.SendRoomListTo(playerName);
                            Console.WriteLine($"Lista de salas enviada a {playerName}");
                            break;
                        default:
                            Console.WriteLine("Mensaje no reconocido");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al procesar mensaje: {ex.Message}");
                }
            };
        });

        Console.WriteLine("Servidor WebSocket iniciado en ws://localhost:9001");
    }
    // Envia un mensaje a todos los jugadores conectados
    public static void Broadcast(string message)
    {
        foreach (var socket in ConnectedPlayers.Values)
        {
            socket.Send(JsonConvert.SerializeObject(new { action = "system", msg = message }));
        }
    }
    // Envia un mensaje a un jugador específico
    public static void SendTo(string playerName, object payload)
    {
        if (ConnectedPlayers.TryGetValue(playerName, out var socket))
        {
            string json = JsonConvert.SerializeObject(payload);
            socket.Send(json);
        }
    }
    // Obtiene los nombres de los jugadores conectados
    public static IEnumerable<string> GetConnectedPlayerNames()
    {
        return ConnectedPlayers.Keys;
    }

}
