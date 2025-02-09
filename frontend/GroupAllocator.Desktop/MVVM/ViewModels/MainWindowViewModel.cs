using GroupAllocator.Client.Clients;
using GroupAllocator.Desktop.States;
using GroupAllocator.Desktop.Commands;
using GroupAllocator.Models.ServiceModels;
using System.Net.Http;
using System.Windows.Input;

namespace GroupAllocator.Desktop.MVVM.ViewModels
{
    internal class MainWindowViewModel : BaseViewModel
    {
        private readonly ApplicationState _appState;

        private BaseViewModel _currentView;

        public BaseViewModel CurrentView 
        {
            get => _currentView;
            set => Set(ref _currentView, value);
        }

        public BaseViewModel HomeViewModel { get; set; }

        public ICommand SwitchToHomeViewModelCommand { get; set; }

        private void OnSwitchToHomeViewCommandExecuted(object parameter) => CurrentView = HomeViewModel;


        public BaseViewModel GroupsViewModel { get; set; }

        public ICommand SwitchToGroupsViewModelCommand { get; set; }

        private void OnSwitchToGroupsViewCommandExecuted(object parameter) => CurrentView = GroupsViewModel;


        public BaseViewModel SettingsViewModel { get; set; }

        public ICommand SwitchToSettingsViewModelCommand { get; set; }

        private void OnSwitchToSettingsViewCommandExecuted(object parameter) => CurrentView = SettingsViewModel;



        public ICommand CloseAppCommand { get; set; }

        private void OnCloseAppCommandExecuted(object parameter) => CloseApp();

        public ICommand CollapseAppCommand { get; set; }

        private void OnCollapseAppCommandExecuted(object parameter)
        {
            App.Current.MainWindow.WindowState = System.Windows.WindowState.Minimized;
        }

        public ICommand MaximizeAppCommand { get; set; }

        private void OnMaximizeAppCommandExecuted(object parameter)
        {
            if (App.Current.MainWindow.WindowState == System.Windows.WindowState.Maximized)
            {
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Normal;
            }
            else
            {
                App.Current.MainWindow.WindowState = System.Windows.WindowState.Maximized;
            }
        }

        public MainWindowViewModel()
        {
            var accessApiToken = Environment.GetEnvironmentVariable("ACCESS_API_TOKEN");
            var options = new GroupAllocatorClientOptions(accessApiToken);

            var urlApi = GetConnectionString();

            HttpClient _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(urlApi)
            };

            var client = new GroupAllocatorClient(_httpClient, options);
            _appState = new ApplicationState(client, this);

            HomeViewModel = new HomeViewModel(_appState);
            GroupsViewModel = new GroupsViewModel(_appState);

            SettingsViewModel = new SettingsViewModel(urlApi);

            CloseAppCommand = new LambdaCommand(OnCloseAppCommandExecuted);
            CollapseAppCommand = new LambdaCommand(OnCollapseAppCommandExecuted);
            MaximizeAppCommand = new LambdaCommand(OnMaximizeAppCommandExecuted);

            SwitchToHomeViewModelCommand = new LambdaCommand(OnSwitchToHomeViewCommandExecuted);
            SwitchToGroupsViewModelCommand = new LambdaCommand(OnSwitchToGroupsViewCommandExecuted);
            SwitchToSettingsViewModelCommand = new LambdaCommand(OnSwitchToSettingsViewCommandExecuted);

            CurrentView = HomeViewModel;
        }

        private string GetConnectionString()
        {
            var urlApi = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");
            if (string.IsNullOrWhiteSpace(urlApi))
            {
                return "http://localhost:5043";
            }

            var urls = urlApi.Split(new[] { ';' });
            if (urls.Length == 1)
            {
                return urlApi;
            }
            return urls[0];
        }

        public async Task UpdateHomeVMState()
        {
            await ((HomeViewModel)HomeViewModel).UpdateGroups();
        }

        public Task UpdateGroupsVMState()
        {
            ((GroupsViewModel)GroupsViewModel).UpdateGroupsUsers();
            return Task.CompletedTask;
        }

        private void CloseApp()
        {
            Dispose();
            App.Current.Shutdown();
        }

        public override void Dispose()
        {
            GroupsViewModel?.Dispose();
            HomeViewModel?.Dispose();
            SettingsViewModel?.Dispose();
            _appState?.Dispose();
        }
    }
}
