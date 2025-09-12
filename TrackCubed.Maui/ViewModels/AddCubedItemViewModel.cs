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
using TrackCubed.Shared.DTOs;
using TrackCubed.Shared.Models;

namespace TrackCubed.Maui.ViewModels
{
    [QueryProperty(nameof(ItemToEdit), "ItemToEdit")]
    public partial class AddCubedItemViewModel : ObservableObject
    {
        private readonly CubedDataService _dataService;
        private bool _isEditMode;

        [ObservableProperty]
        private string _pageTitle;

        [ObservableProperty]
        private string _saveButtonText;
        
        [ObservableProperty]
        private CubedItemDto _itemToEdit;

        // An ObservableCollection to hold the list of tags for the UI
        [ObservableProperty]
        private ObservableCollection<string> _tags;

        // The text from the "Add Tag" entry field
        [ObservableProperty]
        private string _newTagText;

        // Form properties
        [ObservableProperty] private Guid _itemId;
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _link;
        [ObservableProperty] private string _description;
        [ObservableProperty] private string _notes;
        [ObservableProperty] private CubedItemType _itemType;

        // Add other properties for other fields as needed

        public AddCubedItemViewModel(CubedDataService dataService)
        {
            _dataService = dataService;
            PageTitle = "Add New Item"; // Default title
            SaveButtonText = "Create"; // Default text for "Add Mode"
            Tags = new ObservableCollection<string>(); // Initialize the collection
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (_isEditMode)
            {
                var updatedDto = new CubedItemDto
                {
                    Id = this.ItemId,
                    Name = this.Name,
                    Link = this.Link,
                    Description = this.Description,
                    ItemType = this.ItemType,
                    Notes = this.Notes,
                    // Convert the ObservableCollection back to a simple List for the DTO
                    Tags = new List<string>(this.Tags)
                };
                bool success = await _dataService.UpdateCubedItemAsync(updatedDto);
                if (!success) await Shell.Current.DisplayAlert("Error", "Failed to update.", "OK");
            }
            else
            {
                var newItemDto = new CubedItemCreateDto
                {
                    Name = this.Name,
                    Link = this.Link,
                    Description = this.Description,
                    Notes = this.Notes,
                    ItemType = CubedItemType.Link,
                    // Convert the ObservableCollection back to a simple List for the DTO
                    Tags = new List<string>(this.Tags)
                };
                var createdItem = await _dataService.AddCubedItemAsync(newItemDto);
                if (createdItem == null) await Shell.Current.DisplayAlert("Error", "Failed to save.", "OK");
            }

            // Regardless of add or edit, notify the main page to refresh and navigate back.
            WeakReferenceMessenger.Default.Send(new RefreshItemsMessage(true));
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task CancelAsync()
        {
            await Shell.Current.GoToAsync("..");
        }

        // This method is automatically called by MAUI's navigation system
        // when the ItemToEdit property is set.
        partial void OnItemToEditChanged(CubedItemDto value)
        {
            System.Diagnostics.Debug.WriteLine($"[AddCubedItemViewModel] OnItemToEditChanged called. Item is {(value == null ? "NULL" : "NOT NULL")}");

            if (value != null)
            {
                _isEditMode = true;
                PageTitle = "Edit Item";
                SaveButtonText = "Update"; 
                // Populate the form fields with the item's data
                ItemId = value.Id;
                Name = value.Name;
                Link = value.Link;
                Description = value.Description;
                ItemType = value.ItemType;
                Notes = value.Notes;

                // Load the tags from the DTO into the ObservableCollection
                Tags.Clear();
                foreach (var tag in value.Tags)
                {
                    Tags.Add(tag);
                }
            }
        }

        [RelayCommand]
        private void AddTag()
        {
            if (string.IsNullOrWhiteSpace(NewTagText)) return;

            var tagToAdd = NewTagText.Trim();
            if (!Tags.Contains(tagToAdd, StringComparer.OrdinalIgnoreCase))
            {
                Tags.Add(tagToAdd);
            }

            // Clear the entry field for the next tag
            NewTagText = string.Empty;
        }

        [RelayCommand]
        private void RemoveTag(string tagToRemove)
        {
            if (tagToRemove != null)
            {
                Tags.Remove(tagToRemove);
            }
        }

    }
}
