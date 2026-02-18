using DidWeFeedTheCatToday.Client;
using DidWeFeedTheCatToday.Client.Handlers;
using DidWeFeedTheCatToday.Client.Providers;
using DidWeFeedTheCatToday.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddTransient<JwtAuthorizationHandler>();
builder.Services.AddHttpClient("ServerApi", client =>
    client.BaseAddress = new Uri("http://localhost:8080/"))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();
builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerApi"));

builder.Services.AddScoped<PetService>();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddScoped<AuthService>();

await builder.Build().RunAsync();
