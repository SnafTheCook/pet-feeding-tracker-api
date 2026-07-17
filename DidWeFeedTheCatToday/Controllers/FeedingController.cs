using Asp.Versioning;
using DidWeFeedTheCatToday.Features.Feedings;
using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace DidWeFeedTheCatToday.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/feedings")]
    [ApiController]
    [EnableRateLimiting("fixed")]
    public class FeedingController(IMediator mediator) : ControllerBase
    {
        /// <summary>
        /// Retrieves complete history of feedings.
        /// </summary>
        /// <returns>API reponse containing a collection of <see cref="GetFeedingDTO"/></returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GetFeedingDTO>>>> GetFeedings()
        {
            var result = await mediator.Send(new GetFeedingsQuery());

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
            var result = await mediator.Send(new GetFeedingByIdQuery(id));

            if (result == null)
                return NotFound(ApiResponse<GetFeedingDTO>.Fail("Feeding not found."));

            return Ok(ApiResponse<GetFeedingDTO>.Ok(result));
        }
        /// <summary>
        /// Persists a new feeding.
        /// </summary>
        /// <param name="feeding">Feeding information object.</param>
        /// <returns>API response containing a <see cref="GetFeedingDTO"/>.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<GetFeedingDTO>>> PostFeeding(PostFeedingDTO feeding, CancellationToken ct = default)
        {
            var result = await mediator.Send(new AddFeedingCommand(feeding));

            if (result == null)
                return BadRequest(ApiResponse<GetFeedingDTO>.Fail("Invalid pet."));

            return CreatedAtAction(
                nameof(GetFeedingById),
                new { version = "1.0", id = result.Id },
                ApiResponse<GetFeedingDTO>.Ok(result));
        }
        /// <summary>
        /// Removes the unique feeding.
        /// </summary>
        /// <param name="id">Unique identifier of a feeding.</param>
        /// <returns>No content (204) on success. API response containing an error message on failure.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeeding(int id)
        {
            var success = await mediator.Send(new DeleteFeedingCommand(id));

            if (!success)
                return NotFound(ApiResponse<GetFeedingDTO>.Fail("Feeding not found."));

            return NoContent(); //204 for delete, no wrapper
        }
    }
}
