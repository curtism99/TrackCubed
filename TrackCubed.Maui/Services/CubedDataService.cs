using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Shared.Models;

namespace TrackCubed.Maui.Services
{
    public class CubedDataService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthService _authService;

        public CubedDataService(HttpClient httpClient, AuthService authService)
        {
            _httpClient = httpClient;
            _authService = authService;
        }

        public async Task<List<CubedItem>> GetMyCubedItemsAsync()
        {
            try
            {
                // Ensure the user is authenticated and we have a token
                var token = await _authService.GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    // Not logged in or token expired
                    return new List<CubedItem>();
                }

                // Set the authorization header for this request
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Make the call and automatically deserialize the JSON response
                var items = await _httpClient.GetFromJsonAsync<List<CubedItem>>("api/CubedItems");

                return items ?? new List<CubedItem>();
            }
            catch (Exception ex)
            {
                // Handle exceptions (e.g., log them, show an alert)
                System.Diagnostics.Debug.WriteLine($"Error fetching CubedItems: {ex.Message}");
                return new List<CubedItem>();
            }
        }
    }
}
