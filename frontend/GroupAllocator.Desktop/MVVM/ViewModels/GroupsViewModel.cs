using GroupAllocator.Client.Clients;
using GroupAllocator.DAL.Entities;
using GroupAllocator.Desktop.States;
using GroupAllocator.Desktop.Commands;
using GroupAllocator.Models.GroupModels;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Windows.Input;
using System.ComponentModel;
using GroupAllocator.Models.ServiceModels;

namespace GroupAllocator.Desktop.MVVM.ViewModels
{
    internal class GroupsViewModel : BaseViewModel
    {
        private readonly ApplicationState _appState;
        private readonly GroupAllocatorClient _client;


        #region BackgrounWorker _updateUsersFromGroup
        private bool _canUpdateUsersFromGroup = true;
        private bool _needUpdateUsers = false;
        private readonly BackgroundWorker _updateUsersFromGroupWorker;
        #endregion

        #region Property StatusGroups
        private string _statusGroups = "Статус";

        public string StatusGroups { get => _statusGroups; set => Set(ref _statusGroups, value); }
        #endregion

        #region Property StatusAddNewGroup
        private string _statusAddNewGroup = "Статус";

        public string StatusAddNewGroup { get => _statusAddNewGroup; set => Set(ref _statusAddNewGroup, value); }
        #endregion

        #region Property StatusDeleteUser
        private string _statusDeleteUser = "Статус";

        public string StatusUsers
        {
            get => _statusDeleteUser;
            set => Set(ref _statusDeleteUser, value);
        }
        #endregion

        #region Property NewGroupName
        private string _newGroupName = string.Empty;

        public string NewGroupName { get => _newGroupName; set => Set(ref _newGroupName, value); }
        #endregion

        #region Property NewGroupDesc
        private string _newGroupDesc = string.Empty;

        public string NewGroupDesc { get => _newGroupDesc; set => Set(ref _newGroupDesc, value); }
        #endregion

        #region Property Groups
        private ObservableCollection<Group> _groups = new();

        public ObservableCollection<Group> Groups { get => _groups; set => Set(ref _groups, value); }
        #endregion

        #region Property SelectedGroup

        private Group _selectedGroup = new();

        public Group SelectedGroup
        {
            get => _selectedGroup;
            set 
            { 
                Set(ref _selectedGroup, value);
                _needUpdateUsers = true;
            }
        }

        #endregion

        #region Property UsersFromGroup

        private List<User> _usersFromGroup = new();

        public List<User> UsersFromGroup
        {
            get => _usersFromGroup;
            set => Set(ref _usersFromGroup, value);
        }

        #endregion

        #region Property SelectedUserFromGroup

        private User _selectedUserFromGroup = new();

        public User SelectedUserFromGroup
        {
            get => _selectedUserFromGroup;
            set => Set(ref _selectedUserFromGroup, value);
        }

        #endregion

        #region GetAllGroupsCommand

        public ICommand GetAllGroupsCommand { get; set; }

