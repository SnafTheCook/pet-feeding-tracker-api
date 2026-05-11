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

                builder.ConfigureServices(services =>
                {
                    services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));

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
