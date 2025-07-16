using System;
using Fleck;
using System.Collections.Generic;

namespace gaton.WebSockets;

public static class WebSocketServerLauncher
{
    private static readonly List<IWebSocketConnection> Connections = new();
    private static WebSocketServer? server;
    public static void Start()
    {
        server = new WebSocketServer("ws://0.0.0.0:9001"); // Escucha en todas las interfaces

        server.Start(socket =>
        {
            socket.OnOpen = () =>
            {
                string playerName = socket.ConnectionInfo.Path.Trim('/');
                Console.WriteLine($"Jugador conectado: {playerName}");
                Connections.Add(socket);
            };

            socket.OnClose = () =>
            {
                string playerName = socket.ConnectionInfo.Path.Trim('/');
                Console.WriteLine($"Jugador desconectado: {playerName}");
                Connections.Remove(socket);
            };

            socket.OnMessage = message =>
            {
                Console.WriteLine($"Mensaje recibido: {message}");
            };
        });

        Console.WriteLine("Servidor WebSocket iniciado en ws://localhost:9001");
    }
}
