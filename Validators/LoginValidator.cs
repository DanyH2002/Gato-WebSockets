using System;
using FluentValidation;
using gaton.Model;

namespace gaton.Validators;

public class LoginValidator : AbstractValidator<Login>
{
    public LoginValidator()
    {
        RuleFor(login => login.Email)
            .NotEmpty().WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress().WithMessage("El correo electrónico no es válido.");

        RuleFor(login => login.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .Length(6, 16).WithMessage("La contraseña debe tener entre 6 y 16 caracteres.")
            .Matches(@"[A-Z]").WithMessage("Debe tener al menos una letra mayúscula")
            .Matches(@"[a-z]").WithMessage("Debe tener al menos una letra minúscula")
            .Matches(@"\d").WithMessage("Debe contener al menos un número");
    }
}
