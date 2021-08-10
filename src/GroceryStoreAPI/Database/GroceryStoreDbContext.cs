using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GroceryStoreAPI.Database
{
    public sealed class GroceryStoreDbContext : IDbContext, IDisposable
    {
        private static readonly ReaderWriterLockSlim _lock = new();
        private bool _disposed;
        private readonly string _connectionString;
        private readonly Stream _fileStream;
        private Dictionary<string, string> _data;

        public GroceryStoreDbContext(string connectionString)
        {
            _connectionString = connectionString;
            _fileStream = LoadFileStream();
        }

        public Dictionary<string, string> Data
        {
            get => _data ??= LoadData();
            set => _data = value;
        }

        public Stream LoadFileStream()
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

        public Dictionary<string, string> LoadData()
        {
            using var jsonDocument = JsonDocument.Parse(_fileStream);
            return jsonDocument.RootElement.EnumerateObject()
                .ToDictionary(property => property.Name, property => property.Value.GetRawText());
        }

        public async Task SaveChanges<T>(IEnumerable<T> entities)
        {
            Dictionary<string, IEnumerable<T>> newData = Data.ToDictionary(key => key.Key, _ => entities);
            var serializedData = JsonSerializer.Serialize(newData,
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });
            
            _fileStream.SetLength(0);
            await using var writer = new StreamWriter(_fileStream);
            await writer.WriteAsync(serializedData);
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