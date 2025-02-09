using GroupAllocator.Client.Clients;
using GroupAllocator.DAL.Entities;
using GroupAllocator.Desktop.States;
using GroupAllocator.Desktop.Commands;
using GroupAllocator.Models.GroupModels;
using GroupAllocator.Models.ServiceModels;
using GroupAllocator.Models.TelegramModels;
using GroupAllocator.Models.TelegramModels.TelegramUserServiceModels;
using GroupAllocator.Models.UserModels;
using GroupAllocator.TelegramUserService;
using Microsoft.VisualBasic;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.IO;
using System.Net.Http.Json;
using System.Windows.Input;

namespace GroupAllocator.Desktop.MVVM.ViewModels
{
    internal class HomeViewModel : BaseViewModel
    {
        private readonly GroupAllocatorClient _client;

        private readonly ApplicationState _appState;

        private readonly BackgroundWorker _updateStateBackgroundWorker;

        private TelegramUserServiceConnector? _connector;

        private int _apiId;

        private string _apiHash = string.Empty;

        private bool _canConnect = false;

        #region BackgroundWorker _updateUsersFromTelegramChatWorker
        private readonly ConcurrentQueue<TelegramChatModel> _queueSelectedChats = new();
        private bool _canUpdateUsersFromTelegramChat = true;
        private readonly BackgroundWorker _updateUsersFromTelegramChatWorker;
        #endregion

        #region Property Status
        private string _status = "Статус";

        public string Status { get => _status; set => Set(ref _status, value); }
        #endregion

        #region Property StatusAddedNewUser
        private string _statusAddedNewUser = "Статус";
        public string StatusAddedNewUser
        {
            get => _statusAddedNewUser;
            set => Set(ref _statusAddedNewUser, value);
        }
        #endregion

        #region Property PhoneNumber
        private string _phoneNumber = string.Empty;

        public string PhoneNumber { get => _phoneNumber; set => Set(ref _phoneNumber, value); }
        #endregion

        #region Property Password
        private string _password = string.Empty;
        public string Password { get => _password; set => Set(ref _password, value); }
        #endregion

        #region Property TelegramChats
        private List<TelegramChatModel> _telegramChats = new List<TelegramChatModel>();

        public List<TelegramChatModel> TelegramChats
        {
            get => _telegramChats;
            set => Set(ref _telegramChats, value);
        }
        #endregion

        #region Property SelectedTelegramChat
        private TelegramChatModel _selectedTelegramChat = new();

        public TelegramChatModel SelectedTelegramChat
        {
            get => _selectedTelegramChat;
            set
            {
                Set(ref _selectedTelegramChat, value);
                _queueSelectedChats.Enqueue(_selectedTelegramChat);
            }
        }
        #endregion

        #region Property TelegramUsersFromSelectedChat
        private List<TelegramUserModel> _telegramUsersFromSelectedChat = new List<TelegramUserModel>();

        public List<TelegramUserModel> TelegramUsersFromSelectedChat
        {
            get => _telegramUsersFromSelectedChat;
            set => Set(ref _telegramUsersFromSelectedChat, value);
        }
        #endregion

        #region Property SelectedTelegramUserFromChat
        private TelegramUserModel _selectedTelegramUserFromChat = new();

        public TelegramUserModel SelectedTelegramUserFromChat
        {
            get => _selectedTelegramUserFromChat;
            set => Set(ref _selectedTelegramUserFromChat, value);
        }
        #endregion

        #region Property NewUserFirstName
        private string _newUserFirstName = string.Empty;

        public string NewUserFirstName { get => _newUserFirstName; set => Set(ref _newUserFirstName, value); }
        #endregion

        #region Property NewUserLastName
        private string _newUserLastName = string.Empty;

        public string NewUserLastName { get => _newUserLastName; set => Set(ref _newUserLastName, value); }
        #endregion

        #region Property Groups

        private List<Group> _groups = new List<Group>();

        public List<Group> Groups
        {
            get => _groups;
            set => Set(ref _groups, value);
        }

        #endregion

        #region Property SelectedGroup

        private Group _selectedGroup = new();

        public Group SelectedGroup
        {
            get => _selectedGroup;
            set => Set(ref _selectedGroup, value);
        }

        #endregion

        #region Property Roles

