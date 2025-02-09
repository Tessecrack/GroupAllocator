using GroupAllocator.DAL.Entities;

namespace GroupAllocator.DAL.Interfaces.Repositories
{
    public interface IGroupRepository
    {
        Task<IEnumerable<Group>> GetAll();

        Task<Group> Create(Group group);

        Task<Group?> Get(Guid id);

        Task<Group?> GetByName(string name);

        Task<Group?> Update(Group group);

        Task<Group?> Delete(Guid id);
    }
}
