using System;
using FluentValidation;
using gaton.Model;

namespace gaton.Validators;

public class RecoverPasswordValidator : AbstractValidator<RecoverPassword>
{
    public RecoverPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El correo es obligatorio.")
            .EmailAddress().WithMessage("Formato de correo no válido.")
            .When(x => string.IsNullOrEmpty(x.SecurityAnswer) && string.IsNullOrEmpty(x.NewPassword));

        RuleFor(x => x.SecurityAnswer)
            .NotEmpty().WithMessage("La respuesta de seguridad es obligatoria.")
            .When(x => !string.IsNullOrEmpty(x.NewPassword));

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("La nueva contraseña es obligatoria.")
            .Length(6, 16).WithMessage("Debe tener entre 6 y 16 caracteres.")
            .Matches(@"[A-Z]").WithMessage("Al menos una mayúscula.")
            .Matches(@"[a-z]").WithMessage("Al menos una minúscula.")
            .Matches(@"\d").WithMessage("Al menos un número.")
            .When(x => !string.IsNullOrEmpty(x.SecurityAnswer));
    }
}