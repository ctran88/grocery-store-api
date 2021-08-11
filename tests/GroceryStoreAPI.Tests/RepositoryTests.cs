using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GroceryStoreAPI.Database;
using GroceryStoreAPI.Models;
using GroceryStoreAPI.Repositories;
using Humanizer;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace GroceryStoreAPI.Tests
{
    public class RepositoryTests
    {
        private readonly Repository<Customer> _sut;
        private readonly IDbContext _dbContext;

        private const int _sampleCount = 5;
        private static readonly string _tableName = nameof(Customer).Pluralize().ToLower();
        private readonly Dictionary<string, string> _emptyData = new()
        {
            {
                _tableName, JsonSerializer.Serialize(GetCustomers(0))
            }
        };
        private readonly Dictionary<string, string> _anyData = new()
        {
            {
                _tableName, JsonSerializer.Serialize(GetCustomers(_sampleCount))
            }
        };
        private readonly Dictionary<string, string> _unorderedData = new()
        {
            {
                _tableName, JsonSerializer.Serialize(GetCustomers(_sampleCount, true))
            }
        };

        public RepositoryTests()
        {
            _dbContext = Substitute.For<IDbContext>();
            _sut = new Repository<Customer>(_dbContext);
        }

        private static IEnumerable<Customer> GetCustomers(int count, bool randomIds = false)
        {
            if (randomIds)
            {
                var rnd = new Random();
                return Enumerable.Range(0, count).Select(_ => new Customer { Id = rnd.Next(1, 1000) });
            }
            
            return Enumerable.Range(0, count).Select(i => new Customer { Id = i });
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntities_WhenEntitiesExist()
        {
            _dbContext.Data.Returns(_anyData);
            
            var actual = await _sut.GetAllAsync(CancellationToken.None);

            actual.Should().HaveCount(_sampleCount);
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntitiesInAscendingOrderById_WhenEntitiesHaveIds()
        {
            _dbContext.Data.Returns(_unorderedData);
            
            var actual = await _sut.GetAllAsync(CancellationToken.None);

            actual.Should().HaveCount(_sampleCount);
            actual.Should().BeInAscendingOrder(e => e.Id);
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldReturnAnEmptyArray_WhenThereAreNoEntities()
        {
            _dbContext.Data.Returns(_emptyData);
            
            var actual = await _sut.GetAllAsync(CancellationToken.None);

            actual.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldReturnAnEmptyArray_WhenDbContextDoesNotHaveTable()
        {
            _dbContext.Data.Returns(new Dictionary<string, string>
            {
                {
                    "wrongTableName", JsonSerializer.Serialize(Enumerable.Empty<string>())
                }
            });

            var actual = await _sut.GetAllAsync(CancellationToken.None);

            actual.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldThrowException_WhenDbContextThrowsException()
        {
            _dbContext.Data.Throws<Exception>();

            Func<Task> actual = async () => await _sut.GetAllAsync(CancellationToken.None);

            await actual.Should().ThrowAsync<Exception>();
        }
        
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntityWithMatchingId_WhenEntityExists()
        {
            _dbContext.Data.Returns(_anyData);
            var expectedId = new Random().Next(0, _sampleCount);
            
            var actual = await _sut.GetByIdAsync(expectedId, CancellationToken.None);

            actual.Id.Should().Be(expectedId);
        }
        
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEntityIsNotFound()
        {
            _dbContext.Data.Returns(_anyData);
            var nonExistingId = _sampleCount + 1;
            
            var actual = await _sut.GetByIdAsync(nonExistingId, CancellationToken.None);

            actual.Should().BeNull();
        }
        
        [Fact]
        public async Task GetByIdAsync_ShouldThrowInvalidOperationException_WhenThereAreMoreThanOneMatchingEntities()
        {
            var entities = Enumerable.Range(0, _sampleCount).Select(_ => new Customer { Id = _sampleCount });
            var anyData = new Dictionary<string, string>
            {
                {
                    nameof(Customer).Pluralize().ToLower(), JsonSerializer.Serialize(entities)
                }
            };
            _dbContext.Data.Returns(anyData);
            
            Func<Task> actual = async() => await _sut.GetByIdAsync(_sampleCount, CancellationToken.None);

            await actual.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AddAsync_ShouldReturnTrue_WhenEntityIsSaved()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<IEnumerable<Customer>>()).Returns(true);

            var actual = await _sut.AddAsync(new Customer(), CancellationToken.None);

            actual.Should().BeTrue();
        }
        
        [Fact]
        public async Task AddAsync_ShouldAddEntityWithIncrementedId_WhenEntityIsAdded()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<IEnumerable<Customer>>()).Returns(true);
            var anyEntity = new Customer();
            
            var actual = await _sut.AddAsync(anyEntity, CancellationToken.None);

            actual.Should().BeTrue();
            anyEntity.Id.Should().Be(_sampleCount);
        }
        
        [Fact]
        public async Task AddAsync_ShouldAddEntityWithIncrementedId_WhenEntityIsAdded_AndExistingEntitiesAreNotInSequence()
        {
            _dbContext.Data.Returns(_unorderedData);
            _dbContext.SaveChangesAsync(Arg.Any<Dictionary<string, IEnumerable<Customer>>>()).Returns(true);
            var anyEntity = new Customer();
            var expectedId = JsonSerializer.Deserialize<IEnumerable<Customer>>(_unorderedData[_tableName])!
                .Max(e => e.Id) + 1;
            
            await _sut.AddAsync(anyEntity, CancellationToken.None);
            
            anyEntity.Id.Should().Be(expectedId);
        }
        
        [Fact]
        public async Task AddAsync_ShouldReturnFalse_WhenEntityIsNotAdded()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<Dictionary<string, IEnumerable<Customer>>>()).Returns(false);
            
            var actual = await _sut.AddAsync(new Customer(), CancellationToken.None);
        
            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task AddAsync_ShouldReturnFalse_WhenDbContextThrowsException()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<Dictionary<string, IEnumerable<Customer>>>()).Throws<Exception>();
        
            var actual = await _sut.AddAsync(new Customer(), CancellationToken.None);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldReturnTrue_WhenEntityIsSaved()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<IEnumerable<Customer>>()).Returns(true);

            var actual = await _sut.UpdateAsync(new Customer(), CancellationToken.None);

            actual.Should().BeTrue();
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenEntityIsNotSaved()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<Dictionary<string, IEnumerable<Customer>>>()).Returns(false);

            var actual = await _sut.UpdateAsync(new Customer(), CancellationToken.None);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenEntityIsNotFound()
        {
            _dbContext.Data.Returns(_emptyData);
            var anyEntity = new Customer { Id = _sampleCount };
            
            var actual = await _sut.UpdateAsync(anyEntity, CancellationToken.None);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenDbContextThrowsException()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<Dictionary<string, IEnumerable<Customer>>>()).Throws<Exception>();
        
            var actual = await _sut.UpdateAsync(new Customer { Id = _sampleCount }, CancellationToken.None);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task RemoveAsync_ShouldReturnTrue_WhenEntityIsSaved()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<IEnumerable<Customer>>()).Returns(true);

            var actual = await _sut.RemoveAsync(1, CancellationToken.None);

            actual.Should().BeTrue();
        }
        
        [Fact]
        public async Task RemoveAsync_ShouldReturnFalse_WhenEntityIsNotSaved()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<Dictionary<string, IEnumerable<Customer>>>()).Returns(false);

            var actual = await _sut.RemoveAsync(1, CancellationToken.None);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task RemoveAsync_ShouldReturnFalse_WhenEntityIsNotFound()
        {
            _dbContext.Data.Returns(_emptyData);
            
            var actual = await _sut.RemoveAsync(_sampleCount, CancellationToken.None);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task RemoveAsync_ShouldReturnFalse_WhenDbContextThrowsException()
        {
            _dbContext.Data.Returns(_anyData);
            _dbContext.SaveChangesAsync(Arg.Any<Dictionary<string, IEnumerable<Customer>>>()).Throws<Exception>();
        
            var actual = await _sut.RemoveAsync(1, CancellationToken.None);

            actual.Should().BeFalse();
        }
    }
}