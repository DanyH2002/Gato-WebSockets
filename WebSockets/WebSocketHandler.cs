using System.Net.WebSockets;
using System.Text;
using Newtonsoft.Json;
using gaton.Model;
namespace gaton.WebSockets;

/*
Inia el servidor, maneja conexion base, y enruta mensajes.
*/

public class WebSocketHandler
{
    private static readonly Dictionary<string, WebSocket> ConnectedPlayers = new();
    public static async Task HandleAsync(WebSocket socket, string playerName)
    {
        if (string.IsNullOrWhiteSpace(playerName))
        {
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Nombre inválido", CancellationToken.None);
            return;
        }

        if (!ConnectedPlayers.ContainsKey(playerName))
        {
            ConnectedPlayers[playerName] = socket;
            Console.WriteLine($"Jugador conectado: {playerName}");
            Broadcast($"{playerName} se ha unido al sistema.");
        }

        var buffer = new byte[4096];
        while (socket.State == WebSocketState.Open)
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Close)
            {
                Console.WriteLine($"Jugador desconectado: {playerName}");
                ConnectedPlayers.Remove(playerName);
                Broadcast($"{playerName} ha salido.");

                var roomId = RoomManager.GetRoomIdOfPlayer(playerName);
                if (roomId != null)
                {
                    await Task.Delay(1000);
                    var stillConnected = ConnectedPlayers.ContainsKey(playerName);
                    if (!stillConnected)
                        RoomManager.HandleLeave(playerName, roomId);
                }

                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Sesión terminada", CancellationToken.None);
                return;
            }

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Mensaje recibido de {playerName}: {message}");

            try
            {
                var entry = JsonConvert.DeserializeObject<EntryModel>(message);
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
                        RoomManager.SendRoomListTo(playerName);
                        break;
                    case "move":
                        if (entry.Value?.Casilla is int casilla)
                            GameSessionManager.HandleMove(playerName, casilla);
                        break;
                    case "rematch-request":
                        GameSessionManager.RequestRematch(entry.Value.Name);
                        break;
                    case "rematch-decline":
                        GameSessionManager.RejectRematch(entry.Value.Name);
                        break;
                    case "request-room-list":
                        RoomManager.SendRoomListTo(playerName);
                        break;
                    case "request-board-state":
                        GameSessionManager.HandleBoardRequest(playerName);
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
        }
    }
    public static void Broadcast(string message)
    {
        foreach (var socket in ConnectedPlayers.Values)
        {
            if (socket.State == WebSocketState.Open)
            {
                var payload = JsonConvert.SerializeObject(new { action = "system", msg = message });
                var bytes = Encoding.UTF8.GetBytes(payload);
                socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }
    public static void SendTo(string playerName, object payload)
    {
        if (ConnectedPlayers.TryGetValue(playerName, out var socket) && socket.State == WebSocketState.Open)
        {
            string json = JsonConvert.SerializeObject(payload);
            var bytes = Encoding.UTF8.GetBytes(json);
            socket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }

    public static IEnumerable<string> GetConnectedPlayerNames() => ConnectedPlayers.Keys;
}
