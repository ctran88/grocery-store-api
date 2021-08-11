using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GroceryStoreAPI.Models;
using GroceryStoreAPI.Repositories;
using GroceryStoreAPI.Services;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace GroceryStoreAPI.Tests
{
    public class ServiceTests
    {
        private readonly Service<Customer> _sut;
        private readonly IRepository<Customer> _repository;

        public ServiceTests()
        {
            _repository = Substitute.For<IRepository<Customer>>();
            _sut = new Service<Customer>(_repository);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntities_WhenEntitiesExist()
        {
            const int expectedCount = 5;
            var entities = Enumerable.Range(0, expectedCount).Select(_ => new Customer());
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.GetAllAsync(cancellationToken).Returns(entities);
            
            var actual = await _sut.GetAllAsync(cancellationToken);

            actual.Should().HaveCount(expectedCount);
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllEntitiesInAscendingOrderById_WhenEntitiesHaveIds()
        {
            const int expectedCount = 5;
            var rnd = new Random();
            var entities = Enumerable.Range(0, expectedCount).Select(_ => new Customer { Id = rnd.Next(1, 1000) })
                .OrderBy(e => e.Id);
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.GetAllAsync(cancellationToken).Returns(entities);
            
            var actual = await _sut.GetAllAsync(cancellationToken);

            actual.Should().HaveCount(expectedCount);
            actual.Should().BeInAscendingOrder(e => e.Id);
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldReturnAnEmptyArray_WhenThereAreNoEntities()
        {
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.GetAllAsync(cancellationToken).Returns(Enumerable.Empty<Customer>());
            
            var actual = await _sut.GetAllAsync(cancellationToken);

            actual.Should().BeEmpty();
        }
        
        [Fact]
        public async Task GetAllAsync_ShouldThrowException_WhenRepositoryThrowsException()
        {
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.GetAllAsync(cancellationToken).Throws<Exception>();

            Func<Task> actual = async () => await _sut.GetAllAsync(cancellationToken);

            await actual.Should().ThrowAsync<Exception>();
        }
        
        [Fact]
        public async Task GetByIdAsync_ShouldReturnEntityWithMatchingId_WhenEntityExists()
        {
            var expectedId = Arg.Any<int>();
            var entity = new Customer { Id = expectedId };
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.GetByIdAsync(expectedId, cancellationToken).Returns(entity);
            
            var actual = await _sut.GetByIdAsync(expectedId, cancellationToken);

            actual.Id.Should().Be(expectedId);
        }
        
        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenEntityIsNotFound()
        {
            var anyId = Arg.Any<int>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.GetByIdAsync(anyId, cancellationToken).ReturnsNull();
            
            var actual = await _sut.GetByIdAsync(anyId, cancellationToken);

            actual.Should().BeNull();
        }
        
        [Fact]
        public async Task GetByIdAsync_ShouldThrowInvalidOperationException_WhenThereAreMoreThanOneMatchingEntities()
        {
            var anyId = Arg.Any<int>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.GetByIdAsync(anyId, cancellationToken).Throws<InvalidOperationException>();
            
            Func<Task> actual = async() => await _sut.GetByIdAsync(anyId, cancellationToken);

            await actual.Should().ThrowAsync<InvalidOperationException>();
        }
        
        [Fact]
        public async Task AddAsync_ShouldReturnTrue_WhenEntityIsAdded()
        {
            var anyEntity = Arg.Any<Customer>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.AddAsync(anyEntity, cancellationToken).Returns(true);

            var actual = await _sut.AddAsync(anyEntity, cancellationToken);

            actual.Should().BeTrue();
        }
        
        [Fact]
        public async Task AddAsync_ShouldReturnFalse_WhenEntityIsNotAdded()
        {
            var anyEntity = Arg.Any<Customer>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.AddAsync(anyEntity, cancellationToken).Returns(false);

            var actual = await _sut.AddAsync(anyEntity, cancellationToken);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task AddAsync_ShouldThrowNullReferenceException_WhenRepositoryThrowsNullReferenceException()
        {
            var anyEntity = Arg.Any<Customer>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.AddAsync(anyEntity, cancellationToken).Throws<NullReferenceException>();

            Func<Task> actual = async () => await _sut.AddAsync(anyEntity, cancellationToken);

            await actual.Should().ThrowAsync<NullReferenceException>();
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldReturnTrue_WhenEntityIsUpdated()
        {
            var anyEntity = Arg.Any<Customer>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.UpdateAsync(anyEntity, cancellationToken).Returns(true);

            var actual = await _sut.UpdateAsync(anyEntity, cancellationToken);

            actual.Should().BeTrue();
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenEntityIsNotUpdated()
        {
            var anyEntity = Arg.Any<Customer>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.UpdateAsync(anyEntity, cancellationToken).Returns(false);

            var actual = await _sut.UpdateAsync(anyEntity, cancellationToken);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldReturnFalse_WhenEntityIsNotFound()
        {
            var anyEntity = Arg.Any<Customer>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.UpdateAsync(anyEntity, cancellationToken).Returns(false);

            var actual = await _sut.UpdateAsync(anyEntity, cancellationToken);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task UpdateAsync_ShouldThrowNullReferenceException_WhenRepositoryThrowsNullReferenceException()
        {
            var anyEntity = Arg.Any<Customer>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.UpdateAsync(anyEntity, cancellationToken).Throws<NullReferenceException>();

            Func<Task> actual = async () => await _sut.UpdateAsync(anyEntity, cancellationToken);

            await actual.Should().ThrowAsync<NullReferenceException>();
        }
        
        [Fact]
        public async Task RemoveAsync_ShouldReturnTrue_WhenEntityIsRemoved()
        {
            var anyId = Arg.Any<int>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.RemoveAsync(anyId, cancellationToken).Returns(true);

            var actual = await _sut.RemoveAsync(anyId, cancellationToken);

            actual.Should().BeTrue();
        }
        
        [Fact]
        public async Task RemoveAsync_ShouldReturnFalse_WhenEntityIsNotRemoved()
        {
            var anyId = Arg.Any<int>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.RemoveAsync(anyId, cancellationToken).Returns(false);

            var actual = await _sut.RemoveAsync(anyId, cancellationToken);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task RemoveAsync_ShouldReturnFalse_WhenEntityIsNotFound()
        {
            var anyId = Arg.Any<int>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.RemoveAsync(anyId, cancellationToken).Returns(false);

            var actual = await _sut.RemoveAsync(anyId, cancellationToken);

            actual.Should().BeFalse();
        }
        
        [Fact]
        public async Task RemoveAsync_ShouldThrowNullReferenceException_WhenRepositoryThrowsNullReferenceException()
        {
            var anyId = Arg.Any<int>();
            var cancellationToken = Arg.Any<CancellationToken>();
            _repository.RemoveAsync(anyId, cancellationToken).Throws<NullReferenceException>();

            Func<Task> actual = async () => await _sut.RemoveAsync(anyId, cancellationToken);

            await actual.Should().ThrowAsync<NullReferenceException>();
        }
    }
}