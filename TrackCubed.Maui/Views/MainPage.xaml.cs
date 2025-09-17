using TrackCubed.Maui.ViewModels;

namespace TrackCubed.Maui.Views
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageViewModel _viewModel;
        private bool _isFirstAppearance = true;

        public MainPage(MainPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // This ensures our initialization logic only runs ONCE,
            // when the app is first started.
            if (_isFirstAppearance)
            {
                _isFirstAppearance = false;
                // Execute the new, consolidated initialization command.
                await _viewModel.InitializeCommand.ExecuteAsync(null);
            }
        }
    }
}
