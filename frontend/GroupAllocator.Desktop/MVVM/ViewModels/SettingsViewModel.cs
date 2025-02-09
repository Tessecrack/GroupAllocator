namespace GroupAllocator.Desktop.MVVM.ViewModels
{
    internal class SettingsViewModel : BaseViewModel
    {
        #region Property StringConnectionApi

        private string _stringConnectionApi = string.Empty;

        public string StringConnectionApi
        {
            get => _stringConnectionApi;
            set => Set(ref _stringConnectionApi, value);
        }

        #endregion

        public SettingsViewModel(string stringConnectionApi)
        {
            StringConnectionApi = stringConnectionApi;
        }

        public override void Dispose()
        {
            
        }
    }
}
