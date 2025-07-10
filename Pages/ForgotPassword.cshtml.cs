using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gaton.Model;
using FluentValidation;

namespace gaton.Pages
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly PalyerContext _context;
        private readonly IValidator<RecoverPassword> _validator;

        public ForgotPasswordModel(PalyerContext context, IValidator<RecoverPassword> validator)
        {
            _context = context;
            _validator = validator;
        }
        [BindProperty]
        public RecoverPassword RecoverRequest { get; set; } = new();

        public string? ResultMessage { get; set; }

        public IActionResult OnPost()
        {
            var validation = _validator.Validate(RecoverRequest);
            if (!validation.IsValid)
            {
                ResultMessage = string.Join("<br/>", validation.Errors.Select(e => e.ErrorMessage));
                return Page();
            }

            var player = _context.Players.FirstOrDefault(p =>
                p.Email == RecoverRequest.Email &&
                p.Question == RecoverRequest.Question);
            System.Diagnostics.Debug.WriteLine($"Question: {RecoverRequest.Question}, Email: {RecoverRequest.Email}");
            System.Diagnostics.Debug.WriteLine($"Player Found: {player?.Email}, Stored Question: {player?.Question}");


            if (player == null)
            {
                ResultMessage = "Usuario no encontrado o pregunta incorrecta.";
                return Page();
            }
            bool respuestaCoincide = SimilarEnough(player.SecurityAnswer, RecoverRequest.SecurityAnswer);
            if (!respuestaCoincide)
            {
                ResultMessage = "La respuesta de seguridad no coincide.";
                return Page();
            }

            player.Password = RecoverRequest.NewPassword;
            _context.SaveChanges();

            ResultMessage = "Tu contraseña fue actualizada con éxito.";
            return RedirectToPage("List");
        }
        private bool SimilarEnough(string original, string ingreso)
        {
            if (string.IsNullOrWhiteSpace(original) || string.IsNullOrWhiteSpace(ingreso)) return false;

            var cleanOriginal = original.Trim().ToLower();
            var cleanIngreso = ingreso.Trim().ToLower();

            return GetLevenshteinDistance(cleanOriginal, cleanIngreso) <= 2;
        }
        private int GetLevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];

            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s.Length, t.Length];
        }
    }
}
