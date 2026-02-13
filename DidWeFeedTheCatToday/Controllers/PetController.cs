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
        [HttpGet]
        public async Task<ActionResult<ApiResponse<IEnumerable<GetPetDTO>>>> GetPets()
        {
            var result = await petService.GetAllPetsAsync();

            return Ok(ApiResponse<IEnumerable<GetPetDTO>>.Ok(result));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<GetPetDTO>>> GetPetById(int id)
        {
            var result = await petService.GetPetByIdAsync(id);

            if (result == null)
                return NotFound(ApiResponse<GetPetDTO>.Fail("No Pet found under index."));

            return Ok(ApiResponse<GetPetDTO>.Ok(result));
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<GetPetDTO>>> PostPet(CommandPetDTO petDTO)
        {
            var result = await petService.AddPetAsync(petDTO);

            return CreatedAtAction(nameof(GetPetById), new { id = result.Id }, ApiResponse<GetPetDTO>.Ok(result));
        }

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
