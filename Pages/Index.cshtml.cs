using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gaton.Model;

namespace gaton.Pages;

public class IndexModel : PageModel
{
    private readonly PalyerContext _context;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(PalyerContext context, ILogger<IndexModel> logger)
    {
        _context = context;
        _logger = logger;
    }

    [BindProperty]
    public string? LoginEmail { get; set; }

    [BindProperty]
    public string? LoginPassword { get; set; }

    public string? LoginError { get; set; }

    public IActionResult OnPost()
    {
        var player = _context.Players.FirstOrDefault(p => p.Email == LoginEmail && p.Password == LoginPassword);
        if (player == null || player.Password != LoginPassword)
        {
            LoginError = "Email o contraseña incorrectos.";
            return Page();
        }
        TempData["PlayerId"] = player.Id;
        TempData["PlayerName"] = player.Name;

        return RedirectToPage("ListaJugadores");
    }
}
