using System;
using gaton.Model;
using gaton.WebSockets;
using System.Collections.Generic;
using System.Linq;
using System.Timers;

namespace gaton.WebSockets;
/*
Control de turnos, timer del juego, y union con la logica de juego.
*/
public class GameSessionManager
{
    private static readonly Dictionary<string, GameSession> ActiveGames = new();
    private static readonly Dictionary<string, System.Timers.Timer> TurnTimers = new();
    public static void StartGame(string roomId, List<string> players)
    {
        if (players.Count != 2)
        {
            return;
        }
        var game = new GameSession
        {
            RoomId = roomId,
            Tablero = Enumerable.Repeat("", 9).ToArray(),
            Turno = "X",
            PlayerSymbols = new Dictionary<string, string>
            {
                { players[0], "X" },
                { players[1], "O" }
            }
        };
        ActiveGames[roomId] = game;
        StartTurnTimer(roomId);
        BroadcastGameState(roomId);
        Console.WriteLine($"Partida iniciada en la sala: {roomId}");
        foreach (var jugador in players)
        {
            Console.WriteLine($"Enviando game-started a: {jugador}");
            WebSocketHandler.SendTo(jugador, new
            {
                action = "game-started",
                msg = "Partida iniciada. ¡Buena suerte!"
            });
        }
    }
    public static void HandleMove(string playerName, int casilla)
    {
        var roomId = RoomManager.GetRoomIdOfPlayer(playerName);
        if (roomId == null || !ActiveGames.ContainsKey(roomId)) return;

        var game = ActiveGames[roomId];
        if (game.Ganador != null || game.Tablero[casilla] != "") return;

        if (GetSymbolByPlayer(playerName, game) != game.Turno) return;

        game.Tablero[casilla] = game.Turno;
        game.Ganador = VerificarGanador(game.Tablero);

        if (game.Ganador == null)
        {
            game.Turno = game.Turno == "X" ? "O" : "X";
            StartTurnTimer(roomId);
        }
        else
        {
            EndGame(roomId);
        }

        BroadcastGameState(roomId);
    }
    private static void EndGame(string roomId)
    {
        if (!ActiveGames.ContainsKey(roomId)) return;

        var game = ActiveGames[roomId];
        StopTurnTimer(roomId);
        StatsManager.RegistrarResultado(game);

        foreach (var jugador in game.PlayerSymbols.Keys)
        {
            WebSocketHandler.SendTo(jugador, new
            {
                action = "endgame",
                ganador = game.Ganador,
                msg = game.Ganador == "Empate"
                    ? "🤝 ¡Empate!"
                    : $"🎉 ¡Ganó {game.Ganador}!"
            });
        }
    }
    public static void ForceVictory(string playerName, string roomId)
    {
        var game = ActiveGames.ContainsKey(roomId) ? ActiveGames[roomId] : null;
        if (game == null) return;
        var jugadoresActivos = game.PlayerSymbols.Keys.Where(p =>
        p != playerName && RoomManager.IsPlayerInRoom(p)).ToList();
        if (jugadoresActivos.Count == 1)
        {
            var simboloGanador = GetSymbolByPlayer(jugadoresActivos[0], game);
            game.Ganador = simboloGanador ?? jugadoresActivos[0];
            StopTurnTimer(roomId);
            StatsManager.RegistrarResultado(game);
            foreach (var jugador in game.PlayerSymbols.Keys)
            {
                WebSocketHandler.SendTo(jugador, new
                {
                    action = "endgame",
                    ganador = game.Ganador,
                    msg = $"🏆 ¡Victoria automática para {game.Ganador} por abandono del oponente!"
                });
            }
        }
    }

