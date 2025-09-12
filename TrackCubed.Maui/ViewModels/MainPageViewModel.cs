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

        // Inject both services
        public MainPageViewModel(CubedDataService cubedDataService, AuthService authService)
        {
            _cubedDataService = cubedDataService;
            _authService = authService; // For the SignOut command
            Items = new ObservableCollection<CubedItemDto>();
            Title = "My Cubed Items";

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
                // This await will now complete on a background thread
                var items = await _cubedDataService.GetMyCubedItemsAsync().ConfigureAwait(false);

                // We must switch back to the main thread to update the UI-bound collection.
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Items.Clear();
                    if (items != null)
                    {
                        foreach (var item in items)
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
    }
}
