using DidWeFeedTheCatToday.Controllers;
using DidWeFeedTheCatToday.Features.Pets;
using DidWeFeedTheCatToday.Features.Pets.Commands;
using DidWeFeedTheCatToday.Features.Pets.Queries;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DidWeFeedTheCatToday.Tests.Controllers
{
    public class PetControllerTests
    {
        private readonly Mock<IMediator> _mockMediator;
        private readonly PetController _petController;

        public PetControllerTests()
        {
            _mockMediator = new Mock<IMediator>();
            _petController = new PetController(_mockMediator.Object);
        }

        [Fact]
        public async Task GetPetById_WhenPetExists_ReturnsOkWithData()
        {
            var petId = 1;
            var petDto = new GetPetDTO { Id = petId, Name = "Meowstarion" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPetByIdQuery>(), It.IsAny<CancellationToken>()))
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

            _mockMediator
                .Setup(m => m.Send(It.IsAny<GetPetByIdQuery>(), It.IsAny<CancellationToken>()))
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

            _mockMediator
                .Setup(m => m.Send(It.IsAny<AddPetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(returnedPet);

            var result = await _petController.PostPet(testPet);
            var okResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
            var apiResponse = okResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeTrue();
            apiResponse.Data.Should().NotBeNull();
            apiResponse.Data.Id.Should().Be(returnedPet.Id);
            apiResponse.Data.Name.Should().Be(returnedPet.Name);

            _mockMediator.Verify(m => m.Send(It.IsAny<AddPetCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PutPet_WhenPetOverrode_ReturnsNoContent()
        {
            int testId = 1;
            var testPet = new CommandPetDTO { Name = "Meowstaroin" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceResult.Ok());

            var result = await _petController.PutPet(testId, testPet);
            result.Should().BeOfType<NoContentResult>();
        }

        [Fact]
        public async Task PutPet_WhenPathNotFound_ReturnsNotFound()
        {
            int testId = 9999;
            var testPet = new CommandPetDTO { Name = "Meowstaroin" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceResult.Fail(ServiceResultError.NotFound));

            var result = await _petController.PutPet(testId, testPet);
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var apiResponse = notFoundResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeFalse();
            apiResponse.Error.Should().Be("Pet not found.");

            _mockMediator.Verify(m => m.Send(It.IsAny<UpdatePetCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task PutPet_WhenConflicted_ReturnsConcurrencyConflict()
        {
            int testId = 1;
            var testPet = new CommandPetDTO { Name = "Meowstaroin" };

            _mockMediator
                .Setup(m => m.Send(It.IsAny<UpdatePetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ServiceResult.Fail(ServiceResultError.ConcurrencyConflict));

            var result = await _petController.PutPet(testId, testPet);
            var concConflictResult = result.Should().BeOfType<ConflictObjectResult>().Subject;
            var apiResponse = concConflictResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeFalse();
            apiResponse.Error.Should().Be("Pet was changed since last request.");

            _mockMediator.Verify(m => m.Send(It.IsAny<UpdatePetCommand>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task DeletePet_WhenPetDeleted_ReturnsNoContent()
        {
            int testId = 1;

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _petController.DeletePet(testId);
            result.Should().BeOfType<NoContentResult>();
        }


        [Fact]
        public async Task DeletePet_WhenPetNotFound_ReturnsNotFound()
        {
            int testId = 9999;

            _mockMediator
                .Setup(m => m.Send(It.IsAny<DeletePetCommand>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _petController.DeletePet(testId);
            var notFoundResult = result.Should().BeOfType<NotFoundObjectResult>().Subject;
            var apiResponse = notFoundResult.Value.Should().BeOfType<ApiResponse<GetPetDTO>>().Subject;

            apiResponse.Success.Should().BeFalse();
            apiResponse.Error.Should().Be("Pet not found.");
        }
    }
}
