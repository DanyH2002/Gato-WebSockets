using System;

namespace gaton.Model;

public class RecoverPassword
{
    public string? Email { get; set; }
    public SecurityQuestion Question { get; set; }
    public string? SecurityAnswer { get; set; }
    public string? NewPassword { get; set; }
}
