using System;

namespace gaton.Model;

public class GameSession
{
    public string RoomId { get; set; } = "";
    public string[] Tablero { get; set; } = Enumerable.Repeat("", 9).ToArray();
    public string Turno { get; set; } = "X";
    public string? Ganador { get; set; }
    public Dictionary<string, string> PlayerSymbols { get; set; } = new(); // jugador → simbolo
    public HashSet<string> JugadoresQueAceptaronRevancha { get; set; } = new(); // coleccion de jugadores  (O(1))
}
