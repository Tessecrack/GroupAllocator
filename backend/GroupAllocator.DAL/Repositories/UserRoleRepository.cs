using GroupAllocator.DAL.Context;
using GroupAllocator.DAL.Entities;
using GroupAllocator.DAL.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.DAL.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRoleRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserRole> Create(UserRole userRole)
        {
            _dbContext.UserRoles.Add(userRole);
            await _dbContext.SaveChangesAsync();
            return userRole;
        }

        public async Task<UserRole?> Delete(Guid id)
        {
            var userRole = await _dbContext.UserRoles.FirstOrDefaultAsync(ur => ur.Id == id);

            if (userRole == null)
            {
                return null;
            }

            _dbContext.UserRoles.Remove(userRole);
            await _dbContext.SaveChangesAsync();
            return userRole;
        }

        public async Task<UserRole?> Get(Guid id)
        {
            var userRole = await _dbContext.UserRoles.AsNoTracking().FirstOrDefaultAsync(ur => ur.Id == id);
            return userRole;
        }

        public async Task<UserRole?> GetByName(string name)
        {
            var userRole = await _dbContext.UserRoles.AsNoTracking().FirstOrDefaultAsync(ur => ur.Name == name);
            return userRole;
        }

        public async Task<IEnumerable<UserRole>> GetAll() => await _dbContext.UserRoles.AsNoTracking().ToListAsync();

        public async Task<UserRole?> Update(UserRole userRole)
        {
            var foundRole = await _dbContext.UserRoles.FirstOrDefaultAsync(ur => ur.Id == userRole.Id);

            if (foundRole == null)
            {
                return null;
            }

            foundRole.Name = userRole.Name;
            foundRole.Description = userRole.Description;

            await _dbContext.SaveChangesAsync();
            return foundRole;
        }
    }
}
