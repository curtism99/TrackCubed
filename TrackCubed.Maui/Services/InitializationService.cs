using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackCubed.Maui.Services
{
    public class InitializationService
    {
        private readonly CubedDataService _dataService;
        private readonly AuthService _authService;

        public InitializationService(CubedDataService dataService, AuthService authService)
        {
            _dataService = dataService;
            _authService = authService;
        }

        public async Task InitializeAfterLoginAsync()
        {
            System.Diagnostics.Debug.WriteLine("Starting post-login initialization...");

            // Create a list of all the startup tasks you need to run.
            var startupTasks = new List<Task>
        {
            // These will now run in parallel on background threads.
            _dataService.CleanUpOrphanedTagsIfNeededAsync(),
            _dataService.CleanUpOrphanedItemTypesIfNeededAsync(),
            _dataService.GetAvailableItemTypesAsync() // Pre-load item types if needed
            // Add any other data pre-loading tasks here.
        };

            // Asynchronously wait for ALL of them to complete.
            await Task.WhenAll(startupTasks);

            System.Diagnostics.Debug.WriteLine("Post-login initialization complete.");
        }
    }
}
