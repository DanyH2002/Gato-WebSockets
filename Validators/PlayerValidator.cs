using System;
using FluentValidation;
using gaton.Model;

namespace gaton.Validators;

public class PlayerValidator : AbstractValidator<Player>
{
    public PlayerValidator(PalyerContext context)
    {
        RuleFor(player => player.Name)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .Length(1, 50).WithMessage("El nombre debe tener entre 1 y 50 caracteres.");

        RuleFor(player => player.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.")
            .Must(email => !context.Players.Any(p => p.Email == email))
            .WithMessage("Este correo ya está registrado.");

        RuleFor(player => player.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .Length(6, 16).WithMessage("La contraseña debe tener entre 6 y 16 caracteres.")
            .Matches(@"[A-Z]").WithMessage("Debe tener al menos una letra mayúscula")
            .Matches(@"[a-z]").WithMessage("Debe tener al menos una letra minúscula")
            .Matches(@"\d").WithMessage("Debe contener al menos un número");

        RuleFor(player => player.Question)
            .IsInEnum().WithMessage("La pregunta de seguridad es obligatoria.");
            //.NotEmpty().WithMessage("La pregunta de seguridad es obligatoria.");

        RuleFor(player => player.SecurityAnswer)
            .NotEmpty().WithMessage("La respuesta de seguridad es obligatoria.");
    }
}
