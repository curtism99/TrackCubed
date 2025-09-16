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

            if (_isFirstAppearance)
            {
                _isFirstAppearance = false;

                // Call BOTH commands on the first appearance of the page.
                // Let them run in parallel for better performance.
                await Task.WhenAll(
                    _viewModel.LoadFilterOptionsCommand.ExecuteAsync(null),
                    _viewModel.RefreshCommand.ExecuteAsync(null)
                );
            }
        }
    }
}
