using GroupAllocator.DAL.Entities;

namespace GroupAllocator.DAL.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAll();

        Task<IEnumerable<User>> GetByGroupId(Guid id);

        Task<User> Create(User user);

        Task<User?> Get(Guid id);

        Task<User?> Update(User user);

        Task<User?> Delete(Guid id);
    }
}
