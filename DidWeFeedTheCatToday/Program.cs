using Asp.Versioning;
using DidWeFeedTheCatToday.Configuration;
using DidWeFeedTheCatToday.Data;
using DidWeFeedTheCatToday.Data.Interceptors;
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
var isTesting = builder.Environment.IsEnvironment("Testing") ||
                AppDomain.CurrentDomain.GetAssemblies().Any(a => a.FullName!.Contains("Microsoft.AspNetCore.Mvc.Testing"));
var appSettings = new AppSettings();

builder.Configuration.GetSection("AppSettings").Bind(appSettings);
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(ValidationFilter));
});
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Was The Cat Fed Today? API v1";
        return Task.CompletedTask;
    });
});

builder.Services.AddSingleton<UpdateAuditableInterceptor>();

if (isTesting)
{
    //
}
else
{
    builder.Services.AddDbContext<AppDbContext>((sp, opt) =>
    {
        var interceptor = sp.GetRequiredService<UpdateAuditableInterceptor>();
        opt.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddInterceptors(interceptor);
    });
}

var jwtKey = string.IsNullOrEmpty(appSettings.Token)
    ? "Place128CharLongTokenHereButThisWillAlsoWork111111111111111111111111111111111111111111111111111111111111111111111111111111111111"
    : appSettings.Token;

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
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
builder.Services.AddMemoryCache();
builder.Services.AddMassTransit(x =>
{
    if (isTesting)
    {
        x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
    }
    else
    {
        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(appSettings.RabbitMqHost);
            cfg.ConfigureEndpoints(context);
        });
    }
});
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new Asp.Versioning.ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

var app = builder.Build();

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
app.MapHealthChecks("/health").RequireCors("BlazorCorsPolicy");

if (!isTesting)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
    await DbSeeder.SeedAsync(dbContext);
}

app.Run();

public partial class Program { }
