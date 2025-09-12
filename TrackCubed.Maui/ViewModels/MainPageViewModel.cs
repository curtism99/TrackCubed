using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        [NotifyPropertyChangedFor(nameof(IsNotBusy))]
        private bool _isBusy;

        public bool IsNotBusy => !IsBusy;

        [ObservableProperty]
        private string _title;

        // Inject both services
        public MainPageViewModel(CubedDataService cubedDataService, AuthService authService)
        {
            _cubedDataService = cubedDataService;
            _authService = authService; // For the SignOut command
            Items = new ObservableCollection<CubedItemDto>();
            Title = "My Cubed Items";
        }

        // This is the new command for loading/refreshing data
        [RelayCommand]
        private async Task LoadItemsAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;
                var items = await _cubedDataService.GetMyCubedItemsAsync();

                Items.Clear();
                foreach (var item in items)
                {
                    Items.Add(item);
                }
            }
            finally
            {
                IsBusy = false;
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
