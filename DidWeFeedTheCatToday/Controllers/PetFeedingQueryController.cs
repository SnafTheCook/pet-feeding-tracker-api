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

        [Authorize(Roles = nameof(Roles.Admin) + "," + nameof(Roles.Parent))] //for now authorization is used only here for development
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GetPetFeedingDTO>>>> GetPetsWithFeedings()
        {
            var result = await petFeedingQueryService.GetAllPetFeedingsAsync();

            return Ok(ApiResponse<IEnumerable<GetPetFeedingDTO>>.Ok(result));
        }

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
