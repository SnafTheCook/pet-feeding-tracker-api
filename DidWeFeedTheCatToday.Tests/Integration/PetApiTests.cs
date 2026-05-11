using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace DidWeFeedTheCatToday.Tests.Integration
{
    public class PetApiTests(WebApplicationFactory<Program> factory) : IntegrationTestBase(factory)
    {
        [Fact]
        public async Task PostPet_WithValidData_ReturnsCreated()
        {
            var request = new { Name = "Meowstarion", Age = 2 };

            var response = await _httpClient.PostAsJsonAsync("api/pets", request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        }

        [Fact]
        public async Task PostPet_WithInvalidData_ReturnsBadRequest()
        {
            var request = new { Name = "Meow", Age = 2 };

            var response = await _httpClient.PostAsJsonAsync("api/pets", request);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}
