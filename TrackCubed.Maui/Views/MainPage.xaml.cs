using TrackCubed.Maui.ViewModels;

namespace TrackCubed.Maui.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            // Execute the command to load data when the page becomes visible
            if (_viewModel.PageAppearingCommand.CanExecute(null))
            {
                await _viewModel.PageAppearingCommand.ExecuteAsync(null);
            }
        }
    }
}
