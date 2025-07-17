using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using gaton.Model;
using FluentValidation;
using gaton.WebSockets;


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

        [BindProperty]
        public string? PlayerIdTemp { get; set; }

        public SecurityQuestion? PreguntaDelUsuario { get; set; }

        public bool MostrarSegundaEtapa { get; set; } = false;

        public string? ResultMessage { get; set; }

        public IActionResult OnPostBuscar()
        {
            var email = RecoverRequest.Email?.Trim().ToLower();
            var player = _context.Players.FirstOrDefault(p => p.Email.ToLower() == email);

            if (player != null)
            {
                PreguntaDelUsuario = player.Question;
                PlayerIdTemp = player.Id.ToString();
                MostrarSegundaEtapa = true;
            }
            else
            {
                ResultMessage = "Correo no encontrado.";
            }
            return Page();
        }
        public IActionResult OnPostCambiar()
        {
            if (!int.TryParse(PlayerIdTemp, out int id)) return Page();
            var player = _context.Players.FirstOrDefault(p => p.Id == id);
            if (player == null || !SimilarEnough(player.SecurityAnswer, RecoverRequest.SecurityAnswer ?? ""))
            {
                ResultMessage = "La respuesta no coincide.";
                return Page();
            }
            var validation = _validator.Validate(RecoverRequest);
            if (!validation.IsValid)
            {
                ResultMessage = string.Join("<br/>", validation.Errors.Select(e => e.ErrorMessage));
                return Page();
            }

            player.Password = RecoverRequest.NewPassword;
            _context.SaveChanges();

            TempData["PlayerId"] = player.Id;
            TempData["PlayerName"] = player.Name;

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
