using System;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

namespace gaton.Helpers;
/*
Esta clase  simplifica el acceso a la conexion configurada en appsettings.json
*/
public static class ConfigHelper
{
    public static string GetConnectionString(string name)
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false);

        var config = builder.Build();
        var connectionString = config.GetConnectionString(name);

        if (string.IsNullOrEmpty(connectionString))
            throw new Exception($"⚠️ No se encontró la cadena de conexión con el nombre '{name}'.");

        return connectionString;
    }
}
