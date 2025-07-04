using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace gaton.Pages
{
    public class GatoModel : PageModel
    {
        [TempData]
        public string tableroJson { get; set; } = "";

        [BindProperty]
        public int casillaSeleccionada { get; set; }

        [BindProperty]
        public string ganador { get; set; } = "";

        [BindProperty]
        public string turno { get; set; } = "X";

        public List<string> tablero { get; set; } = new();

        public void OnGet()
        {
            tablero = Enumerable.Repeat("", 9).ToList();
            turno = "X";
            tableroJson = JsonSerializer.Serialize(tablero);
        }

        public void OnPost()
        {
            if (string.IsNullOrEmpty(tableroJson))
            {
                tablero = Enumerable.Repeat("", 9).ToList();
            }
            else
            {
                tablero =
                    JsonSerializer.Deserialize<List<string>>(tableroJson)
                    ?? Enumerable.Repeat("", 9).ToList();
            }
            if (!string.IsNullOrEmpty(ganador))
            {
                return;
            }
            if (tablero[casillaSeleccionada] == "")
            {
                tablero[casillaSeleccionada] = turno;
                ganador = verificarGanador(tablero);

                if (string.IsNullOrEmpty(ganador))
                {
                    turno = turno == "X" ? "O" : "X";
                }
            }
            tableroJson = JsonSerializer.Serialize(tablero);
        }

        public string verificarGanador(List<string> tab)
        {
            int[][] jugadas = new int[][]
            {
                new[] { 0, 1, 2 },
                new[] { 3, 4, 5 },
                new[] { 6, 7, 8 },
                new[] { 0, 3, 6 },
                new[] { 1, 4, 7 },
                new[] { 2, 5, 8 },
                new[] { 0, 4, 8 },
                new[] { 2, 4, 6 },
            };

            foreach (var jGanadora in jugadas)
            {
                string a = tab[jGanadora[0]];
                string b = tab[jGanadora[1]];
                string c = tab[jGanadora[2]];

                if (!string.IsNullOrEmpty(a) && a == b && b == c)
                {
                    return a;
                }
                else if (
                    !string.IsNullOrEmpty(tab[0])
                    && !string.IsNullOrEmpty(tab[1])
                    && !string.IsNullOrEmpty(tab[2])
                    && !string.IsNullOrEmpty(tab[3])
                    && !string.IsNullOrEmpty(tab[4])
                    && !string.IsNullOrEmpty(tab[5])
                    && !string.IsNullOrEmpty(tab[6])
                    && !string.IsNullOrEmpty(tab[7])
                    && !string.IsNullOrEmpty(tab[8])
                )
                {
                    return "Empate";
                }
            }
            ;
            return "";
        }

        public IActionResult OnPostReiniciar()
        {
            tablero = Enumerable.Repeat("", 9).ToList();
            turno = "X";
            ganador = "";
            tableroJson = JsonSerializer.Serialize(tablero);

            return RedirectToPage();
        }
    }
}
