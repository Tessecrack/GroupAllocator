using GroupAllocator.DAL.Entities;

namespace GroupAllocator.DAL.Interfaces.UnitOfWork
{
    public interface IUnitOfWork
    {
        #region roles

        Task<IEnumerable<UserRole>> GetAllUserRoles();

        Task<UserRole> CreateUserRole(UserRole userRole);

        Task<UserRole?> GetUserRole(Guid id);

        Task<UserRole?> GetUserRoleByName(string name);

        Task<UserRole?> UpdateUserRole(UserRole userRole);

        Task<UserRole?> DeleteUserRole(Guid id);

        #endregion

        #region groups

        Task<IEnumerable<Group>> GetAllGroups();

        Task<Group> CreateGroup(Group group);

        Task<Group?> GetGroup(Guid id);

        Task<Group?> GetGroupByName(string name);

        Task<Group?> UpdateGroup(Group group);

        Task<Group?> DeleteGroup(Guid id);

        #endregion

        #region telegram_users

        Task<IEnumerable<TelegramUser>> GetAllTelegramUsers();

        Task<IEnumerable<TelegramUser>> GetTelegramUsersByGroupId(Guid groupId);

        Task<TelegramUser> CreateTelegramUser(TelegramUser telegramUser);

        Task<TelegramUser?> GetTelegramUser(Guid id);

        Task<TelegramUser?> GetTelegramUserByTelegramId(long telegramUserId);

        Task<TelegramUser?> UpdateTelegramUser(TelegramUser telegramUser);

        Task<TelegramUser?> DeleteTelegramUser(Guid id);

        #endregion

        #region users

        Task<IEnumerable<User>> GetAllUsers();

        Task<IEnumerable<User>> GetUsersByGroupId(Guid groupId);

        Task<User> CreateUser(User user);

        Task<User?> GetUser(Guid id);

        Task<User?> UpdateUser(User user);

        Task<User?> DeleteUser(Guid id);

        #endregion
    }
}
