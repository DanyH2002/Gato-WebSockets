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

                    // if (entry?.Action_Type == "identify")
                    // {
                    //     var name = entry.Value?.Name;
                    //     Console.WriteLine($"Identificado como: {name}");

                    //     SendTo(name, new
                    //     {
                    //         action = "welcome",
                    //         msg = $"¡Bienvenido al Juego del Gato, {name}!"
                    //     });
                    // }
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

                        // case "move":
                        //     GameSessionManager.HandleMove(playerName, entry.Value.MoveData);
                        //     break;

                        // case "rematch":
                        //     RematchConnection.HandleRematchResponse(playerName, entry.Value.RematchAccepted);
                        //     break;
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
    public static void Broadcast(string message)
    {
        foreach (var socket in ConnectedPlayers.Values)
        {
            socket.Send(JsonConvert.SerializeObject(new { action = "system", msg = message }));
        }
    }

    public static void SendTo(string playerName, object payload)
    {
        if (ConnectedPlayers.TryGetValue(playerName, out var socket))
        {
            string json = JsonConvert.SerializeObject(payload);
            socket.Send(json);
        }
    }
    public static IEnumerable<string> GetConnectedPlayerNames()
    {
        return ConnectedPlayers.Keys;
    }

}
