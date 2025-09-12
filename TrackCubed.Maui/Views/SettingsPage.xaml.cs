using TrackCubed.Maui.ViewModels;

namespace TrackCubed.Maui.Views;

public partial class SettingsPage : ContentPage
{
	public SettingsPage(SettingsPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // When the page appears, execute the command to load the user's info
        if (BindingContext is SettingsPageViewModel vm && vm.LoadUserInformationCommand.CanExecute(null))
        {
            vm.LoadUserInformationCommand.Execute(null);
        }
    }
}