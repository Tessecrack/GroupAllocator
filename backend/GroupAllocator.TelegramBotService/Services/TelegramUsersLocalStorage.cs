using GroupAllocator.DAL.Entities;
using GroupAllocator.DAL.Interfaces.UnitOfWork;
using GroupAllocator.Models.Constants;
using GroupAllocator.TelegramBotService.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;

namespace GroupAllocator.TelegramBotService.Services
{
    public class TelegramUsersLocalStorage
    {
        private ConcurrentDictionary<long, OwnerModel> _idsOwners = new();

        private ConcurrentDictionary<long, ParticipantModel> _idsParticipants = new();

        private ConcurrentDictionary<long, AdminModel> _idsAdmins = new();


        private readonly IServiceProvider _serviceProvider;

        public TelegramUsersLocalStorage(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            Task.Run(() => SetInitialState()).Wait();
        }

        public async Task UpdateState(CancellationToken cancel = default) => await SetInitialState(cancel);

        private async Task SetInitialState(CancellationToken cancel = default)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                _idsOwners.Clear();
                _idsParticipants.Clear();

                var db = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                var users = await db.GetAllUsers();

                var groups = GetGroupedUsers(users);

                foreach (var group in groups)
                {
                    int orderNumber = 0;
                    var owner = new OwnerModel();
                    foreach (var user in group.Value)
                    {
                        if (user.TelegramUser == null || user.TelegramUser.UserRole == null)
                        {
                            continue;
                        }

                        switch (user.TelegramUser.UserRole.Name)
                        {
                            case Role.OWNER: 
                                owner.Me = user;
                                if (!_idsOwners.ContainsKey(owner.Me.TelegramUser.ChatId))
                                {
                                    _idsOwners.TryAdd(owner.Me.TelegramUser.ChatId, owner);
                                }
                                break;
                            case Role.PARTICIPANT:
                                ++orderNumber;
                                var participant = new ParticipantModel()
                                { Me = user, Owner = owner.Me, OrderNumber = orderNumber };
                                if (!_idsParticipants.ContainsKey(participant.Me.TelegramUser.ChatId))
                                {                                    
                                    _idsParticipants.TryAdd(participant.Me.TelegramUser.ChatId, participant);
                                }
                                if (!owner.Participants.ContainsKey(user.TelegramUser.ChatId))
                                {
                                    owner.Participants.TryAdd(orderNumber, participant);
                                }
                                break;
                            case Role.ADMIN:
                                if (!_idsAdmins.ContainsKey(user.TelegramUser.ChatId))
                                {
                                    _idsAdmins.TryAdd(user.TelegramUser.ChatId,
                                        new AdminModel()
                                        {
                                            TelegramUser = user.TelegramUser
                                        });
                                }
                                break;
                        }

                        foreach (var (id, part) in owner.Participants)
                        {
                            if (part.Owner == null)
                            {
                                part.Owner = owner.Me;
                            }
                        }
                    }
                }
            }
        }

        private Dictionary<Guid, List<User>> GetGroupedUsers(IEnumerable<User> users)
        {
            var groups = new Dictionary<Guid, List<User>>();

            foreach (var user in users)
            {
                if (user == null || user.TelegramUser == null || user.TelegramUser.Group == null)
                {
                    continue;
                }

                if (groups.ContainsKey(user.TelegramUser.GroupId))
                {
                    groups[user.TelegramUser.GroupId].Add(user);
                }
                else
                {
                    groups.Add(user.TelegramUser.GroupId, new List<User>() { user });
                }
            }

            return groups;
        }

        internal ConcurrentDictionary<long, OwnerModel> IdsOwners => _idsOwners;

        internal ConcurrentDictionary<long, ParticipantModel> IdsParticipants => _idsParticipants;

        internal ConcurrentDictionary<long, AdminModel> IdsAdmins => _idsAdmins;
    }
}
