using System.Threading.Tasks;

namespace Api.Interfaces
{
    public interface IUnitOfWork
    {
        IUserRepository UserRepository {get;}
        Task<bool> Complete();
        bool HasChanges();
    }
}
