using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GroceryStoreAPI.Database
{
    public interface IDbContext
    {
        public Dictionary<string, string> Data { get; set; }
        Task SaveChanges<T>(IEnumerable<T> entities);
        Stream LoadFileStream();
        Dictionary<string, string> LoadData();
    }
}