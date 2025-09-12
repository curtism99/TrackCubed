using TrackCubed.Maui.ViewModels;

namespace TrackCubed.Maui.Views;

public partial class AddCubedItemPage : ContentPage
{
	public AddCubedItemPage(AddCubedItemViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // When the page appears, load the dynamic item types.
        if (BindingContext is AddCubedItemViewModel vm && vm.InitializePageCommand.CanExecute(null))
        {
            await vm.InitializePageCommand.ExecuteAsync(null);
        }
    }
}