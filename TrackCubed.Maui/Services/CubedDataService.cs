using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using TrackCubed.Shared.DTOs;
using TrackCubed.Shared.Models;

namespace TrackCubed.Maui.Services
{
    public class CubedDataService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceProvider _services; // Use the service provider to break circular dependencies
        private const string LastTagCleanupKey = "LastTagCleanup";

        public CubedDataService(HttpClient httpClient, IServiceProvider services)
        {
            _httpClient = httpClient;
            _services = services;
        }

        // It safely resolves the AuthService from the DI container.
        private AuthService GetAuthService() => _services.GetRequiredService<AuthService>();

        public async Task<List<CubedItemDto>> GetMyCubedItemsAsync()
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    System.Diagnostics.Debug.WriteLine("[GetMyCubedItemsAsync] Failed: No auth token.");
                    return new List<CubedItemDto>();
                }

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Let's get the raw response first to see what the server is sending
                // The GetAsync() and ReadFromJsonAsync() calls do not need the UI thread.
                var response = await _httpClient.GetAsync("api/CubedItems").ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"[GetMyCubedItemsAsync] Success! Received JSON: {jsonString}");

                    // Now, try to deserialize
                    var items = await response.Content.ReadFromJsonAsync<List<CubedItemDto>>().ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"[GetMyCubedItemsAsync] Deserialized {items?.Count ?? 0} items.");
                    return items ?? new List<CubedItemDto>();
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    System.Diagnostics.Debug.WriteLine($"[GetMyCubedItemsAsync] Failed with status {response.StatusCode}: {errorContent}");
                    return new List<CubedItemDto>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetMyCubedItemsAsync] Exception: {ex.Message}");
                return new List<CubedItemDto>();
            }
        }

        public async Task<CubedItemDto> AddCubedItemAsync(CubedItemCreateDto itemToAdd)
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return null;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.PostAsJsonAsync("api/CubedItems", itemToAdd).ConfigureAwait(false);

                // --- START: IMPROVED ERROR HANDLING ---

                // Explicitly check for the 201 Created status code.
                if (response.StatusCode == HttpStatusCode.Created)
                {
                    try
                    {
                        // The API returns the created item, so we deserialize it.
                        // This is the line that might be failing silently.
                        var createdItemDto = await response.Content.ReadFromJsonAsync<CubedItemDto>().ConfigureAwait(false); 
                        return createdItemDto;
                    }
                    catch (JsonException ex)
                    {
                        // This will catch any errors during deserialization.
                        System.Diagnostics.Debug.WriteLine($"JSON Deserialization Error: {ex.Message}");
                        return null; // The save worked, but we couldn't read the response.
                    }
                }

                // If the code is not 201, it's an error.
                var errorContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine($"API returned an error ({response.StatusCode}): {errorContent}");
                return null;

                // --- END: IMPROVED ERROR HANDLING ---
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception in AddCubedItemAsync: {ex.Message}");
                return null;
            }
        }

        public async Task<bool> DeleteCubedItemAsync(Guid id)
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return false;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Use the DeleteAsync method and pass the ID in the URL.
                var response = await _httpClient.DeleteAsync($"api/CubedItems/{id}").ConfigureAwait(false);

                // IsSuccessStatusCode will be true for 204 No Content.
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception deleting CubedItem: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateCubedItemAsync(CubedItemDto itemToUpdate)
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return false;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Use PutAsJsonAsync, passing the ID in the URL and the DTO in the body.
                var response = await _httpClient.PutAsJsonAsync($"api/CubedItems/{itemToUpdate.Id}", itemToUpdate)
                                                .ConfigureAwait(false);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception updating CubedItem: {ex.Message}");
                return false;
            }
        }

        public async Task<List<CubedItemDto>> SearchItemsAsync(string? searchText, int? itemTypeId, List<string>? tags, string tagMode, int pageNumber, int pageSize)
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return new List<CubedItemDto>();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Build the query string dynamically, now including the tagMode
                var sb = new StringBuilder("api/CubedItems/search?");
                if (!string.IsNullOrWhiteSpace(searchText)) sb.Append($"searchText={Uri.EscapeDataString(searchText)}&");
                if (itemTypeId.HasValue) sb.Append($"itemTypeId={itemTypeId.Value}&");
                if (tags != null && tags.Any())
                {
                    foreach (var tag in tags) sb.Append($"tags={Uri.EscapeDataString(tag)}&");
                }
                sb.Append($"tagMode={Uri.EscapeDataString(tagMode)}&");
                sb.Append($"pageNumber={pageNumber}&");
                sb.Append($"pageSize={pageSize}");

                var response = await _httpClient.GetAsync(sb.ToString()).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<CubedItemDto>>().ConfigureAwait(false);
                }
                return new List<CubedItemDto>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error searching items: {ex.Message}");
                return new List<CubedItemDto>();
            }
        }

        public async Task<List<string>> GetTagSuggestionsAsync(string prefix)
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return new List<string>();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var suggestions = await _httpClient.GetFromJsonAsync<List<string>>($"api/Tags/suggest?prefix={Uri.EscapeDataString(prefix)}")
                                                   .ConfigureAwait(false);
                return suggestions ?? new List<string>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error getting tag suggestions: {ex.Message}");
                return new List<string>();
            }
        }

        public async Task<bool> WipeAllUserDataAsync()
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return false;

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync("api/User/wipe-data").ConfigureAwait(false);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception wiping user data: {ex.Message}");
                return false;
            }
        }

        public async Task<int> CleanUpOrphanedTagsAsync()
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return -1; // Return -1 to indicate an auth error

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.DeleteAsync("api/Tags/orphaned").ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    // The API returns a JSON object like { "deletedCount": 5 }
                    // We need to parse this to get the number.
                    var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = JsonDocument.Parse(jsonString).RootElement;
                    if (result.TryGetProperty("deletedCount", out var countElement))
                    {
                        return countElement.GetInt32();
                    }
                    return 0; // Success, but couldn't parse the count
                }

                return -1; // Indicate a failure
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception cleaning up tags: {ex.Message}");
                return -1; // Indicate a failure
            }
        }

        public async Task CleanUpOrphanedTagsIfNeededAsync()
        {
            var lastCleanupTime = Preferences.Get(LastTagCleanupKey, DateTime.MinValue);
            if ((DateTime.UtcNow - lastCleanupTime).TotalMinutes > 24)
            {
                System.Diagnostics.Debug.WriteLine("[Auto-Cleanup] Running...");
                int deletedCount = await CleanUpOrphanedTagsAsync(); // Your existing method
                if (deletedCount >= 0)
                {
                    Preferences.Set(LastTagCleanupKey, DateTime.UtcNow);
                    System.Diagnostics.Debug.WriteLine($"[Auto-Cleanup] Success. Deleted {deletedCount} tags.");
                }
            }
        }

        public async Task<List<ItemType>> GetAvailableItemTypesAsync()
        {
            try
            {
                var authService = GetAuthService();
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return new List<ItemType>();

                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var types = await _httpClient.GetFromJsonAsync<List<ItemType>>("api/ItemTypes").ConfigureAwait(false);

                return types ?? new List<ItemType>();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error fetching ItemTypes: {ex.Message}");
                // On failure, return a minimal list to prevent the app from crashing.
                return new List<ItemType> { new ItemType { Id = 7, Name = "Other" } };
            }
        }
        public async Task<int> CleanUpOrphanedItemTypesAsync()
        {
            try
            {
                var authService = GetAuthService(); // Resolve the service
                var token = await authService.GetAccessTokenAsync().ConfigureAwait(false);
                if (string.IsNullOrEmpty(token)) return -1;

                var request = new HttpRequestMessage(HttpMethod.Delete, "api/ItemTypes/orphaned-custom");
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                if (response.IsSuccessStatusCode)
                {
                    var jsonString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    var result = System.Text.Json.JsonDocument.Parse(jsonString).RootElement;
                    return result.TryGetProperty("deletedCount", out var count) ? count.GetInt32() : 0;
                }
                return -1;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Exception cleaning up item types: {ex.Message}");
                return -1;
            }
        }
    }
}
