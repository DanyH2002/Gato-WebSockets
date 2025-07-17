using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gaton.Model;

public class LobbyModel : PageModel
{
    private readonly PalyerContext _context;
    public string NombreJugadorActual { get; set; } = "Invitado";
    public List<PlayerStats> Ranking { get; set; } = new();
    public LobbyModel(PalyerContext context)
    {
        _context = context;
    }
    public void OnGet()
    {
        var playerId = Convert.ToInt32(TempData["PlayerId"]);
        var player = _context.Players.FirstOrDefault(p => p.Id == playerId);

        if (player != null)
            NombreJugadorActual = player.Name;
        Ranking = _context.PlayerStats
            .Include(s => s.Player)
            .OrderByDescending(s => s.Victorias)
            .ToList();
    }
}
