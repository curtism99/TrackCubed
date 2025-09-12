using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrackCubed.Maui.Messages;
using TrackCubed.Maui.Services;
using TrackCubed.Shared.DTOs;
using TrackCubed.Shared.Models;

namespace TrackCubed.Maui.ViewModels
{
    [QueryProperty(nameof(CubedItem), "CubedItem")]
    public partial class AddCubedItemViewModel : ObservableObject
    {
        private readonly CubedDataService _dataService;

        [ObservableProperty]
        private string _name;

        [ObservableProperty]
        private string _link;

        [ObservableProperty]
        private string _description;

        [ObservableProperty]
        private string _notes;

        // Add other properties for other fields as needed

        public AddCubedItemViewModel(CubedDataService dataService)
        {
            _dataService = dataService;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // Create the DTO with user-provided data
            var newItemDto = new CubedItemCreateDto
            {
                Name = this.Name,
                Link = this.Link,
                Description = this.Description,
                Notes = this.Notes,
                ItemType = CubedItemType.Link
            };

            // Send the DTO to the data service
            var createdItem = await _dataService.AddCubedItemAsync(newItemDto);

            if (createdItem != null)
            {
                // Send a message to any listening page that the items need to be refreshed.
                WeakReferenceMessenger.Default.Send(new RefreshItemsMessage(true));

                // Go back to the main page.
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.DisplayAlert("Error", "Failed to save the new item. Please try again.", "OK");
            }
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }
    }
}
