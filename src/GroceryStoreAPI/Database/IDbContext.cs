using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryStoreAPI.Database
{
    public interface IDbContext
    {
        public Dictionary<string, string> Data { get; set; }
        Task<bool> SaveChangesAsync<T>(IEnumerable<T> entities);
    }
}