using TrackCubed.Maui.ViewModels;

namespace TrackCubed.Maui.Views;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
    }
}