using Fleck;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace gaton.WebSockets;

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
                    var entry = JsonConvert.DeserializeObject<Entry>(message);

                    if (entry?.Action_Type == "identify")
                    {
                        var name = entry.Value?.Name;
                        Console.WriteLine($"Identificado como: {name}");

                        SendTo(name, new
                        {
                            action = "welcome",
                            msg = $"¡Bienvenido al Juego del Gato, {name}!"
                        });
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
    private static void Broadcast(string message)
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

    private class Entry
    {
        public string? Action_Type { get; set; }
        public Payload? Value { get; set; }
    }

    private class Payload
    {
        public string? Name { get; set; }
    }
}
