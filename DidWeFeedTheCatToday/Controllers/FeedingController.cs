using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using Microsoft.AspNetCore.Mvc;

namespace DidWeFeedTheCatToday.Controllers
{
    [Route("api/feedings")]
    [ApiController]
    public class FeedingController(IFeedingService feedingService) : ControllerBase
    {
        /// <summary>
        /// Retrieves complete history of feedings.
        /// </summary>
        /// <returns>API reponse containing a collection of <see cref="GetFeedingDTO"/></returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GetFeedingDTO>>>> GetFeedings()
        {
            var result = await feedingService.GetFeedingsAsync();

            return Ok(ApiResponse<IEnumerable<GetFeedingDTO>>.Ok(result));
        }
        /// <summary>
        /// Retrieves a unique feeding.
        /// </summary>
        /// <param name="id">Unique identifier of a feeding.</param>
        /// <returns>API response containing a single <see cref="GetFeedingDTO"/></returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GetFeedingDTO>>> GetFeedingById(int id)
        {
            var result = await feedingService.GetFeedingByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponse<GetFeedingDTO>.Fail("Feeding not found."));

            return Ok(ApiResponse<GetFeedingDTO>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GetFeedingDTO>>> PostFeeding(PostFeedingDTO feeding)
        {
            var result = await feedingService.AddFeedingAsync(feeding);

            if (result == null) 
                return BadRequest(ApiResponse<GetFeedingDTO>.Fail("Invalid pet."));

            return CreatedAtAction(nameof(GetFeedingById), new { id = result.Id }, ApiResponse<GetFeedingDTO>.Ok(result));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeeding(int id)
        {
            var success = await feedingService.DeleteFeedingAsync(id);

            if (!success)
                return NotFound(ApiResponse<GetFeedingDTO>.Fail("Feeding not found."));

            return NoContent(); //204 for delete, no wrapper
        }
    }
}
