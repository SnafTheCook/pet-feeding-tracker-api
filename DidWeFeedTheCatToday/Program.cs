using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Middleware;
using DidWeFeedTheCatToday.Services.Implementations;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using System.Text;


var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ValidationFilter));
});

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["AppSettings:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["AppSettings:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!))
    };
});

builder.Services.AddCors(policy =>
{
    policy.AddPolicy("BlazorCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://localhost:7200", "http://localhost:5240")
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

var app = builder.Build();

using var scope = app.Services.CreateScope(); //making sure Dispose() is called

var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
dbContext.Database.Migrate();

await DbSeeder.SeedAsync(dbContext);

app.UseMiddleware<RequestResponseLog>();
app.UseMiddleware<ErrorHandlerMiddleware>();

// Configure the HTTP request pipeline.
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

app.Run();
