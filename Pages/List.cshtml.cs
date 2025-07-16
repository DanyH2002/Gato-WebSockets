using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using gaton.Model;

public class LobbyModel : PageModel
{
    private readonly PalyerContext _context;
    public string NombreJugadorActual { get; set; } = "Invitado";

    public LobbyModel(PalyerContext context)
    {
        _context = context;
    }
    public List<PlayerStats> Ranking { get; set; } = new();
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
    public IActionResult OnPostUnirse1()
    {
        TempData["Mensaje"] = "¡Te uniste a Room #1!";
        return RedirectToPage("/Gato"); // Simula entrada
    }

    public IActionResult OnPostCrearRoom()
    {
        TempData["Mensaje"] = "¡Sala creada exitosamente!";
        return RedirectToPage("/Gato"); // En el futuro rediriges con RoomID
    }
}
