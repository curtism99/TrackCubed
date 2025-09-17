using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Maui.Messages;
using TrackCubed.Maui.Services;
using TrackCubed.Maui.Views;
using TrackCubed.Shared.DTOs;
using TrackCubed.Shared.Models;

namespace TrackCubed.Maui.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly CubedDataService _cubedDataService;
        private readonly AuthService _authService; // Also inject AuthService

        private int _currentPage = 1;
        private const int PageSize = 20;


        // This flag is still needed to prevent unnecessary API calls
        // when the user has already scrolled to the end of the list.
        private bool _areAllItemsLoaded = false;

        private CancellationTokenSource _searchCancellationTokenSource;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReady))] // <-- ADD THIS ATTRIBUTE
        private bool _isBusy = true;

        // This computed property now works perfectly.
        public bool IsReady => !IsBusy;

        [ObservableProperty] 
        private bool _isRefreshing;

        [ObservableProperty] 
        private bool _isLoadingMore;

        [ObservableProperty]
        private ObservableCollection<CubedItemDto> _items;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private ItemType _selectedItemTypeFilter;

        [ObservableProperty]
        private ObservableCollection<ItemType> _itemTypeFilterOptions;

        [ObservableProperty]
        private ObservableCollection<string> _appliedTags;

        // A collection to hold the returned tag suggestions from the API
        [ObservableProperty]
        private ObservableCollection<string> _tagSuggestions;

        [ObservableProperty]
        private string _tagSearchText; // For the "Add Tag" entry field

        [ObservableProperty]
        private bool _isExclusiveTagSearch; // Bound to the new Switch control

        // Inject both services
        public MainPageViewModel(CubedDataService cubedDataService, AuthService authService)
        {
            _cubedDataService = cubedDataService;
            _authService = authService; // For the SignOut command
            Items = new ObservableCollection<CubedItemDto>();
            Title = "Track³ | My Cubes";
            AppliedTags = new ObservableCollection<string>();
            ItemTypeFilterOptions = new ObservableCollection<ItemType>();
            TagSuggestions = new ObservableCollection<string>();

            // Register for messages as before
            WeakReferenceMessenger.Default.Register<RefreshItemsMessage>(this, (r, m) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (RefreshCommand.CanExecute(null))
                    {
                        RefreshCommand.Execute(null);
                    }
                });
            });
        }

        // This command will orchestrate all startup tasks.
        [RelayCommand]
        private async Task InitializeAsync()
        {
            // The IsBusy flag is already true, so no need to set it again.
            try
            {
                Debug.WriteLine("Starting initial page load and cleanup...");

                // Create a list of tasks that can run in parallel.
                // Loading filter options and performing cleanup do not depend on each other.
                var initializationTasks = new List<Task>
                {
                    LoadFilterOptionsCommand.ExecuteAsync(null),
                    _cubedDataService.CleanUpOrphanedTagsIfNeededAsync(),
                    _cubedDataService.CleanUpOrphanedItemTypesIfNeededAsync()
                };

                // Wait for all of them to complete.
                await Task.WhenAll(initializationTasks);
                Debug.WriteLine("Cleanup and filter loading complete.");

                // Now that the initial setup is done, load the first page of items.
                // We execute the RefreshCommand to ensure consistent behavior.
                await RefreshCommand.ExecuteAsync(null);
                Debug.WriteLine("Initial items loaded.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CRITICAL] Page initialization failed: {ex.Message}");
                // Show a user-friendly error. This is crucial for handling startup failures.
                await Shell.Current.DisplayAlert("Initialization Error", "Could not load data. Please check your connection and restart the app.", "OK");
            }
            finally
            {
                // No matter what happens, ensure the loading indicator is hidden
                // and the main content becomes visible.
                IsBusy = false;
                // We must notify the UI that our computed property has also changed.
                OnPropertyChanged(nameof(IsReady));
            }
        }

        // This is the new command for loading/refreshing data
        [RelayCommand]
        private async Task RefreshAsync()
        {
            // When the user manually refreshes, we are no longer in the "initial load" state.
            _currentPage = 1;
            _areAllItemsLoaded = false;
            try
            {
                // Fetch the very first page using our new helper
                var newItems = await FetchCubedItemsPage(_currentPage);

                // Replace the entire collection. This sends ONE notification to the UI
                // and ensures the scroll position resets to the top.
                if (newItems != null)
                {
                    Items = new ObservableCollection<CubedItemDto>(newItems);
                }

                // Check if we've already loaded everything on the first go
                if (newItems == null || newItems.Count < PageSize)
                {
                    _areAllItemsLoaded = true;
                }
                else
                {
                    _currentPage++; // Get ready for the next page
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error refreshing items: {ex.Message}");
                // Optionally, display an error message to the user
            }
        }

        [RelayCommand]
        private async Task AddNewItemAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddCubedItemPage));
        }

        [RelayCommand]
        private async Task SignOutAsync()
        {
            bool confirmed = await Shell.Current.DisplayAlert(
                "Sign Out",
                "Are you sure you want to sign out?",
                "Yes, Sign Out",
                "Cancel");

            if (confirmed)
            {
                await _authService.SignOutAsync();

                // OLD CODE (to be removed):
                // AppShell.OnLoginStateChanged?.Invoke();

                // NEW CODE: Send a message to any part of the app that is listening.
                WeakReferenceMessenger.Default.Send(new SignOutMessage(true));
            }
        }

        [RelayCommand]
        private async Task DeleteCubedItemAsync(CubedItemDto itemToDelete)
        {
            if (itemToDelete == null) return;

            // CRITICAL UX: Always ask for confirmation before a destructive action.
            bool confirmed = await Shell.Current.DisplayAlert(
                "Delete Item",
                $"Are you sure you want to delete '{itemToDelete.Name}'?",
                "Yes, Delete",
                "No, Cancel");

            if (!confirmed) return;

            // If confirmed, proceed with deletion.
            bool success = await _cubedDataService.DeleteCubedItemAsync(itemToDelete.Id);

            if (success)
            {
                // If the API call was successful, remove the item from the list on the screen.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Items.Remove(itemToDelete);
                });
            }
            else
            {
                // If it failed, inform the user.
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Shell.Current.DisplayAlert("Error", "Failed to delete the item. Please try again.", "OK");
                });
            }
        }

        [RelayCommand]
        private async Task GoToEditItemAsync(CubedItemDto item)
        {
            if (item == null) return;

            // Explicitly define the dictionary type
            var navigationParameter = new Dictionary<string, object>
            {
                { "ItemToEdit", item }
            };

            // Navigate to the page, passing the selected item as a parameter.
            // The key "ItemToEdit" must match the QueryProperty in the target ViewModel.
            await Shell.Current.GoToAsync(nameof(AddCubedItemPage), navigationParameter);
        }

        // When the user changes any filter, re-run the search.
        // The On...Changed methods are automatically generated by [ObservableProperty]
        partial void OnSearchTextChanged(string value) => DebouncedRefresh();
        partial void OnSelectedItemTypeFilterChanged(ItemType value) => DebouncedRefresh();
        partial void OnIsExclusiveTagSearchChanged(bool value) => DebouncedRefresh();

        // The user selects a tag, and we re-run the search
        partial void OnAppliedTagsChanged(ObservableCollection<string> value)
        {
            RefreshCommand.Execute(null);
        }

        [RelayCommand]
        private void AddTagFilter(string tagToAdd) // Modify this to accept a parameter
        {
            if (string.IsNullOrWhiteSpace(tagToAdd)) return;

            var newTag = tagToAdd.Trim();
            if (!AppliedTags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
            {
                AppliedTags.Add(newTag);
                DebouncedRefresh(); // Use the debouncer            }

                TagSearchText = string.Empty; // Clear the entry field
                TagSuggestions.Clear();      // Clear the suggestions list
            }
        }

        [RelayCommand]
        private void RemoveTagFilter(string tagToRemove)
        {
            if (AppliedTags.Remove(tagToRemove))
            {
                DebouncedRefresh(); // Use the debouncer
            }
        }

        // This partial method is generated by [ObservableProperty] and runs
        // every time the user types a character in the tag entry field.
        async partial void OnTagSearchTextChanged(string value)
        {
            // Don't search for very short strings, and clear suggestions if the box is empty.
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
            {
                TagSuggestions.Clear();
                return;
            }

            // Fetch new suggestions from the API
            var suggestions = await _cubedDataService.GetTagSuggestionsAsync(value);

            // Update the UI on the main thread
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TagSuggestions.Clear();
                if (suggestions != null)
                {
                    foreach (var s in suggestions)
                    {
                        TagSuggestions.Add(s);
                    }
                }
            });
        }

        // A new command dedicated to loading the filter options on startup.
        [RelayCommand]
        private async Task LoadFilterOptionsAsync()
        {
            // 1. Get the full list of types from the service (includes "Other" and custom types)
            var allTypes = await _cubedDataService.GetAvailableItemTypesAsync();

            // 2. Filter out the specific "Other" type for the filter UI.
            var filteredTypes = allTypes?.Where(t => !t.Name.Equals("Other", StringComparison.OrdinalIgnoreCase));


            MainThread.BeginInvokeOnMainThread(() =>
            {
                ItemTypeFilterOptions.Clear();

                // 3. Create a "dummy" ItemType object to represent the "All" filter.
                var allItemsType = new ItemType { Id = 0, Name = "All Item Types" };
                ItemTypeFilterOptions.Add(allItemsType);

                if (filteredTypes != null)
                {
                    foreach (var type in filteredTypes)
                    {
                        ItemTypeFilterOptions.Add(type);
                    }
                }

                // 4. Set the default selection.
                SelectedItemTypeFilter = allItemsType;
            });
        }

        [RelayCommand]
        private async Task OpenLinkAsync(string url)
        {
            // 1. Basic validation: Make sure the URL is not empty.
            if (string.IsNullOrWhiteSpace(url))
            {
                await Shell.Current.DisplayAlert("No Link", "This item does not have a link to open.", "OK");
                return;
            }

            // 2. Add 'http://' or 'https://' if it's missing, which is a common user input error.
            //    The Launcher requires a well-formed URI.
            string fullUrl = url;
            if (!fullUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !fullUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                fullUrl = "http://" + fullUrl;
            }

            try
            {
                // 3. Use the cross-platform Launcher to open the URL in the default browser.
                Uri uri = new Uri(fullUrl);
                await Browser.Default.OpenAsync(uri, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                // 4. Handle potential errors, like a malformed URL.
                System.Diagnostics.Debug.WriteLine($"Failed to open link: {ex.Message}");
                await Shell.Current.DisplayAlert("Invalid Link", $"Could not open the link: {url}", "OK");
            }
        }


        // --- COMMAND 2: FOR LOADING THE NEXT PAGE (INFINITE SCROLL) ---
        // The generated command will be called "LoadMoreCommand"
        [RelayCommand]
        private async Task LoadMoreAsync()
        {
            // Guard against running if a refresh is happening, if another load is happening,
            // or if we've already loaded all items.
            if (RefreshCommand.IsRunning || LoadMoreCommand.IsRunning || _areAllItemsLoaded) return;

            try
            {
                // Fetch the NEXT page using our new helper
                var newItems = await FetchCubedItemsPage(_currentPage);

                if (newItems != null && newItems.Any())
                {
                    // Add the new items to the END of the existing collection.
                    foreach (var item in newItems)
                    {
                        Items.Add(item);
                    }
                    _currentPage++; // Increment for the next page
                }

                // Check if this was the last page
                if (newItems == null || newItems.Count < PageSize)
                {
                    _areAllItemsLoaded = true;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading more items: {ex.Message}");
                // Optionally, display an error message to the user
            }
            finally
            {
                _areAllItemsLoaded = false;
            }
        }

        private async Task<List<CubedItemDto>> FetchCubedItemsPage(int page)
        {
            // 1. Translate the UI filters into API-friendly values
            string apiItemTypeFilter = SelectedItemTypeFilter?.Id == 0 ? "All" : SelectedItemTypeFilter?.Name;
            string tagMode = IsExclusiveTagSearch ? "all" : "any";

            // 2. Call the data service with all the filter criteria and the requested page number
            var items = await _cubedDataService.SearchItemsAsync(
                SearchText,
                SelectedItemTypeFilter?.Id,
                new List<string>(AppliedTags),
                tagMode,
                page,
                PageSize); // PageSize is a const we defined earlier (e.g., 20)

            return items;
        }
        private void DebouncedRefresh()
        {
            // Cancel any previously scheduled refresh
            _searchCancellationTokenSource?.Cancel();
            _searchCancellationTokenSource = new CancellationTokenSource();

            // Schedule the refresh to happen after 500ms
            Task.Delay(500, _searchCancellationTokenSource.Token)
                .ContinueWith(t =>
                {
                    // Only run if the task wasn't cancelled
                    if (t.IsCompletedSuccessfully)
                    {
                        // We must dispatch the command execution to the main thread
                        MainThread.BeginInvokeOnMainThread(() => RefreshCommand.Execute(null));
                    }
                }, TaskScheduler.Default);
        }
    }
}
