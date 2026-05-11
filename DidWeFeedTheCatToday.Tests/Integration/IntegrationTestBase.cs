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

namespace DidWeFeedTheCatToday.Tests.Integration
{
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly HttpClient _httpClient;

        public IntegrationTestBase(WebApplicationFactory<Program> webApplicationFactory)
        {
            var testFactory = webApplicationFactory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options =>
                    {
                        options.UseSqlite("Data Source=:memory:");
                    });

                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    db.Database.OpenConnection();
                    db.Database.EnsureCreated();
                });
            });

            _httpClient = webApplicationFactory.CreateClient();
        }
    }
}