        private List<UserRole> _roles = new List<UserRole>();

        public List<UserRole> Roles
        {
            get => _roles;
            set => Set(ref _roles, value);
        }

        #endregion

        #region Property SelectedRole

        private UserRole _selectedRole = new();

        public UserRole SelectedRole
        {
            get => _selectedRole;
            set => Set(ref _selectedRole, value);
        }

        #endregion

        #region ConnectToTelegramCommand
        public ICommand ConnectToTelegramCommand { get; set; }

        private async void OnConnectToTelegramCommandExecuted(object parameter)
        {
            if (string.IsNullOrWhiteSpace(PhoneNumber))
            {
                Status = "Введите номер телефона";
                return;
            }
            if (_canConnect)
            {
                try
                {
                    var directory = CreateDirectory("sessions");
                    var sessionPath = CreateFile($"{directory}/{_apiId}.session");
                    _connector = new TelegramUserServiceConnector(_apiId, _apiHash, sessionPath, Verify);
                    if (!await _connector.Connect(PhoneNumber, Password))
                    {
                        Status = "Ошибка";
                        return;
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Status = "Подключен";
                        TelegramChats = _connector.Chats.Values.ToList();
                    });
                    _updateUsersFromTelegramChatWorker.RunWorkerAsync();
                }
                catch (Exception ex)
                {
                    Status = ex.Message;
                }
            }
        }

        private bool CanConnectToTelegramCommandExecute(object parameter) => _canConnect;
        #endregion

        #region RefreshCommand

        public ICommand RefreshCommand { get; set; }

        private async void OnRefreshCommandExecuted(object parameter)
        {
            await UpdateGroups();
            await UpdateRoles();
        }

        #endregion

        #region AddNewUserCommand

        public ICommand AddNewUserCommand { get; set; }

