using GroupAllocator.DAL.Entities;
using GroupAllocator.DAL.Interfaces.Repositories;
using GroupAllocator.DAL.Interfaces.UnitOfWork;


namespace GroupAllocator.DAL.Unit
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGroupRepository _groupRepository;
        private readonly ITelegramUserRepository _telegramUserDataRepository;

        public UnitOfWork(IUserRoleRepository userRoleRepository, IUserRepository userRepository, IGroupRepository groupRepository
            , ITelegramUserRepository telegramUserDataRepository)
        {
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
            _groupRepository = groupRepository;
            _telegramUserDataRepository = telegramUserDataRepository;
        }

        #region roles

        public async Task<IEnumerable<UserRole>> GetAllUserRoles() => await _userRoleRepository.GetAll();

        public async Task<UserRole> CreateUserRole(UserRole userRole) => await _userRoleRepository.Create(userRole);

        public async Task<UserRole?> GetUserRole(Guid id) => await _userRoleRepository.Get(id);

        public async Task<UserRole?> GetUserRoleByName(string name) => await _userRoleRepository.GetByName(name);

        public async Task<UserRole?> UpdateUserRole(UserRole userRole) => await _userRoleRepository.Update(userRole);

        public async Task<UserRole?> DeleteUserRole(Guid id) => await _userRoleRepository.Delete(id);

        #endregion

        #region groups

        public async Task<IEnumerable<Group>> GetAllGroups() => await _groupRepository.GetAll();

        public async Task<Group> CreateGroup(Group group) => await _groupRepository.Create(group);

        public async Task<Group?> GetGroup(Guid id) => await _groupRepository.Get(id);

        public async Task<Group?> GetGroupByName(string name) => await _groupRepository.GetByName(name);

        public async Task<Group?> UpdateGroup(Group group) => await _groupRepository.Update(group);

        public async Task<Group?> DeleteGroup(Guid id) => await _groupRepository.Delete(id);

        #endregion

        #region telegram_users

        public async Task<IEnumerable<TelegramUser>> GetAllTelegramUsers() 
            => await _telegramUserDataRepository.GetAll();

        public async Task<IEnumerable<TelegramUser>> GetTelegramUsersByGroupId(Guid groupId)
            => await _telegramUserDataRepository.GetByGroupId(groupId);

        public async Task<TelegramUser> CreateTelegramUser(TelegramUser telegramUser) 
            => await _telegramUserDataRepository.Create(telegramUser);

        public async Task<TelegramUser?> GetTelegramUser(Guid id) 
            => await _telegramUserDataRepository.Get(id);

        public async Task<TelegramUser?> GetTelegramUserByTelegramId(long telegramUserId)
            => await _telegramUserDataRepository.GetByTelegramUserId(telegramUserId);

        public async Task<TelegramUser?> UpdateTelegramUser(TelegramUser telegramUser) 
            => await _telegramUserDataRepository.Update(telegramUser);

        public async Task<TelegramUser?> DeleteTelegramUser(Guid id) 
            => await _telegramUserDataRepository.Delete(id);

        #endregion

        #region users

        public async Task<IEnumerable<User>> GetAllUsers() => await _userRepository.GetAll();

        public async Task<IEnumerable<User>> GetUsersByGroupId(Guid groupId) => await _userRepository.GetByGroupId(groupId);

        public async Task<User> CreateUser(User user) => await _userRepository.Create(user);

        public async Task<User?> GetUser(Guid id) => await _userRepository.Get(id);

        public async Task<User?> UpdateUser(User user) => await _userRepository.Update(user);

        public async Task<User?> DeleteUser(Guid id) => await _userRepository.Delete(id);

        #endregion
    }
}
