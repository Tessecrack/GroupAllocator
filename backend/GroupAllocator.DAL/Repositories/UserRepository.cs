using GroupAllocator.DAL.Context;
using GroupAllocator.DAL.Entities;
using GroupAllocator.DAL.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.DAL.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> Create(User user)
        {
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User?> Delete(Guid id)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return null;
            }

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        public async Task<User?> Get(Guid id)
        {
            var user = await _dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<IEnumerable<User>> GetAll() 
            => await _dbContext.Users
            .Include(u => u.TelegramUser)
                    .ThenInclude(t => t.UserRole)
            .Include(u => u.TelegramUser)
                .ThenInclude(t => t.Group)
            .AsNoTracking().ToListAsync();

        public async Task<IEnumerable<User>> GetByGroupId(Guid id)
        {
            var users = await _dbContext.Users
                .Include(u => u.TelegramUser)
                    .ThenInclude(t => t.UserRole)
                .Include(u => u.TelegramUser)
                    .ThenInclude(t => t.Group)
                .AsNoTracking()
                .Where(u => u.TelegramUser != null && u.TelegramUser.GroupId == id)
                .ToListAsync();

            return users;
        }

        public async Task<User?> Update(User user)
        {
            var foundUser = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            if (foundUser == null)
            {
                return null;
            }

            foundUser.FirstName = user.FirstName;
            foundUser.LastName = user.LastName;

            await _dbContext.SaveChangesAsync();
            return foundUser;
        }
    }
}
