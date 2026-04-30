using DidWeFeedTheCatToday.Shared.Common;
using DidWeFeedTheCatToday.Shared.DTOs.Feedings;
using DidWeFeedTheCatToday.Shared.DTOs.PetFeedings;
using DidWeFeedTheCatToday.Shared.DTOs.Pets;
using System.Net.Http.Json;

namespace DidWeFeedTheCatToday.Client.Services
{
    public class PetService(HttpClient http)
    {
        /*public async Task<List<GetPetDTO>> GetPetsAsync()
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
        }*/

		public async Task<PagedResult<GetPetDTO>> GetPetsAsync(int page, string? search, string? sortBy)
		{
            var url = $"api/pets?page={page}&search={search}&sortBy={sortBy}";
			var response = await http.GetFromJsonAsync<ApiResponse<PagedResult<GetPetDTO>>>(url);
			return response?.Data ?? new PagedResult<GetPetDTO>();
        }

		public async Task<GetPetDTO?> GetPetByIdAsync(int id)
		{
			try
			{
				var url = $"api/pets/{id}";
				var response = await http.GetAsync(url);

				if (response.IsSuccessStatusCode)
				{
					var result = await response.Content.ReadFromJsonAsync<ApiResponse<GetPetDTO>>();
					return result?.Data;
				}

				return null;
			}
			catch (Exception ex)
			{
                Console.WriteLine($"Error loading pet: {ex.Message}");
                return null;
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

		public async Task<bool> DeletePetAsync(int id)
		{
			var response = await http.DeleteAsync($"api/pets/{id}");
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> AddFeedingAsync(PostFeedingDTO feeding)
		{
			var response = await http.PostAsJsonAsync("api/feedings", feeding);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> UpdatePetAsync(int id, CommandPetDTO petToUpdate)
		{
			var response = await http.PutAsJsonAsync($"api/pets/{id}", petToUpdate);
			return response.IsSuccessStatusCode;
		}
    }
}