    //** Metodo complementarios
    public static void BroadcastGameState(string roomId)
    {
        var game = ActiveGames[roomId];
        foreach (var jugador in game.PlayerSymbols.Keys)
        {
            Console.WriteLine($"[PlayerSymbols] {jugador} = {game.PlayerSymbols[jugador]}");
            WebSocketHandler.SendTo(jugador, new
            {
                action = "update-board",
                tablero = game.Tablero,
                turno = game.Turno,
                ganador = game.Ganador,
                simbolo = game.PlayerSymbols[jugador],
                playerNames = game.PlayerSymbols.Keys.ToList(),
                tableroSimbolos = game.PlayerSymbols,
                roomId = game.RoomId
            });
        }
        var jugadorDelTurno = GetPlayerNameBySymbol(game.Turno, game);
        Console.WriteLine($"Juego actualizado en la sala: {roomId}, turno de {game.Turno} para {jugadorDelTurno}, ganador: {game.Ganador}");
    }
    private static string? VerificarGanador(string[] tab)
    {
        int[][] jugadas = new int[][]
        {
            new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },
            new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },
            new[] { 0, 4, 8 }, new[] { 2, 4, 6 }
        };

        foreach (var j in jugadas)
        {
            string a = tab[j[0]], b = tab[j[1]], c = tab[j[2]];
            if (!string.IsNullOrEmpty(a) && a == b && b == c)
                return a;
        }

        if (tab.All(c => !string.IsNullOrEmpty(c)))
            return "Empate";

        return null;
    }
    private static string? GetPlayerNameBySymbol(string simbolo, GameSession game)
    {
        return game.PlayerSymbols.FirstOrDefault(p => p.Value == simbolo).Key;
    }
    private static string? GetSymbolByPlayer(string playerName, GameSession game)
    {
        return game.PlayerSymbols.TryGetValue(playerName, out var symbol) ? symbol : null;
    }

    public static void HandleBoardRequest(string playerName)
    {
        var roomId = RoomManager.GetRoomIdOfPlayer(playerName);
        if (roomId != null && ActiveGames.ContainsKey(roomId))
        {
            BroadcastGameState(roomId);
        }
    }

    //** Timer por turno
    private static void StartTurnTimer(string roomId)
    {
        StopTurnTimer(roomId);
        var timer = new System.Timers.Timer(15000); // 15 segundos
        timer.Elapsed += (sender, e) =>
        {
            if (!ActiveGames.ContainsKey(roomId)) return;

            var game = ActiveGames[roomId];
            string currentTurn = game.Turno;
            string? jugador = GetPlayerNameBySymbol(currentTurn, game);


            if (jugador != null)
            {
                WebSocketHandler.SendTo(jugador, new
                {
                    action = "timeout",
                    msg = "⏱ Se agotó tu tiempo. Turno perdido."
                });

                game.Turno = currentTurn == "X" ? "O" : "X";
                BroadcastGameState(roomId);
                StartTurnTimer(roomId);
            }
        };

        timer.AutoReset = false;
        timer.Start();
        TurnTimers[roomId] = timer;
    }
    private static void StopTurnTimer(string roomId)
    {
        if (TurnTimers.TryGetValue(roomId, out var timer))
        {
            timer.Stop();
            timer.Dispose();
            TurnTimers.Remove(roomId);
        }
    }
    //** Revancha
    public static void RequestRematch(string playerName)
    {
        var roomId = RoomManager.GetRoomIdOfPlayer(playerName);
        if (roomId == null || !ActiveGames.ContainsKey(roomId)) return;

        var game = ActiveGames[roomId];
        game.JugadoresQueAceptaronRevancha.Add(playerName);

        foreach (var jugador in game.PlayerSymbols.Keys)
        {
            WebSocketHandler.SendTo(jugador, new
            {
                action = "revancha-status",
                jugadores = game.JugadoresQueAceptaronRevancha.Count,
                total = game.PlayerSymbols.Count
            });
        }
        if (game.JugadoresQueAceptaronRevancha.Count == game.PlayerSymbols.Count)
        {
            // reinicia partida
            game.Tablero = Enumerable.Repeat("", 9).ToArray();
            game.Turno = "X";
            game.Ganador = null;
            game.JugadoresQueAceptaronRevancha.Clear();

            BroadcastGameState(roomId);
            StartTurnTimer(roomId);
        }
    }
    public static void RejectRematch(string playerName)
    {
        var roomId = RoomManager.GetRoomIdOfPlayer(playerName);
        WebSocketHandler.SendTo(playerName, new
        {
            action = "revancha-rechazada",
            msg = "Has salido de la sala. ¡Puedes unirte a otra partida!"
        });
        RoomManager.HandleLeave(playerName, roomId);
        RoomManager.SendRoomListTo(playerName);
    }
}
