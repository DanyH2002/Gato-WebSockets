using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gaton.Model;
using FluentValidation;
using gaton.WebSockets;

namespace gaton.Pages
{
    public class RegisterModel : PageModel
    {
        private readonly PalyerContext _context;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IValidator<Player> _validator;
        public RegisterModel(PalyerContext context, ILogger<RegisterModel> logger, IValidator<Player> validator)
        {
            _context = context;
            _logger = logger;
            _validator = validator;
        }
        [BindProperty]
        public string? Name { get; set; }
        [BindProperty]
        public string? Email { get; set; }
        [BindProperty]
        public string? Password { get; set; }
        [BindProperty]
        public SecurityQuestion Question { get; set; }
        [BindProperty]
        public string? SecurityAnswer { get; set; }
        public string? RegisterError { get; set; }
        public IActionResult OnPost()
        {
            var player = new Player
            {
                Name = Name,
                Email = Email,
                Password = Password,
                Question = Question,
                SecurityAnswer = SecurityAnswer
            };

            var validationResult = _validator.Validate(player);
            if (!validationResult.IsValid)
            {
                RegisterError = string.Join("<br/>", validationResult.Errors.Select(e => e.ErrorMessage));
                return Page();
            }

            _context.Players.Add(player);
            _context.SaveChanges();

            TempData["PlayerId"] = player.Id;
            TempData["PlayerName"] = player.Name;

            return RedirectToPage("List");
        }
    }
}
