using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GroceryStoreAPI.Models;
using GroceryStoreAPI.Repositories;

namespace GroceryStoreAPI.Services
{
    public class Service<T> : IService<T> where T : IEntity 
    {
        private readonly IRepository<T> _repository;

        public Service(IRepository<T> repo) => _repository = repo;

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken) =>
            await _repository.GetAllAsync(cancellationToken);

        public virtual async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            await _repository.GetByIdAsync(id, cancellationToken);

        public virtual async Task<bool> AddAsync(T entity, CancellationToken cancellationToken) =>
            await _repository.AddAsync(entity, cancellationToken);

        public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken) =>
            await _repository.UpdateAsync(entity, cancellationToken);

        public virtual async Task<bool> RemoveAsync(int id, CancellationToken cancellationToken) =>
            await _repository.RemoveAsync(id, cancellationToken);
    }
}