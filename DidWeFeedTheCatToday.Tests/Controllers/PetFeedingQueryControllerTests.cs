using DidWeFeedTheCatToday.Controllers;
using DidWeFeedTheCatToday.Features.PetFeedings.Queries;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Controllers
{
    public class PetFeedingQueryControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PetFeedingQueryController _controller;

        public PetFeedingQueryControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _controller = new PetFeedingQueryController(_mockMediator.Object);
        }

        private List<GetPetFeedingDTO> GetTestDataSetup()
        {
            return new List<GetPetFeedingDTO>
            {
                new GetPetFeedingDTO
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
                },

                new GetPetFeedingDTO
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
                }
            };
        }

        [Fact]
        public async Task GetPetsWithFeedings_WhenPetsExist_ReturnsOKWithData()
        {
            var testPets = GetTestDataSetup();

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPetFeedingsQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPets);

            var result = await _controller.GetPetsWithFeedings();

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<IEnumerable<GetPetFeedingDTO>>>().Subject;

            var responseData = apiResponse.Data.Should().NotBeNull().And.Subject.ToList();
            responseData.Should().HaveCount(2);
            responseData[0].FeedingTimes.Should().NotBeEmpty();
            responseData[1].FeedingTimes[0].Id.Should().Be(4);
        }

        [Fact]
        public async Task GetPetsWithFeedingsById_WhenPetFound_ReturnsOkWithData()
        {
            var testPet = GetTestDataSetup()[0];
            int testId = 1;

            _mockMediator
                .Setup(m => m.Send(It.Is<GetPetFeedingsByIdQuery>(q => q.Id == testId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(testPet);

            var result = await _controller.GetPetsWithFeedingsById(testId);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<GetPetFeedingDTO>>().Subject;

            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data.Id.Should().Be(testId);
            apiResponse.Data.FeedingTimes.Should().NotBeNull();
        }

        [Fact]
        public async Task GetPetsWithFeedingsById_WhenPetNotFound_ReturnsNotFound()
        {
            int testId = 1;

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPetFeedingsByIdQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((GetPetFeedingDTO?)null);

            var result = await _controller.GetPetsWithFeedingsById(testId);

            var notFoundResult = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var apiResponse = notFoundResult.Value.Should().BeOfType<ApiResponse<GetPetFeedingDTO>>().Subject;

            apiResponse.Data.Should().BeNull();
            apiResponse.Error.Should().Be("Pet and its feeding times were not found.");
        }
    }
}
