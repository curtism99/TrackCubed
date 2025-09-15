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

        // --- Properties for Page State & UI ---
        [ObservableProperty] private string _pageTitle;
        [ObservableProperty] private string _saveButtonText;
        [ObservableProperty] private CubedItemDto _itemToEdit; // The full DTO passed during navigation

        // --- Properties for Form Data Binding ---
        [ObservableProperty] private Guid _itemId;
        [ObservableProperty] private string _name;
        [ObservableProperty] private string _link;
        [ObservableProperty] private string _description;
        [ObservableProperty] private string _notes;

        // --- Properties for Item Type Picker ---
        [ObservableProperty] private ObservableCollection<ItemType> _itemTypeOptions; // Holds the full ItemType objects
        [ObservableProperty] private ItemType _selectedItemType; // Holds the currently selected ItemType OBJECT

        // --- Properties for Custom Item Type ---
        [ObservableProperty] private bool _isCustomTypeEntryVisible;
        [ObservableProperty] private string _customItemType;

        // --- Properties for Tag Management ---
        [ObservableProperty] private ObservableCollection<string> _tags;
        [ObservableProperty] private string _newTagText;
        [ObservableProperty] private ObservableCollection<string> _tagSuggestions;

        public AddCubedItemViewModel(CubedDataService dataService)
        {
            _dataService = dataService;

            // Initialize all collections immediately to prevent null reference errors
            ItemTypeOptions = new ObservableCollection<ItemType>();
            Tags = new ObservableCollection<string>();
            TagSuggestions = new ObservableCollection<string>();
        }

        // This command is called from the page's OnAppearing method
        [RelayCommand]
        private async Task InitializePageAsync()
        {
            // 1. Set default state for "Add Mode"
            PageTitle = "Add New Item";
            SaveButtonText = "Create";
            _isEditMode = false;

            // 2. Load the available item types from the API
            var types = await _dataService.GetAvailableItemTypesAsync();
            ItemTypeOptions.Clear();
            foreach (var type in types)
            {
                ItemTypeOptions.Add(type);
            }

            // 3. If an item was passed in, switch to "Edit Mode"
            if (ItemToEdit != null)
            {
                _isEditMode = true;
                PageTitle = "Edit Item";
                SaveButtonText = "Update";

                // Populate form from the DTO
                ItemId = ItemToEdit.Id;
                Name = ItemToEdit.Name;
                Link = ItemToEdit.Link;
                Description = ItemToEdit.Description;
                Notes = ItemToEdit.Notes;

                Tags.Clear();
                foreach (var tag in ItemToEdit.Tags) Tags.Add(tag);

                // Set the Picker's selection
                SelectedItemType = ItemTypeOptions.FirstOrDefault(t => t.Name.Equals(ItemToEdit.ItemTypeName, StringComparison.OrdinalIgnoreCase));

                // If the type is custom, set the custom text field
                if (SelectedItemType?.Name == "Other")
                {
                    CustomItemType = ItemToEdit.ItemTypeName;
                }
            }

            // 4. If no item type is selected (e.g., in Add Mode), default to "Link"
            if (SelectedItemType == null)
            {
                SelectedItemType = ItemTypeOptions.FirstOrDefault(t => t.Name == "Link");
            }
        }

        // This partial method fires when the Picker's selection changes
        partial void OnSelectedItemTypeChanged(ItemType value)
        {
            IsCustomTypeEntryVisible = value?.Name == "Other";
        }

        // This partial method fires when the user types in the tag box
        async partial void OnNewTagTextChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length < 2)
            {
                TagSuggestions.Clear();
                return;
            }
            var suggestions = await _dataService.GetTagSuggestionsAsync(value);
            MainThread.BeginInvokeOnMainThread(() =>
            {
                TagSuggestions.Clear();
                if (suggestions != null) { foreach (var s in suggestions) TagSuggestions.Add(s); }
            });
        }

        [RelayCommand]
        private void AddTag(string tagToAdd)
        {
            if (string.IsNullOrWhiteSpace(tagToAdd)) return;
            var tag = tagToAdd.Trim();
            if (!Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                Tags.Add(tag);
            }
            NewTagText = string.Empty;
            TagSuggestions.Clear();
        }

        [RelayCommand]
        private void RemoveTag(string tagToRemove)
        {
            if (tagToRemove != null) Tags.Remove(tagToRemove);
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            // 1. Get the final item type name
            if (SelectedItemType == null)
            {
                await Shell.Current.DisplayAlert("Validation Error", "Please select an item type.", "OK");
                return;
            }

            string finalItemTypeName = SelectedItemType.Name;
            if (SelectedItemType.Name == "Other")
            {
                if (string.IsNullOrWhiteSpace(CustomItemType))
                {
                    await Shell.Current.DisplayAlert("Validation Error", "Please enter a custom item type when 'Other' is selected.", "OK");
                    return;
                }
                finalItemTypeName = CustomItemType.Trim();
            }

            // 2. Call the correct service method based on the mode
            if (_isEditMode)
            {
                var updatedDto = new CubedItemDto
                {
                    Id = ItemId,
                    Name = Name,
                    Link = Link,
                    Description = Description,
                    Notes = Notes,
                    ItemTypeName = finalItemTypeName,
                    Tags = new List<string>(Tags)
                };
                await _dataService.UpdateCubedItemAsync(updatedDto);
            }
            else
            {
                var newItemDto = new CubedItemCreateDto
                {
                    Name = Name,
                    Link = Link,
                    Description = Description,
                    Notes = Notes,
                    ItemTypeName = finalItemTypeName,
                    Tags = new List<string>(Tags)
                };
                await _dataService.AddCubedItemAsync(newItemDto);
            }

            // 3. Notify the main page and navigate back
            WeakReferenceMessenger.Default.Send(new RefreshItemsMessage(true));
            await Shell.Current.GoToAsync("..");
        }

        [RelayCommand]
        private async Task CancelAsync() => await Shell.Current.GoToAsync("..");
    }
}
