using DidWeFeedTheCatToday.Entities;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;
using DidWeFeedTheCatToday.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DidWeFeedTheCatToday.Controllers
{
    [Route("api/pet-feedings")]
    [ApiController]
    public class PetFeedingQueryController(IPetFeedingQueryService petFeedingQueryService) : ControllerBase
    {
        /// <summary>
        /// Retrieve all pets and their respective feeding histories.
        /// </summary>
        /// <returns>API response containing a collection of <see cref="GetPetFeedingDTO"/> and their nested feeding records.</returns>
        [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.Parent))] //for now authorization is used only here for development
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GetPetFeedingDTO>>>> GetPetsWithFeedings()
        {
            var result = await petFeedingQueryService.GetAllPetFeedingsAsync();

            return Ok(ApiResponse<IEnumerable<GetPetFeedingDTO>>.Ok(result));
        }

        /// <summary>
        /// Retrieve a specific pet and its feeding history by the unique id.
        /// </summary>
        /// <param name="id">Unique id of the pet.</param>
        /// <returns>API response containing a List of feedings records of that pet.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GetPetFeedingDTO>>> GetPetsWithFeedingsById(int id)
        {
            var result = await petFeedingQueryService.GetPetFeedingsByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponse<GetPetFeedingDTO>.Fail("Pet and it's feeding times were not found."));

            return Ok(ApiResponse<GetPetFeedingDTO>.Ok(result));
        }
    }
}
