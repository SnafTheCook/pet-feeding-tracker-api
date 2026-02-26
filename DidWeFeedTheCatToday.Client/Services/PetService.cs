using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using System.Net.Http.Json;

namespace DidWeFeedTheCatToday.Client.Services
{
    public class PetService(HttpClient http)
    {
        public async Task<List<GetPetDTO>> GetPetsAsync()
        {
			try
			{
				var response = await http.GetFromJsonAsync<ApiResponse<IEnumerable<GetPetDTO>>>("api/pets");
				return response?.Data?.ToList() ?? new List<GetPetDTO>();
			}
			catch (Exception e)
			{
				Console.WriteLine($"Error fetching pets: {e.Message}");
				return new List<GetPetDTO>();
			}
        }

		public async Task<List<GetPetFeedingDTO>> GetPetFeedingsAsync()
		{
			try
			{
				var response = await http.GetFromJsonAsync<ApiResponse<IEnumerable<GetPetFeedingDTO>>>("api/pet-feedings");
				return response?.Data?.ToList() ?? new List<GetPetFeedingDTO>();
			}
			catch(Exception e)
			{
				Console.WriteLine($"Error fetching pet feedings: {e.Message}");
				return new List<GetPetFeedingDTO>();
			}
		}

        public async Task<bool> AddPetAsync(CommandPetDTO petToAdd)
        {
            var response = await http.PostAsJsonAsync("api/pets", petToAdd);
            return response.IsSuccessStatusCode;
        }
    }
}