        private async void OnGetAllGroupsCommandExecuted(object parameter)
        {
            if (_client == null)
            {
                return;
            }

            try
            {
                var response = await _client.GetAllGroups();
                if (response.IsSuccessStatusCode)
                {
                    var groups = await response.Content.ReadFromJsonAsync<List<Group>>();
                    if (groups == null)
                    {
                        return;
                    }
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        StatusGroups = "Группы получены";
                        Groups = new ObservableCollection<Group>(groups);
                    });
                }
                else
                {
                    StatusGroups = $"Ошибка: {response.StatusCode}";
                }
            }
            catch (Exception ex)
            {
                StatusGroups = ex.Message;
            }
        }

        #endregion

        #region CreateGroupCommand
        public ICommand CreateGroupCommand { get; set; }

        private async void OnCreateGroupCommandExecuted(object parameter)
        {
            if (string.IsNullOrWhiteSpace(NewGroupName))
            {
                StatusAddNewGroup = "Не задано название";
                return;
            }

            NewGroupDesc ??= string.Empty;

            var groupModel = new GroupModel()
            {
                Name = NewGroupName,
                Description = NewGroupDesc,
            };

            try
            {
                var response = await _client.CreateGroup(groupModel);
                if (response.IsSuccessStatusCode)
                {
                    var newGroup = await response.Content.ReadFromJsonAsync<Group>();
                    if (newGroup == null)
                    {
                        return;
                    }
                    await _appState.UpdateHomeVMState();
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        StatusAddNewGroup = $"Группа '{newGroup.Name}' добавлена";
                        Groups.Add(newGroup);
                    });
                }
            }
            catch (Exception ex)
            {
                StatusAddNewGroup = ex.Message;
            }
        }
        #endregion

        #region DeleteGroupCommand

        public ICommand DeleteGroupCommand { get; set; }

        private async void OnDeleteGroupCommandExecuted(object? parameter)
        {
            if (SelectedGroup == null)
            {
                return;
            }

            try
            {
                var response = await _client.DeleteGroup(SelectedGroup.Id);
                if (response.IsSuccessStatusCode)
                {
                    App.Current.Dispatcher?.Invoke(() =>
                    {
                        UsersFromGroup = new List<User>();
                        StatusGroups = $"Группа {SelectedGroup.Name} успешно удалена";
                    });
                    OnGetAllGroupsCommandExecuted(parameter);
                }
            }
            catch(Exception ex)
            {
                App.Current.Dispatcher?.Invoke(() =>
                {
                    StatusGroups = ex.Message;
                });
            }
        }

        private bool CanDeleteGroupCommandExecute(object? parameter) => SelectedGroup != null;

        #endregion

        #region DeleteUserCommand
        public ICommand DeleteUserCommand { get; set; }

        private async void OnDeleteUserCommandExecuted(object? parameter)
        {
            if (SelectedUserFromGroup == null)
            {
                return;
            }

            try
            {
                var response = await _client.DeleteUser(SelectedUserFromGroup.Id);
                if (response.IsSuccessStatusCode)
                {
                    var responseTelegramDeleteUser = await _client.DeleteTelegramUser(SelectedUserFromGroup.TelegramUserId);

                    if (responseTelegramDeleteUser.IsSuccessStatusCode)
                    {
                        UpdateGroupsUsers();

                        App.Current.Dispatcher.Invoke(() =>
                        {
                            StatusUsers = $"Пользователь {SelectedUserFromGroup.TelegramUser!.ChatId} был удален";
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                App.Current.Dispatcher.Invoke(() =>
                {
                    StatusUsers = $"{ex.Message}";
                });
            }
        }

        private bool CanDeleteUserCommandExecute(object? parameter) => SelectedUserFromGroup != null;

        #endregion

        public GroupsViewModel(ApplicationState appState)
        {
            _appState = appState;
            _client = appState.Client;

            _updateUsersFromGroupWorker = new BackgroundWorker()
            { WorkerSupportsCancellation = true };

            _updateUsersFromGroupWorker.DoWork += UpdateUsersFromGroup;

            GetAllGroupsCommand = new LambdaCommand(OnGetAllGroupsCommandExecuted);
            CreateGroupCommand = new LambdaCommand(OnCreateGroupCommandExecuted);
            DeleteGroupCommand = new LambdaCommand(OnDeleteGroupCommandExecuted, CanDeleteGroupCommandExecute);
            DeleteUserCommand = new LambdaCommand(OnDeleteUserCommandExecuted, CanDeleteUserCommandExecute);

            _updateUsersFromGroupWorker.RunWorkerAsync();
        }

        public void UpdateGroupsUsers()
        {
            _needUpdateUsers = true;
        }

        private void UpdateUsersFromGroup(object? sender, DoWorkEventArgs e)
        {
            while (_canUpdateUsersFromGroup)
            {
                if (SelectedGroup == null)
                {
                    continue;
                }
                if (_needUpdateUsers)
                {
                    try
                    {
                        var response = _client.GetUsersByGroupId(SelectedGroup.Id).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            var usersFromGroup = response.Content.ReadFromJsonAsync<IEnumerable<User>>().Result;
                            App.Current.Dispatcher.Invoke(() =>
                            {
                                if (usersFromGroup == null)
                                {
                                    StatusGroups = "Не удалось получить участников";
                                }
                                else
                                {
                                    UsersFromGroup = usersFromGroup.ToList();
                                    StatusUsers = $"Участники группы {SelectedGroup.Name}";
                                }
                            });

                            _needUpdateUsers = false;
                        }
                        else
                        {
                            var error = response.Content.ReadFromJsonAsync<ErrorResponse>().Result;
                            if (error != null)
                            {
                                App.Current.Dispatcher.Invoke(() =>
                                {
                                    StatusGroups = error.Message;
                                });
                            }

                            _needUpdateUsers = false;
                        }
                    }
                    catch (Exception ex) 
                    {
                        _needUpdateUsers = false;
                        App.Current.Dispatcher.Invoke(() =>
                        {
                            StatusGroups = $"{ex.Message}";
                        });
                    }

                    _needUpdateUsers = false;
                }
            }
        }

        public override void Dispose()
        {
            _canUpdateUsersFromGroup = false;
            _updateUsersFromGroupWorker?.Dispose();
        }
    }
}
