using GroupAllocator.Models.Constants;
using GroupAllocator.Models.TelegramModels;
using GroupAllocator.Models.TelegramModels.TelegramUserServiceModels;
using GroupAllocator.Models.UserModels;
using TL;

namespace GroupAllocator.TelegramUserService
{
    public class TelegramUserServiceConnector : IDisposable
    {
        private readonly WTelegram.Client _client;

        private Func<string> _onVerify;

        private Dictionary<long, ChatBase> _chats = new();

        public Dictionary<long, TelegramChatModel> Chats { get; private set; } = new();

        public TelegramUserServiceConnector(int apiId, string apiHash, string sessionPath
            , Func<string> verifyFunction)
        {
            _client = new WTelegram.Client(apiId, apiHash, sessionPath);
            _onVerify = verifyFunction;
        }

        public TelegramUserServiceConnector(int apiId, string apiHash, Func<string> verifyFunction)
        {
            _client = new WTelegram.Client(apiId, apiHash);
            _onVerify = verifyFunction;
        }

        public async Task<bool> Connect(string phoneNumber, string password)
        {
            string? logInfo = phoneNumber;
            while (_client.User == null)
            {
                var strLogin = await _client.Login(logInfo);
                switch (strLogin)
                {
                    case "verification_code": logInfo = _onVerify.Invoke(); break;
                    case "password": logInfo = password; break;
                    default: logInfo = null; break;
                }
            }

            if (_client.User == null)
            {
                return false;
            }

            var chats = await _client.Messages_GetAllChats();
            _chats = chats.chats;

            foreach (var (id, chat) in _chats)
            {
                if (chat == null || !chat.IsActive) continue;

                if (!Chats.ContainsKey(id))
                {
                    Chats.Add(id, new TelegramChatModel()
                    {
                        Id = id,
                        Title = chat.Title,
                    });
                }
            }

            return true;
        }

        public async Task<List<TelegramUserModel>> GetUsersFromChat(long chatId, CancellationToken token = default)
        {
            token.ThrowIfCancellationRequested();
            var users = new List<TelegramUserModel>();

            if (!_chats.TryGetValue(chatId, out var chat))
            {
                return users;
            }
            var chatUsers = chat is Channel channel
                ? (await _client.Channels_GetAllParticipants(channel, cancellationToken: token)).users
                : (await _client.Messages_GetFullChat(chat.ID)).users;
            foreach (var user in chatUsers.Values)
            {
                var model = new TelegramUserModel()
                {
                    ChatId = user.ID,
                    PhoneNumber = user.phone,
                    FirstName = user.first_name,
                    LastName = user.last_name,
                    Username = user.MainUsername,
                    UserRole = new UserRoleModel() { Name = Role.PARTICIPANT }
                };

                users.Add(model);
            }
            return users;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
