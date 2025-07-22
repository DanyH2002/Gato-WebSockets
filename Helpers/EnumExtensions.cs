using System;
using System.ComponentModel.DataAnnotations;

namespace gaton.Helpers;
/*
Esta clase proporciona una extensión para los tipos Enum que permite obtener nombres legibles a partir de los valores
*/
public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        var display = enumValue.GetType()
        .GetField(enumValue.ToString())
        ?.GetCustomAttributes(typeof(DisplayAttribute), false)
        .FirstOrDefault() as DisplayAttribute;

        return display?.Name ?? enumValue.ToString();
    }
}
