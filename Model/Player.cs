using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace gaton.Model;

public class Player
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    public string? Name { get; set; }

    [Required]
    public string? Email { get; set; }

    [Required]
    [StringLength(16, MinimumLength = 6)]
    public string? Password { get; set; }
    [Required]
    public SecurityQuestion Question { get; set; }

    [Required]
    public string? SecurityAnswer { get; set; }
}
public enum SecurityQuestion
{
    [Display(Name = "¿Cuál era el nombre de tu mascota de la infancia?")]
    MascotaDeInfancia,

    [Display(Name = "¿En qué ciudad naciste?")]
    CiudadDeNacimiento,

    [Display(Name = "¿Cuál es tu comida favorita?")]
    ComidaFavorita,

    [Display(Name = "¿Cómo se llama tu mejor amigo?")]
    NombreDelMejorAmigo
}