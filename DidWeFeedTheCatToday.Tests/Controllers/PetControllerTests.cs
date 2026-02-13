using DidWeFeedTheCatToday.Controllers;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Controllers
{
    public class PetControllerTests
    {
        private readonly Mock<IPetService> _mockPetService;
        private readonly PetController _petController;

        public PetControllerTests()
        {
            _mockPetService = new Mock<IPetService>();
            _petController = new PetController(_mockPetService.Object);
        }

        [Fact]
        public async Task GetPetById_WhenPetExists_ReturnsOkWithData()
        {
            var petId = 1;
            var petDto = new GetPetDTO { Id = petId, Name = "Meowstarion" };

            _mockPetService
                .Setup(setup => setup.GetPetByIdAsync(petId))
                .ReturnsAsync(petDto);

            var result = await _petController.GetPetById(petId);

            var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().BeEquivalentTo(petDto);
        }

        [Fact]
        public async Task GetPetById_WhenPetDoesntExist_ReturnsNotFound()
        {
            var petId = 9999;

            _mockPetService
                .Setup(setup => setup.GetPetByIdAsync(petId))
                .ReturnsAsync((GetPetDTO?) null);

            var result = await _petController.GetPetById(petId);
            var resultNotFound = result.Result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var apiResponse = resultNotFound.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeFalse();
            apiResponse.Error.Should().Be("No Pet found under index.");
        }

        [Fact]
        public async Task PostPet_WhenPetPosted_ReturnsOk()
        {
            var testPet = new CommandPetDTO { Name = "MeowstarionPost" };
            var returnedPet = new GetPetDTO { Name = "MeowstarionGet", Id = 1 };

            _mockPetService
                .Setup(setup => setup.AddPetAsync(testPet))
                .ReturnsAsync(returnedPet);

            var result = await _petController.PostPet(testPet);
            var okResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data.Id.Should().Be(returnedPet.Id);
            apiResponse.Data.Name.Should().Be(returnedPet.Name);

            _mockPetService.Verify(v => v.AddPetAsync(testPet), Times.Once);
        }

        [Fact]
        public async Task PutPet_WhenPetOverrode_ReturnsNoContent()
        {
            int testId = 1;
            var testPet = new CommandPetDTO { Name = "Meowstaroin" };

            _mockPetService
                .Setup(setup => setup.OverridePetAsync(testId, testPet))
                .ReturnsAsync(ServiceResult.Ok());

            var result = await _petController.PutPet(testId, testPet);
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task PutPet_WhenPathNotFound_ReturnsNotFound()
        {
            int testId = 9999;
            var testPet = new CommandPetDTO { Name = "Meowstaroin" };

            _mockPetService
                .Setup(setup => setup.OverridePetAsync(testId, testPet))
                .ReturnsAsync(ServiceResult.Fail(ServiceResultError.NotFound));

            var result = await _petController.PutPet(testId, testPet);
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var apiResponse = notFoundResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeFalse();
            apiResponse.Error.Should().Be("Pet not found.");

            _mockPetService.Verify(v => v.OverridePetAsync(testId, testPet), Times.Once);
        }

        [Fact]
        public async Task PutPet_WhenConflicted_ReturnsConcurrencyConflict()
        {
            int testId = 1;
            var testPet = new CommandPetDTO { Name = "Meowstaroin" };

            _mockPetService
                .Setup(setup => setup.OverridePetAsync(testId, testPet))
                .ReturnsAsync(ServiceResult.Fail(ServiceResultError.ConcurrencyConflict));

            var result = await _petController.PutPet(testId, testPet);
            var concConflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            var apiResponse = concConflictResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeFalse();
            apiResponse.Error.Should().Be("Pet was changed since last request.");

            _mockPetService.Verify(v => v.OverridePetAsync(testId, testPet), Times.Once);
        }

        [Fact]
        public async Task DeletePet_WhenPetDeleted_ReturnsNoContent()
        {
            int testId = 1;

            _mockPetService
                .Setup(setup => setup.DeletePetAsync(testId))
                .ReturnsAsync(true);

            var result = await _petController.DeletePet(testId);
            result.Should().BeOfType<NoContentResult>();
        }


        [Fact]
        public async Task DeletePet_WhenPetNotFound_ReturnsNotFound()
        {
            int testId = 9999;

            _mockPetService
                .Setup(setup => setup.DeletePetAsync(testId))
                .ReturnsAsync(false);

            var result = await _petController.DeletePet(testId);
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var apiResponse = notFoundResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeFalse();
            apiResponse.Error.Should().Be("Pet not found.");
        }
    }
}
