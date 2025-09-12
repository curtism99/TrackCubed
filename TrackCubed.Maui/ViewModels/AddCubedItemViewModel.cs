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


        // A NEW collection to hold the suggestions returned from the API.
        [ObservableProperty]
        private ObservableCollection<string> _tagSuggestions;


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
            Tags = new ObservableCollection<string>(); // Initialize the collection
            PredefinedItemTypes = new ObservableCollection<string>();
            Tags = new ObservableCollection<string>();
            TagSuggestions = new ObservableCollection<string>();
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


        [RelayCommand]
        private void AddTag(string tagToAdd)
        {
            if (string.IsNullOrWhiteSpace(tagToAdd)) return;
            var tag = tagToAdd.Trim();
            if (!Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            {
                Tags.Add(tag);
            }

            // Clear the entry field and the suggestions list for a clean UX.
            NewTagText = string.Empty;
            TagSuggestions.Clear();
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
        partial void OnSelectedItemTypeChanged(string value) => IsCustomTypeEntryVisible = value == "Other";

        // This method is now responsible for ALL data loading and setup.
        [RelayCommand]
        private async Task InitializePageAsync()
        {
            // Always default to Add mode first
            PageTitle = "✎ Add New Cubed Item";
            SaveButtonText = "+ Create";

            var types = await _dataService.GetPredefinedItemTypesAsync();
            PredefinedItemTypes.Clear();
            foreach (var type in types)
            {
                PredefinedItemTypes.Add(type);
            }

            if (ItemToEdit != null)
            {
                _isEditMode = true;
                PageTitle = "✎ Edit Cubed Item";
                SaveButtonText = "✎ Update";

                ItemId = ItemToEdit.Id;
                Name = ItemToEdit.Name;
                Link = ItemToEdit.Link;
                Description = ItemToEdit.Description;
                Notes = ItemToEdit.Notes;

                if (!string.IsNullOrEmpty(ItemToEdit.ItemType))
                {
                    if (!PredefinedItemTypes.Contains(ItemToEdit.ItemType))
                    {
                        PredefinedItemTypes.Insert(0, ItemToEdit.ItemType);
                    }
                    SelectedItemType = ItemToEdit.ItemType;
                }
                if (ItemToEdit.Tags != null)
                {
                    Tags.Clear();
                    foreach (var tag in ItemToEdit.Tags)
                    {
                        Tags.Add(tag);
                    }
                }
            }

            if (string.IsNullOrEmpty(SelectedItemType))
            {
                SelectedItemType = PredefinedItemTypes.FirstOrDefault();
            }
        }

        // This partial method is the magic. It runs every time NewTagText changes.
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
                if (suggestions != null)
                {
                    foreach (var s in suggestions)
                    {
                        TagSuggestions.Add(s);
                    }
                }
            });
        }
    }
}
