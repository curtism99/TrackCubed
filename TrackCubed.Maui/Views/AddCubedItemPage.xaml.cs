using TrackCubed.Maui.ViewModels;

namespace TrackCubed.Maui.Views;

public partial class AddCubedItemPage : ContentPage
{
	public AddCubedItemPage(AddCubedItemViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}