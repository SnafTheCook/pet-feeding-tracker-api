using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using DidWeFeedTheCatToday;

namespace DidWeFeedTheCatToday.Tests.Integration
{
    public class IntegrationTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly HttpClient _httpClient;

        public IntegrationTestBase(WebApplicationFactory<Program> webApplicationFactory)
        {
            _httpClient = webApplicationFactory.CreateClient();
        }
    }
}
