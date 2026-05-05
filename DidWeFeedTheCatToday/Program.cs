using DidWeFeedTheCatToday.Configuration;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Hubs;
using DidWeFeedTheCatToday.Middleware;
using DidWeFeedTheCatToday.Services.Implementations;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Validation;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;


var builder = WebApplication.CreateBuilder(args);
var appSettings = new AppSettings();

builder.Configuration.GetSection("AppSettings").Bind(appSettings);
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ValidationFilter));
});
builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = appSettings.Issuer,
        ValidateAudience = true,
        ValidAudience = appSettings.Audience,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.Token))
    };
});
builder.Services.AddCors(policy =>
{
    policy.AddPolicy("BlazorCorsPolicy", policy =>
    {
        policy.WithOrigins(appSettings.AllowedOrigins)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
    });
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRequestContext, RequestContext>();
builder.Services.AddScoped<IAuthServices, AuthService>();
builder.Services.AddScoped<IPetService, PetService>();
builder.Services.AddScoped<IFeedingService, FeedingService>();
builder.Services.AddScoped<IPetFeedingQueryService, PetFeedingQueryService>();
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddSignalR();
builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost");
    });
});

var app = builder.Build();

using var scope = app.Services.CreateScope(); //making sure Dispose() is called

var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();

await DbSeeder.SeedAsync(dbContext);

app.UseMiddleware<RequestResponseLog>();
app.UseMiddleware<ErrorHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference();
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors("BlazorCorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<PetHub>("/pet-hub");
app.MapHealthChecks("/health");

app.Run();
