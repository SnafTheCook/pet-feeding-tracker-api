using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using DidWeFeedTheCatToday;
using Microsoft.EntityFrameworkCore;
using DidWeFeedTheCatToday.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using System.Data.Common;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using MassTransit;

namespace DidWeFeedTheCatToday.Tests.Integration
{
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        protected readonly HttpClient _httpClient;
        private readonly DbConnection _connection;

        public IntegrationTestBase(WebApplicationFactory<Program> webApplicationFactory)
        {
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var testFactory = webApplicationFactory.WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                builder.UseSetting("AppSettings:Token", "Place128CharLongTokenHereButThisWillAlsoWork111111111111111111111111111111111111111111111111111111111111111111111111111111111111");
                builder.UseSetting("AppSettings:Issuer", "Test");
                builder.UseSetting("AppSettings:Audience", "Test");
                builder.UseSetting("AppSettings:AllowedOrigins:0", "http://localhost");

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);
                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

                    var massTransitServices = services.Where(d =>
                        d.ServiceType.FullName?.Contains("MassTransit") == true ||
                        d.ImplementationType?.FullName?.Contains("MassTransit") == true).ToList();

                    foreach (var service in massTransitServices) services.Remove(service);

                    services.AddMassTransit(x =>
                    {
                        x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                });
            });

            _httpClient = testFactory.CreateClient();
        }

        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
