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
            // Only trigger the load on the very first time the page appears
            // Although OnAppearing is usually on the UI thread, explicitly dispatching
            // the command is a safer and more robust pattern.
            MainThread.BeginInvokeOnMainThread(() =>
            {
                (BindingContext as MainPageViewModel)?.LoadItemsCommand.Execute(null);
            });
        }
    }
}
