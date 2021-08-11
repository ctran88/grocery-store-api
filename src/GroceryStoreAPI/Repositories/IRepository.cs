using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GroceryStoreAPI.Models;

namespace GroceryStoreAPI.Repositories
{
    public interface IRepository<T> where T : IEntity
    {
        Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken);
        Task<T> GetByIdAsync(int id, CancellationToken cancellationToken);
        Task<bool> AddAsync(T logFile, CancellationToken cancellationToken);
        Task<bool> UpdateAsync(T logFile, CancellationToken cancellationToken);
        Task<bool> RemoveAsync(int id, CancellationToken cancellationToken);
    }
}