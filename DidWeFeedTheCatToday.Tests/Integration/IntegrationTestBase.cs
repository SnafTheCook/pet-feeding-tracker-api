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
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
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

                builder.ConfigureAppConfiguration((context, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["AppSettings:Token"] = "Place128CharLongTokenHereButThisWillAlsoWork111111111111111111111111111111111111111111111111111111111111111111111111111111111111",
                        ["AppSettings:Issuer"] = "TestIssuer",
                        ["AppSettings:Audience"] = "TestAudience",
                        ["AppSettings:AllowedOrigins:0"] = "http://localhost"
                    });
                });

                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlite(_connection);
                    });

                    var massTransitDescriptors = services.Where(d => d.ServiceType.Namespace?.Contains("MassTransit") ?? false).ToList();
                    foreach (var d in massTransitDescriptors) services.Remove(d);

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

            _httpClient = webApplicationFactory.CreateClient();
        }
        public void Dispose()
        {
            _connection.Close();
            _connection.Dispose();
        }
    }
}
