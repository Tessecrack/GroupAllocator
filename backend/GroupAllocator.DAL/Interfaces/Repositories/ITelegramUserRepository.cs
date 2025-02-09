using GroupAllocator.DAL.Entities;

namespace GroupAllocator.DAL.Interfaces.Repositories
{
    public interface ITelegramUserRepository
    {
        Task<IEnumerable<TelegramUser>> GetAll();

        Task<TelegramUser> Create(TelegramUser telegramUser);

        Task<TelegramUser?> Get(Guid id);

        Task<TelegramUser?> GetByTelegramUserId(long telegramUserId);

        Task<TelegramUser?> Update(TelegramUser telegramUser);

        Task<TelegramUser?> Delete(Guid id);

        Task<IEnumerable<TelegramUser>> GetByGroupId(Guid groupId);
    }
}
