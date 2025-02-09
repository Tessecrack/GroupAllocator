using GroupAllocator.DAL.Context;
using GroupAllocator.DAL.Entities;
using GroupAllocator.DAL.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.DAL.Repositories
{
    public class TelegramUserRepository : ITelegramUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TelegramUserRepository(ApplicationDbContext dbContext) 
        {
            _dbContext = dbContext;
        }

        public async Task<TelegramUser> Create(TelegramUser telegramUser)
        {
            _dbContext.TelegramUsers.Add(telegramUser);
            await _dbContext.SaveChangesAsync();
            return telegramUser;
        }

        public async Task<TelegramUser?> Delete(Guid id)
        {
            var telegramUser = await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == id);
            if (telegramUser == null)
            {
                return null;
            }

            _dbContext.TelegramUsers.Remove(telegramUser);
            await _dbContext.SaveChangesAsync();
            return telegramUser;
        }

        public async Task<TelegramUser?> Get(Guid id)
        {
            var telegramUser = await _dbContext.TelegramUsers
                .Include(u => u.UserRole)
                .Include(u => u.Group)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);
            return telegramUser;
        }

        public async Task<TelegramUser?> GetByTelegramUserId(long telegramUserId)
        {
            var telegramUser = await _dbContext.TelegramUsers
                .Include(u => u.UserRole)
                .Include(u => u.Group)
                .AsNoTracking().FirstOrDefaultAsync(u => u.ChatId == telegramUserId);
            return telegramUser;
        }


        public async Task<IEnumerable<TelegramUser>> GetByGroupId(Guid groupId)
        {
            var telegramUsers = await _dbContext.TelegramUsers
                .Include(u => u.UserRole)
                .Include(u => u.Group)
                .AsNoTracking()
                .Where(u => u.GroupId == groupId)
                .ToListAsync();

            return telegramUsers;
        }

        public async Task<IEnumerable<TelegramUser>> GetAll() => await _dbContext.TelegramUsers.AsNoTracking().ToListAsync();

        public async Task<TelegramUser?> Update(TelegramUser telegramUser)
        {
            var foundTelegramUser = await _dbContext.TelegramUsers.FirstOrDefaultAsync(u => u.Id == telegramUser.Id);
            if (foundTelegramUser == null)
            {
                return null;
            }

            foundTelegramUser.ChatId = telegramUser.ChatId;
            foundTelegramUser.UserRole = telegramUser.UserRole;
            foundTelegramUser.Username = telegramUser.Username;
            foundTelegramUser.FirstName = telegramUser.FirstName;
            foundTelegramUser.LastName = telegramUser.LastName;

            await _dbContext.SaveChangesAsync();
            return foundTelegramUser;
        }
    }
}
