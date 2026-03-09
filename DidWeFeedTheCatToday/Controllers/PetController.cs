using DidWeFeedTheCatToday.Services.Interfaces;
using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using Microsoft.AspNetCore.Mvc;

namespace DidWeFeedTheCatToday.Controllers
{
    [Route("api/pets")]
    [ApiController]
    public class PetController(IPetService petService) : ControllerBase
    {
        /// <summary>
        /// Retrieves all pets from the database.
        /// </summary>
        /// <returns>API response containing a collection of <see cref="GetPetDTO"/>.</returns>
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GetPetDTO>>>> GetPets()
        {
            var result = await petService.GetAllPetsAsync();

            return Ok(ApiResponse<IEnumerable<GetPetDTO>>.Ok(result));
        }
        /// <summary>
        /// Retreives a unique pet.
        /// </summary>
        /// <param name="id">Unique identifier of the pet.</param>
        /// <returns>API response containg a single <see cref="GetPetDTO"/>.</returns>
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GetPetDTO>>> GetPetById(int id)
        {
            var result = await petService.GetPetByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponse<GetPetDTO>.Fail("No Pet found under index."));

            return Ok(ApiResponse<GetPetDTO>.Ok(result));
        }
        /// <summary>
        /// Persists a pet.
        /// </summary>
        /// <param name="petDTO">Pet's information.</param>
        /// <returns>API response containing the persisted <see cref="GetPetDTO"/>.</returns>
        [HttpPost]
        public async Task<ActionResult<ApiResponse<GetPetDTO>>> PostPet(CommandPetDTO petDTO)
        {
            var result = await petService.AddPetAsync(petDTO);

            return CreatedAtAction(nameof(GetPetById), new { id = result.Id }, ApiResponse<GetPetDTO>.Ok(result));
        }
        /// <summary>
        /// Updates an existing pet.
        /// </summary>
        /// <param name="id">Unique pet identifier.</param>
        /// <param name="petDTO">Complete pet data set.</param>
        /// <returns>No content (204) if succeeded.</returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPet(int id, CommandPetDTO petDTO)
        {
            var result = await petService.OverridePetAsync(id, petDTO);

            if (!result.Success)
            {
                return result.Error switch
                {
                    ServiceResultError.NotFound => NotFound(ApiResponse<GetPetDTO>.Fail("Pet not found.")),
                    ServiceResultError.ConcurrencyConflict => Conflict(ApiResponse<GetPetDTO>.Fail("Pet was changed since last request.")),
                    _ => BadRequest()
                };

            }

            return NoContent(); //204

        }
        /// <summary>
        /// Removes an existing pet.
        /// </summary>
        /// <param name="id">Unique pet identifier.</param>
        /// <returns>No content (204) if succeeded.</returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePet(int id)
        {
            var success = await petService.DeletePetAsync(id);

            if (!success)
                return NotFound(ApiResponse<GetPetDTO>.Fail("Pet not found."));

            return NoContent();
        }
    }
}
