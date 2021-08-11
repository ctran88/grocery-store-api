using System;
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
    public class Repository<T> : IRepository<T> where T : IEntity
    {
        private readonly IDbContext _dbContext;
        private readonly string _tableName = typeof(T).Name.Pluralize().ToLower();
        private List<T> _entities;

        public Repository(IDbContext dbContext) => _dbContext = dbContext;

        private List<T> Entities => _entities ??= GetTableData().ToList();

        public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken) =>
            await Task.Run(() => Entities.OrderBy(e => e.Id), cancellationToken);

        public virtual async Task<T> GetByIdAsync(int id, CancellationToken cancellationToken) =>
            await Task.Run(() => Entities.SingleOrDefault(e => e.Id == id), cancellationToken);

        public virtual async Task<bool> AddAsync(T entity, CancellationToken cancellationToken) =>
            await Task.Run(async () =>
            {
                var maxId = Entities.Select(e => e.Id).Max();
                entity.Id = maxId + 1;
                
                var updatedEntities = new List<T>();
                updatedEntities.AddRange(Entities);
                updatedEntities.Add(entity);
                
                var saved = await _dbContext.SaveChangesAsync(updatedEntities);

                if (!saved)
                {
                    return false;
                }
                
                Entities.Add(entity);
                return true;
            }, cancellationToken);

        public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken) =>
            await Task.Run(async () =>
            {
                var index = GetEntityIndexById(entity.Id);

                if (index == -1)
                {
                    return false;
                }
                
                var updatedEntities = new List<T>();
                updatedEntities.AddRange(Entities);
                updatedEntities[index] = entity;
                
                var saved = await _dbContext.SaveChangesAsync(updatedEntities);

                if (!saved)
                {
                    return false;
                }
                
                Entities[index] = entity;
                return true;
            }, cancellationToken);

        public virtual async Task<bool> RemoveAsync(int id, CancellationToken cancellationToken) =>
            await Task.Run(async () =>
            {
                var index = GetEntityIndexById(id);

                if (index == -1)
                {
                    return false;
                }
                
                var updatedEntities = new List<T>();
                updatedEntities.AddRange(Entities);
                updatedEntities.RemoveAt(index);
                
                var saved = await _dbContext.SaveChangesAsync(updatedEntities);

                if (!saved)
                {
                    return false;
                }

                Entities.RemoveAt(index);
                return true;
            }, cancellationToken);

        private IEnumerable<T> GetTableData()
        {
            var success = _dbContext.Data.TryGetValue(_tableName, out string tableData);

            if (success)
            {
                return JsonSerializer.Deserialize<IEnumerable<T>>(tableData,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }

            var added = _dbContext.Data.TryAdd(_tableName, JsonSerializer.Serialize(Enumerable.Empty<T>()));

            if (!added)
            {
                throw new ArgumentException($"Could not add ${_tableName}.");
            }
                
            return Enumerable.Empty<T>();

        }

        private int GetEntityIndexById(int id) => Entities.FindIndex(e => e.Id == id);
    }
}