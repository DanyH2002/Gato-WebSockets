using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gaton.Model;
using FluentValidation;
using gaton.WebSockets;


namespace gaton.Pages;

public class IndexModel : PageModel
{
    private readonly PalyerContext _context;
    private readonly ILogger<IndexModel> _logger;
    private readonly IValidator<Login> _validator;
    public IndexModel(PalyerContext context, ILogger<IndexModel> logger, IValidator<Login> validator)
    {
        _context = context;
        _logger = logger;
        _validator = validator;
    }

    [BindProperty]
    public string? LoginEmail { get; set; }

    [BindProperty]
    public string? LoginPassword { get; set; }

    public string? LoginError { get; set; }

    public IActionResult OnPost()
    {

        var loginData = new Login
        {
            Email = LoginEmail,
            Password = LoginPassword
        };

        var validationResult = _validator.Validate(loginData);
        if (!validationResult.IsValid)
        {
            LoginError = string.Join("<br/>", validationResult.Errors.Select(e => e.ErrorMessage));
            return Page();
        }

        var player = _context.Players.FirstOrDefault(p => p.Email == LoginEmail && p.Password == LoginPassword);
        if (player == null)
        {
            LoginError = "Email o contraseña incorrectos.";
            return Page();
        }
        TempData["PlayerId"] = player.Id;
        TempData["PlayerName"] = player.Name;

        WebSocketClient.ConnectAsync(player.Name).Wait();
        return RedirectToPage("List");
    }
}
