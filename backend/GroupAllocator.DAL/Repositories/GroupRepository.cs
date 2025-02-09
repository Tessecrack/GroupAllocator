using GroupAllocator.DAL.Context;
using GroupAllocator.DAL.Entities;
using GroupAllocator.DAL.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GroupAllocator.DAL.Repositories
{
    public class GroupRepository : IGroupRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public GroupRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Group> Create(Group group)
        {
            _dbContext.Groups.Add(group);
            await _dbContext.SaveChangesAsync();
            return group;
        }

        public async Task<Group?> Delete(Guid id)
        {
            var group = await _dbContext.Groups.FirstOrDefaultAsync(u => u.Id == id);
            if (group == null)
            {
                return null;
            }

            _dbContext.Groups.Remove(group);
            await _dbContext.SaveChangesAsync();
            return group;
        }

        public async Task<Group?> Get(Guid id)
        {
            var group = await _dbContext.Groups.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            return group;
        }

        public async Task<Group?> GetByName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return null;
            }

            var group = await _dbContext.Groups.AsNoTracking().FirstOrDefaultAsync(g  => g.Name == name);
            return group;
        }

        public async Task<IEnumerable<Group>> GetAll() 
            => await _dbContext.Groups
            .AsNoTracking()
            .ToListAsync();

        public async Task<Group?> Update(Group group)
        {
            var foundGroup = await _dbContext.Groups.FirstOrDefaultAsync(u => u.Id == group.Id);
            if (foundGroup == null)
            {
                return null;
            }

            foundGroup.Name = group.Name;
            foundGroup.Description = group.Description;

            await _dbContext.SaveChangesAsync();
            return foundGroup;
        }
    }
}
