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

        // This property is bound to the visibility of the custom Entry field
        [ObservableProperty]
        private bool _isCustomTypeEntryVisible;

        // This property is bound to the text of the custom Entry field
        [ObservableProperty]
        private string _customItemType;

        // This allows us to add items to it after it's been created.
        [ObservableProperty]
        private ObservableCollection<string> _predefinedItemTypes;

        // Form properties
        [ObservableProperty] private Guid _itemId;
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _link;
        [ObservableProperty] private string _description;
        [ObservableProperty] private string _notes;
        [ObservableProperty] private CubedItemType _itemType;

        // Add other properties for other fields as needed

        [ObservableProperty]
        private string _selectedItemType;

        public AddCubedItemViewModel(CubedDataService dataService)
        {
            _dataService = dataService;
            PageTitle = "Add New Item"; // Default title
            SaveButtonText = "Create"; // Default text for "Add Mode"
            Tags = new ObservableCollection<string>(); // Initialize the collection
            PredefinedItemTypes = new ObservableCollection<string>();
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            string finalItemType = SelectedItemType;
            if (SelectedItemType == "Other" && !string.IsNullOrWhiteSpace(CustomItemType))
            {
                finalItemType = CustomItemType;
            }


            if (_isEditMode)
            {
                var updatedDto = new CubedItemDto
                {
                    Id = this.ItemId,
                    Name = this.Name,
                    Link = this.Link,
                    Description = this.Description,
                    ItemType = finalItemType, // Use the final Item type
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
                    ItemType = finalItemType, // Use the final Item type
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

        // The only job of this method is now to set the flag and page title.
        // It does NOT try to apply data, because the data isn't ready yet.
        partial void OnItemToEditChanged(CubedItemDto value)
        {
            if (value != null)
            {
                _isEditMode = true;
                PageTitle = "Edit Item";
                SaveButtonText = "Update";
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

        // This method is automatically called when the SelectedItemType property changes
        // This is where we will control the visibility of the custom entry field.
        partial void OnSelectedItemTypeChanged(string value)
        {
            IsCustomTypeEntryVisible = value == "Other";
        }

        // This method is now responsible for ALL data loading and setup.
        [RelayCommand]
        private async Task LoadDataAsync()
        {
            // 1. Fetch the list of types from the API.
            var types = await _dataService.GetPredefinedItemTypesAsync();
            PredefinedItemTypes.Clear();
            foreach (var type in types)
            {
                PredefinedItemTypes.Add(type);
            }

            // 2. NOW, check if we are in Edit Mode.
            if (_isEditMode && ItemToEdit != null)
            {
                // 3. Populate ALL the form fields at once.
                ItemId = ItemToEdit.Id;
                Name = ItemToEdit.Name;
                Link = ItemToEdit.Link;
                Description = ItemToEdit.Description;
                Notes = ItemToEdit.Notes;

                // 4. This is the critical logic to set the Picker correctly.
                if (!string.IsNullOrEmpty(ItemToEdit.ItemType))
                {
                    // If the item's type is a custom one (not in our official list)...
                    if (!PredefinedItemTypes.Contains(ItemToEdit.ItemType))
                    {
                        // ...temporarily add it to the collection so it can be selected.
                        PredefinedItemTypes.Insert(0, ItemToEdit.ItemType);
                        SelectedItemType = ItemToEdit.ItemType;
                        CustomItemType = ItemToEdit.ItemType;
                    }
                    else
                    {
                        // Otherwise, just select it from the existing list.
                        SelectedItemType = ItemToEdit.ItemType;
                    }
                }
            }
            else // We are in Add Mode
            {
                // Set the default selection for a new item.
                SelectedItemType = PredefinedItemTypes.FirstOrDefault();
            }
        }

    }
}
