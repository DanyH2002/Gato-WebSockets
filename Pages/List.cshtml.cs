using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc;

public class LobbyModel : PageModel
{
    public void OnGet()
    {
    }

    public IActionResult OnPost()
    {
        // Simula redirección al juego
        return RedirectToPage("/Gato");
    }
}
