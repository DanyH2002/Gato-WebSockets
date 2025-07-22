using System;
using System.Linq;
using gaton.Model;
using Microsoft.EntityFrameworkCore;
using gaton.Helpers;

namespace gaton.WebSockets;
/*
Gestion de estadisticas despues de cada partida, enviandolos a la base de datos
*/

public static class StatsManager
{
    public static void RegistrarResultado(GameSession game)
    {
        var optionsBuilder = new DbContextOptionsBuilder<PalyerContext>();
        var connectionString = ConfigHelper.GetConnectionString("GameConnection");
        optionsBuilder.UseSqlServer(connectionString);

        using var context = new PalyerContext(optionsBuilder.Options);

        foreach (var nombreJugador in game.PlayerSymbols.Keys)
        {
            var simbolo = game.PlayerSymbols[nombreJugador];
            var resultado = game.Ganador switch
            {
                "Empate" => "Empate",
                _ when simbolo == game.Ganador => "Victoria",
                _ => "Derrota"
            };

            var jugador = context.Players.FirstOrDefault(p => p.Name == nombreJugador);
            if (jugador == null) continue;

            var stats = context.PlayerStats.FirstOrDefault(s => s.PlayerId == jugador.Id);
            if (stats == null)
            {
                stats = new PlayerStats { PlayerId = jugador.Id };
                context.PlayerStats.Add(stats);
            }

            stats.PartidasJugadas++;
            if (resultado == "Victoria") stats.Victorias++;
            else if (resultado == "Empate") stats.Empates++;

            //context.SaveChanges();
        }
        context.SaveChanges();
    }
}

