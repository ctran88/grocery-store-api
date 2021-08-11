using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace GroceryStoreAPI.Database
{
    public class GroceryStoreDbContext : IDbContext, IDisposable
    {
        private static readonly ReaderWriterLockSlim _lock = new();
        private bool _disposed;
        private readonly string _connectionString;
        private Stream _fileStream;
        private Dictionary<string, string> _data;

        public GroceryStoreDbContext(string connectionString) => _connectionString = connectionString;

        public Dictionary<string, string> Data
        {
            get => _data ??= LoadData();
            set => _data = value;
        }

        public virtual async Task<bool> SaveChangesAsync<T>(IEnumerable<T> entities)
        {
            try
            {
                Dictionary<string, IEnumerable<T>> newData = Data.ToDictionary(key => key.Key, _ => entities);
                var serializedData = JsonSerializer.Serialize(newData,
                    new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    });

                await using var writer = new StreamWriter(_fileStream);
                _fileStream.SetLength(0);
                await writer.WriteAsync(serializedData);
                return true;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                return false;
            }
        }
        
        private Stream LoadFileStream()
        {
            try
            {
                _lock.EnterReadLock();
                return File.Open(_connectionString, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private Dictionary<string, string> LoadData()
        {
            _fileStream ??= LoadFileStream();
            using var jsonDocument = JsonDocument.Parse(_fileStream);
            return jsonDocument.RootElement.EnumerateObject()
                .ToDictionary(property => property.Name, property => property.Value.GetRawText());
        }

        public void Dispose()
        {
            Dispose(true); 
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _fileStream?.Dispose();
            }
                
            _disposed = true;
        }

        ~GroceryStoreDbContext()
        {
            Dispose(false);
        }
    }
}