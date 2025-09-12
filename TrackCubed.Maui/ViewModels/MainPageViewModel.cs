using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        [ObservableProperty]
        private ObservableCollection<CubedItemDto> _items;

        [ObservableProperty]
        private string _title;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private string _selectedItemTypeFilter;

        [ObservableProperty]
        private ObservableCollection<string> _itemTypeFilterOptions;

        [ObservableProperty]
        private ObservableCollection<string> _appliedTags;

        [ObservableProperty]
        private string _tagSearchText; // For the "Add Tag" entry field

        // Inject both services
        public MainPageViewModel(CubedDataService cubedDataService, AuthService authService)
        {
            _cubedDataService = cubedDataService;
            _authService = authService; // For the SignOut command
            Items = new ObservableCollection<CubedItemDto>();
            Title = "My Cubed Items";
            AppliedTags = new ObservableCollection<string>();
            ItemTypeFilterOptions = new ObservableCollection<string>();

            // Register to receive the message
            WeakReferenceMessenger.Default.Register<RefreshItemsMessage>(this, (r, m) =>
            {
                // When a message is received, execute the LoadItemsCommand.
                // Ensure the command is executed on the main UI thread.
                // This prevents deadlocks when the message is received.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (LoadItemsCommand.CanExecute(null))
                    {
                        LoadItemsCommand.Execute(null);
                    }
                });
            });
        }

        // This is the new command for loading/refreshing data
        [RelayCommand]
        private async Task LoadItemsAsync()
        {

            try
            {
                // Instead of getting ALL items, we call the new Search method.
                // We pass it the current values of the filter properties that are bound to the UI.
                var loadedItems = await _cubedDataService.SearchItemsAsync(SearchText, SelectedItemTypeFilter, new List<string>(AppliedTags));

                // We must switch back to the main thread to update the UI-bound collection.
                // This UI update logic remains exactly the same.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Items.Clear();
                    if (loadedItems != null)
                    {
                        foreach (var item in loadedItems)
                        {
                            Items.Add(item);
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                // It's also good practice to show errors on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Shell.Current.DisplayAlert("Error", $"Failed to load items: {ex.Message}", "OK");
                });
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
            await _authService.SignOutAsync();
            AppShell.OnLoginStateChanged?.Invoke();
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
        partial void OnSearchTextChanged(string value) => LoadItemsCommand.Execute(null);
        partial void OnSelectedItemTypeFilterChanged(string value) => LoadItemsCommand.Execute(null);

        // We can also load the filter options on startup
        [RelayCommand]
        private async Task LoadFilterOptionsAsync()
        {
            var types = await _cubedDataService.GetPredefinedItemTypesAsync();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ItemTypeFilterOptions.Clear();
                ItemTypeFilterOptions.Add("All"); // Add an "All" option
                foreach (var type in types)
                {
                    ItemTypeFilterOptions.Add(type);
                }
                SelectedItemTypeFilter = "All"; // Set the default
            });
        }

        // The user selects a tag, and we re-run the search
        partial void OnAppliedTagsChanged(ObservableCollection<string> value)
        {
            LoadItemsCommand.Execute(null);
        }

        [RelayCommand]
        private void AddTagFilter()
        {
            if (string.IsNullOrWhiteSpace(TagSearchText)) return;

            var newTag = TagSearchText.Trim();
            if (!AppliedTags.Contains(newTag, StringComparer.OrdinalIgnoreCase))
            {
                AppliedTags.Add(newTag);
                LoadItemsCommand.Execute(null); // Explicitly refresh
            }

            TagSearchText = string.Empty; // Clear the entry
        }

        [RelayCommand]
        private void RemoveTagFilter(string tagToRemove)
        {
            if (AppliedTags.Contains(tagToRemove))
            {
                AppliedTags.Remove(tagToRemove);
                LoadItemsCommand.Execute(null); // Explicitly refresh
            }
        }
    }
}
