using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GroceryStoreAPI.Database;
using GroceryStoreAPI.Models;
using Humanizer;

namespace GroceryStoreAPI.Repositories
{
    public sealed class Repository<T> : IRepository<T> where T : IEntity
    {
        private readonly IDbContext _dbContext;
        private readonly List<T> _entities;
        private readonly string _tableName = typeof(T).Name.Pluralize().ToLower();

        public Repository(IDbContext dbContext)
        {
            _dbContext = dbContext;
            _entities = GetTableData().ToList();
        }

        public async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken) =>
            await Task.Run(() => _entities.OrderBy(e => e.Id), cancellationToken);

        public async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            await Task.Run(() => _entities.SingleOrDefault(e => e.Id == id), cancellationToken);

        public async Task<bool> AddAsync(T entity, CancellationToken cancellationToken) =>
            await Task.Run(() =>
            {
                var maxId = _entities.Select(e => e.Id).Max();
                
                entity.Id = maxId + 1;
                _entities.Add(entity);
                
                _dbContext.SaveChanges(_entities);
                return true;
            }, cancellationToken);

        public async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken) =>
            await Task.Run(() =>
            {
                var index = GetEntityIndexById(entity.Id);

                if (index == -1)
                {
                    return false;
                }

                _entities[index] = entity;
                _dbContext.SaveChanges(_entities);

                return true;
            }, cancellationToken);

        public async Task<bool> RemoveAsync(int id, CancellationToken cancellationToken) =>
            await Task.Run(() =>
            {
                var index = GetEntityIndexById(id);

                if (index == -1)
                {
                    return false;
                }

                _entities.RemoveAt(index);
                _dbContext.SaveChanges(_entities);

                return true;
            }, cancellationToken);

        private IEnumerable<T> GetTableData()
        {
            var tableData = _dbContext.Data[_tableName];
            return JsonSerializer.Deserialize<IEnumerable<T>>(tableData,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        private int GetEntityIndexById(int id) => _entities.FindIndex(e => e.Id == id);
    }
}