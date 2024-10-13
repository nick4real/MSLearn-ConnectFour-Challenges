using ConnectFour;
using ConnectFour.Components;
using ConnectFour.Hubs;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddSignalR();

builder.Services.AddSingleton<GameHub>();
builder.Services.AddScoped<GameState>();
builder.Services.AddScoped(sp =>
{
    var delays = new TimeSpan[]
    {
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(5)
    };
    var navMan = sp.GetRequiredService<NavigationManager>();
    var hubConnection = new HubConnectionBuilder()
        .WithUrl(navMan.ToAbsoluteUri("/hub"))
        .WithAutomaticReconnect(delays)
        .Build();

    return hubConnection;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapHub<GameHub>("/hub");

app.Run();
