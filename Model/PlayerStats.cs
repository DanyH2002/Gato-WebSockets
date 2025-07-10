using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gaton.Model
{
    public class PlayerStats
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PlayerId { get; set; }

        public int Victorias { get; set; } = 0;
        public int Empates { get; set; } = 0;
        public int PartidasJugadas { get; set; } = 0;
        [ForeignKey("PlayerId")]
        public virtual Player? Player { get; set; }
    }
}
