using GroupAllocator.Models.ServiceModels;
using GroupAllocator.DAL.Entities;
using System.Net.Http.Json;
using GroupAllocator.Models.TelegramModels;
using GroupAllocator.Models.UserModels;
using GroupAllocator.Models.GroupModels;

namespace GroupAllocator.Client.Clients
{
    //TODO refactoring; need BaseClient and inherits;

    public class GroupAllocatorClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        private readonly string _accessToken;

        private readonly string _tokenString;

        public GroupAllocatorClient(HttpClient httpClient, GroupAllocatorClientOptions options)
        {
            _httpClient = httpClient;
            _accessToken = options.AccessToken ?? string.Empty;
            _tokenString = $"token={_accessToken}";
        }

        #region roles

        public async Task<HttpResponseMessage> GetAllUserRoles(CancellationToken cancel = default) 
            => await _httpClient.GetAsync($"/api/roles?{_tokenString}", cancel).ConfigureAwait(false);

        public async Task<HttpResponseMessage> GetUserRole(Guid id, CancellationToken cancel = default) 
            => await _httpClient.GetAsync($"/api/roles/{id}?{_tokenString}", cancel);

        public async Task<HttpResponseMessage> DeleteUserRole(Guid id, CancellationToken cancel = default)
            => await _httpClient.DeleteAsync($"/api/roles/{id}?{_tokenString}");

        public async Task<HttpResponseMessage> CreateUserRole(UserRoleModel userRoleModel, CancellationToken cancel = default) 
            => await _httpClient.PostAsJsonAsync<UserRoleModel>($"/api/roles?{_tokenString}", userRoleModel, cancel);

        public async Task<HttpResponseMessage> UpdateUserRole(UserRole userRole, CancellationToken cancel = default)
            => await _httpClient.PutAsJsonAsync<UserRole>($"/api/roles?{_tokenString}", userRole, cancel);

        #endregion

        #region groups
        
        public async Task<HttpResponseMessage> GetAllGroups(CancellationToken cancel = default) 
            => await _httpClient.GetAsync($"/api/groups?{_tokenString}", cancel).ConfigureAwait(false);

        public async Task<HttpResponseMessage> GetGroup(Guid id, CancellationToken cancel = default) 
            => await _httpClient.GetAsync($"/api/groups/{id}?{_tokenString}", cancel);

        public async Task<HttpResponseMessage> DeleteGroup(Guid id, CancellationToken cancel = default)
            => await _httpClient.DeleteAsync($"/api/groups/{id}?{_tokenString}");

        public async Task<HttpResponseMessage> CreateGroup(GroupModel group, CancellationToken cancel = default) 
            => await _httpClient.PostAsJsonAsync<GroupModel>($"/api/groups?{_tokenString}", group, cancel);

        public async Task<HttpResponseMessage> UpdateGroup(Group group, CancellationToken cancel = default)
            => await _httpClient.PutAsJsonAsync<Group>($"/api/groups?{_tokenString}", group, cancel);

        #endregion

        #region telegram_users
        
        public async Task<HttpResponseMessage> GetAllTelegramUsers(CancellationToken cancel = default) 
            => await _httpClient.GetAsync($"/api/telegram_users?{_tokenString}", cancel).ConfigureAwait(false);

        public async Task<HttpResponseMessage> GetTelegramUser(Guid id, CancellationToken cancel = default) 
            => await _httpClient.GetAsync($"/api/telegram_users/{id}?{_tokenString}", cancel);

        public async Task<HttpResponseMessage> DeleteTelegramUser(Guid id, CancellationToken cancel = default)
            => await _httpClient.DeleteAsync($"/api/telegram_users/{id}?{_tokenString}");

        public async Task<HttpResponseMessage> CreateTelegramUser(TelegramUserModel telegramUserModel, CancellationToken cancel = default) 
            => await _httpClient.PostAsJsonAsync<TelegramUserModel>($"/api/telegram_users?{_tokenString}", telegramUserModel, cancel);

        public async Task<HttpResponseMessage> UpdateTelegramUser(TelegramUser telegramUser, CancellationToken cancel = default)
            => await _httpClient.PutAsJsonAsync<TelegramUser>($"/api/telegram_users?{_tokenString}", telegramUser, cancel);

        public async Task<HttpResponseMessage> GetTelegramUsersByGroupId(Guid groupId, CancellationToken cancel = default)
            => await _httpClient.GetAsync($"/api/group_telegram_users/{groupId}?{_tokenString}");

        #endregion

        #region users

        public async Task<HttpResponseMessage> GetAllUsers(CancellationToken cancel = default) 
            => await _httpClient.GetAsync($"/api/users?{_tokenString}", cancel).ConfigureAwait(false);

        public async Task<HttpResponseMessage> GetUser(Guid id, CancellationToken cancel = default) 
            => await _httpClient.GetAsync($"/api/users/{id}?{_tokenString}", cancel);

        public async Task<HttpResponseMessage> DeleteUser(Guid id, CancellationToken cancel = default)
            => await _httpClient.DeleteAsync($"/api/users/{id}?{_tokenString}");

        public async Task<HttpResponseMessage> CreateUser(UserModel userModel, CancellationToken cancel = default) 
            => await _httpClient.PostAsJsonAsync<UserModel>($"/api/users?{_tokenString}", userModel, cancel);

        public async Task<HttpResponseMessage> UpdateUser(User user, CancellationToken cancel = default)
            => await _httpClient.PutAsJsonAsync<User>($"/api/users?{_tokenString}", user, cancel);

        public async Task<HttpResponseMessage> GetUsersByGroupId(Guid groupId, CancellationToken cancel = default)
            => await _httpClient.GetAsync($"/api/group_users/{groupId}?{_tokenString}");

        #endregion

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}