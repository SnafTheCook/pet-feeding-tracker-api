using DidWeFeedTheCatToday.Common;
using DidWeFeedTheCatToday.Controllers;
using DidWeFeedTheCatToday.DTOs.Feedings;
using DidWeFeedTheCatToday.DTOs.PetFeedings;
using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DidWeFeedTheCatToday.Tests.Controllers
{
    public class PetFeedingQueryControllerTests
    {
        private readonly Mock<IPetFeedingQueryService> _mockPetFeedingQueryService;
        private readonly PetFeedingQueryController _controller;

        public PetFeedingQueryControllerTests()
        {
            _mockPetFeedingQueryService = new Mock<IPetFeedingQueryService>();
            _controller = new PetFeedingQueryController(_mockPetFeedingQueryService.Object);
        }

        [Fact]
        public async Task GetPetsWithFeedings_WhenPetsExist_ReturnsOKWithData()
        {
            var testPet1 = new GetPetFeedingDTO
            {
                Id = 1,
                Name = "Meowstarion ",
                FeedingTimes = new List<GetFeedingDTO>
                {
                    new GetFeedingDTO
                    {
                        Id = 1,
                        FeedingTime = DateTime.UtcNow,
                        PetId = 1,
                    },
                    new GetFeedingDTO
                    {
                        Id = 2,
                        FeedingTime = DateTime.UtcNow,
                        PetId = 1,
                    },
                    new GetFeedingDTO
                    {
                        Id = 3,
                        FeedingTime = DateTime.UtcNow,
                        PetId = 1,
                    }
                }
            };

            var testPet2 = new GetPetFeedingDTO
            {
                Id = 2,
                Name = "Katlach",
                FeedingTimes = new List<GetFeedingDTO>
                {
                    new GetFeedingDTO
                    {
                        Id = 4,
                        FeedingTime = DateTime.UtcNow,
                        PetId = 2,
                    },
                    new GetFeedingDTO
                    {
                        Id = 5,
                        FeedingTime = DateTime.UtcNow,
                        PetId = 2,
                    },
                    new GetFeedingDTO
                    {
                        Id = 6,
                        FeedingTime = DateTime.UtcNow,
                        PetId = 2,
                    }
                }
            };

            _mockPetFeedingQueryService
                .Setup(setup => setup.GetAllPetFeedingsAsync())
                .ReturnsAsync(new List<GetPetFeedingDTO> { testPet1, testPet2 });

            var result = await _controller.GetPetsWithFeedings();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<GetPetFeedingDTO>>>().Subject;

            var responseData = apiResponse.Data.Should().NotBeNull().And.Subject.ToList();
            responseData.Should().HaveCount(2);
            responseData[0].FeedingTimes.Should().NotBeEmpty();
            responseData[1].FeedingTimes[0].Id.Should().Be(4);
        }
    }
}
