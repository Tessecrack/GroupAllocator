using GroupAllocator.DAL.Entities;

namespace GroupAllocator.DAL.Interfaces.Repositories
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetAll();

        Task<UserRole> Create(UserRole userRole);

        Task<UserRole?> Get(Guid id);

        Task<UserRole?> GetByName(string name);

        Task<UserRole?> Update(UserRole userRole);

        Task<UserRole?> Delete(Guid id);
    }
}
