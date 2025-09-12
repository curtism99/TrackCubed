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
    }
}
