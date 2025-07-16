using System;
using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using Fleck;

namespace gaton.WebSockets;
public static class WebSocketClient
{
    private static ClientWebSocket? socket;
    private static Uri? socketUri;
    public static async Task ConnectAsync(string playerName)
    {
        socketUri = new Uri($"ws://localhost:9001/{playerName}");
        socket = new ClientWebSocket();
        try
        {
            await socket.ConnectAsync(socketUri, CancellationToken.None);
            var identifyMessage = new
            {
                action_type = "identify",
                value = new { name = playerName }
            };
            string json = JsonConvert.SerializeObject(identifyMessage);
            byte[] buffer = Encoding.UTF8.GetBytes(json);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            Console.WriteLine($"Conectado al servidor como {playerName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al conectar: {ex.Message}");
        }
    }
}