        private async void OnAddNewUserCommandExecuted(object parameter)
        {
            if (SelectedTelegramUserFromChat == null)
            {
                StatusAddedNewUser = "Не выбран пользователь";
                return;
            }
            if (SelectedGroup == null || string.IsNullOrWhiteSpace(SelectedGroup.Name))
            {
                StatusAddedNewUser = "Не выбрана группа";
                return;
            }
            if (SelectedRole == null || string.IsNullOrWhiteSpace(SelectedRole.Name))
            {
                StatusAddedNewUser = "Не выбрана роль";
                return;
            }
            try
            {
                SelectedTelegramUserFromChat.Group = new GroupModel() { Name = SelectedGroup.Name };
                SelectedTelegramUserFromChat.UserRole = new UserRoleModel() { Name = SelectedRole.Name };
                var response = await _client.CreateTelegramUser(SelectedTelegramUserFromChat);
                if (response == null)
                {
                    return;
                }
                if (response.IsSuccessStatusCode)
                {
                    var newUserInGroup = await response.Content.ReadFromJsonAsync<TelegramUser>();
                    if (newUserInGroup != null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            StatusAddedNewUser = $"{newUserInGroup.ChatId} был добавлен в группу {SelectedGroup.Name}";
                        });
                        var userInfo = new UserModel()
                        {
                            FirstName = NewUserFirstName ?? string.Empty,
                            LastName = NewUserLastName ?? string.Empty,
                            TelegramUserModel = SelectedTelegramUserFromChat
                        };
                        var userResponse = await _client.CreateUser(userInfo);
                        if (userResponse.IsSuccessStatusCode)
                        {
                            StatusAddedNewUser += "/Пользователь создан";
                        }
                        else
                        {
                            var err = await userResponse.Content.ReadFromJsonAsync<ErrorResponse>();
                            if (err != null)
                            {
                                StatusAddedNewUser += $"/{err.Message}";
                            }
                        }
                    }
                }
                else
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                    if (errorResponse != null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            StatusAddedNewUser = $"{errorResponse.Message}";
                        });
                    }
                }

            }
            catch (Exception ex)
            {
                StatusAddedNewUser = ex.Message;
            }
        }

        #endregion


        public HomeViewModel(ApplicationState appState)
        {
            _client = appState.Client;
            _appState = appState;

            ConnectToTelegramCommand = new LambdaCommand(OnConnectToTelegramCommandExecuted, CanConnectToTelegramCommandExecute);
            RefreshCommand = new LambdaCommand(OnRefreshCommandExecuted);
            AddNewUserCommand = new LambdaCommand(OnAddNewUserCommandExecuted);

            _updateUsersFromTelegramChatWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };

            _updateStateBackgroundWorker = new BackgroundWorker()
            {
                WorkerSupportsCancellation = true
            };

            _updateUsersFromTelegramChatWorker.DoWork += UpdateUsersFromSelectedChat;
            _updateStateBackgroundWorker.DoWork += UpdateState;

            Initialize();

            _updateStateBackgroundWorker.RunWorkerAsync();
        }

        public void Initialize()
        {
            var apiId = Environment.GetEnvironmentVariable("TELEGRAM_API_ID");
            if (apiId == null)
            {
                Status = "Отсутствует api id";
                return;
            }
            if (!int.TryParse(apiId, out int apiIdValue))
            {
                Status = "Неправильный apiId";
                return;
            }

            var apiHash = Environment.GetEnvironmentVariable("TELEGRAM_API_HASH");

            if (apiHash == null)
            {
                Status = "Отсутствует api hash";
                return;
            }

            _apiId = apiIdValue;
            _apiHash = apiHash!;
            _canConnect = true;
        }

        private string Verify() => Interaction.InputBox("Введите код верификации", "Код верификации");

        private void UpdateUsersFromSelectedChat(object? sender, DoWorkEventArgs e)
        {
            while (_canUpdateUsersFromTelegramChat)
            {
                if (_queueSelectedChats.Count <= 0)
                {
                    continue;
                }
                try
                {
                    if (_queueSelectedChats.TryDequeue(out var chat) && chat != null)
                    {
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Status = "Обновление списка пользователей...";
                            OnPropertyChanged(nameof(TelegramUsersFromSelectedChat));
                        });
                        var cancel = new CancellationTokenSource();
                        cancel.CancelAfter(4000);
                        TelegramUsersFromSelectedChat = _connector!.GetUsersFromChat(chat.Id, cancel.Token).Result;

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            Status = "Список обновлен";
                            OnPropertyChanged(nameof(TelegramUsersFromSelectedChat));
                        });
                    }
                }
                catch (TaskCanceledException ex)
                {
                    _queueSelectedChats.Clear();
                    Status = "Превышен лимит ожидания";
                }
                catch (OperationCanceledException ex)
                {
                    _queueSelectedChats.Clear();
                    Status = "Превышен лимит ожидания";
                }
                catch (Exception ex)
                {
                    _queueSelectedChats.Clear();
                    Status = ex.Message;
                }
            }
        }

        private void UpdateState(object? sender, DoWorkEventArgs e)
        {
            Task.Run(() => UpdateGroups()).Wait();
            Task.Run(() => UpdateRoles()).Wait();
        }

        public async Task UpdateGroups(CancellationToken cancel = default)
        {
            var groups = new List<Group>();

            try
            {
                var response = await _client.GetAllGroups(cancel);

                if (response == null)
                {
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    groups = await response.Content.ReadFromJsonAsync<List<Group>>();
                    if (groups == null)
                    {
                        return;
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Groups = groups;
                    });
                }
            }
            catch (Exception ex)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    StatusAddedNewUser = ex.Message;
                });
            }
        }

        private async Task UpdateRoles(CancellationToken cancel = default)
        {
            var roles = new List<UserRole>();

            try
            {
                var response = await _client.GetAllUserRoles(cancel);

                if (response == null)
                {
                    return;
                }

                if (response.IsSuccessStatusCode)
                {
                    roles = await response.Content.ReadFromJsonAsync<List<UserRole>>();
                    if (roles == null)
                    {
                        return;
                    }

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Roles = roles;
                    });
                }
            }
            catch (Exception ex)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    StatusAddedNewUser = ex.Message;
                });
            }
        }

        public override void Dispose()
        {
            _canUpdateUsersFromTelegramChat = false;
            _updateUsersFromTelegramChatWorker?.Dispose();
            _updateStateBackgroundWorker?.Dispose();
            _connector?.Dispose();
        }

        private string CreateDirectory(string path)
        {
            if (!Directory.Exists($"{path}"))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        private string CreateFile(string fileName)
        {
            if (!File.Exists(fileName))
            {
                using (File.Create(fileName)) ;
            }
            return fileName;
        }
    }
}
