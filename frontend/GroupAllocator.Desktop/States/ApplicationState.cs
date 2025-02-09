using GroupAllocator.Client.Clients;
using GroupAllocator.Desktop.MVVM.ViewModels;

namespace GroupAllocator.Desktop.States
{
    internal class ApplicationState : IDisposable
    {
        private readonly GroupAllocatorClient _client;

        private readonly MainWindowViewModel _mainWindowViewModel;

        public GroupAllocatorClient Client => _client;

        public ApplicationState(GroupAllocatorClient client, MainWindowViewModel mainWindowViewModel)
        {
            _client = client;
            _mainWindowViewModel = mainWindowViewModel;
        }

        public async Task UpdateHomeVMState() => await _mainWindowViewModel.UpdateHomeVMState();

        public async Task UpdateGroupVMState() => await _mainWindowViewModel.UpdateGroupsVMState();

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
