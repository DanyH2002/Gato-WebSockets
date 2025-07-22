using FluentValidation;
using Microsoft.EntityFrameworkCore;
using gaton.Model;
using gaton.Validators;
using gaton.WebSockets;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<PalyerContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("GameConnection"));
});

builder.Services.AddScoped<IValidator<Player>, PlayerValidator>();
builder.Services.AddScoped<IValidator<Login>, LoginValidator>();
builder.Services.AddScoped<IValidator<RecoverPassword>, RecoverPasswordValidator>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseWebSockets();

app.Use(async (context, next) =>
{
    if (context.Request.Path.StartsWithSegments("/ws"))
    {
        var playerName = context.Request.Path.Value?.Split("/").Last();
        if (context.WebSockets.IsWebSocketRequest && !string.IsNullOrWhiteSpace(playerName))
        {
            var socket = await context.WebSockets.AcceptWebSocketAsync();
            await WebSocketHandler.HandleAsync(socket, playerName);
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();
//gaton.WebSockets.WebSocketServerLauncher.Start();
app.Run();
