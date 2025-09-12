using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Maui.Services;
using TrackCubed.Shared.Models;

namespace TrackCubed.Maui.ViewModels
{
    public partial class MainPageViewModel : ObservableObject
    {
        private readonly CubedDataService _cubedDataService;

        [ObservableProperty]
        private ObservableCollection<CubedItem> _items;

        [ObservableProperty]
        private bool _isBusy;

        // Inject the new data service
        public MainPageViewModel(CubedDataService cubedDataService)
        {
            _cubedDataService = cubedDataService;
            Items = new ObservableCollection<CubedItem>();
            Title = "My Cubed Items";
        }

        [ObservableProperty]
        private string _title;

        // This command will be called when the page appears
        [RelayCommand]
        private async Task PageAppearingAsync()
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
            // We will create this page in the future. For now, just a placeholder.
            await Shell.Current.DisplayAlert("Not Implemented", "The 'Add New Item' page has not been created yet.", "OK");
            // The real navigation will be: await Shell.Current.GoToAsync("AddItemPage");
        }
    }
}
