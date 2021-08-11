using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using GroceryStoreAPI.Controllers;
using GroceryStoreAPI.Models;
using GroceryStoreAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NSubstitute.ReturnsExtensions;
using Xunit;

namespace GroceryStoreAPI.Tests
{
    public class CustomersControllerTests
    {
        private readonly CustomersController _sut;
        private readonly IService<Customer> _service;

        private const int _sampleCount = 5;
        private const int _customerDtoNameMaxLength = 255;

        public CustomersControllerTests()
        {
            _service = Substitute.For<IService<Customer>>();
            _sut = new CustomersController(_service);
        }

        public static IEnumerable<object[]> MalformedPayload =>
            new List<object[]>
            {
                new object[] { new CustomerDto { Name = null } },
                new object[] { new CustomerDto { Name = string.Empty } },
                new object[] { new CustomerDto { Name = new string('a', _customerDtoNameMaxLength) } }
            };

        [Fact]
        public async Task GetCustomers_ShouldReturnAllCustomers_WhenCustomersExist()
        {
            var anyCustomers = Enumerable.Range(0, _sampleCount).Select(i => new Customer { Id = i });
            _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns(anyCustomers);

            var result = await _sut.GetCustomers(CancellationToken.None);
            var actual = result as OkObjectResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status200OK);
            actual.Value.Should().BeEquivalentTo(anyCustomers);
        }
        
        [Fact]
        public async Task GetCustomers_ShouldReturnEmptyArray_WhenThereAreNoCustomers()
        {
            var anyCustomers = Enumerable.Empty<Customer>();
            _service.GetAllAsync(Arg.Any<CancellationToken>()).Returns(anyCustomers);

            var result = await _sut.GetCustomers(CancellationToken.None);
            var actual = result as OkObjectResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status200OK);
            actual.Value.Should().BeEquivalentTo(anyCustomers);
        }
        
        [Fact]
        public async Task GetCustomers_ShouldThrowAnException_WhenServiceThrowsException()
        {
            _service.GetAllAsync(Arg.Any<CancellationToken>()).Throws<Exception>();

            Func<Task> actual = async () => await _sut.GetCustomers(CancellationToken.None);

            await actual.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task GetCustomerById_ShouldReturnCustomerWithMatchingId_WhenCustomerExists()
        {
            var expected = new CustomerDto { Name = new string('a', _sampleCount) };
            var matchingCustomer = new Customer { Id = _sampleCount, Name = expected.Name };
            _service.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(matchingCustomer);

            var result = await _sut.GetCustomerById(_sampleCount, CancellationToken.None);
            var actual = result as OkObjectResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status200OK);
            actual.Value.Should().BeOfType<CustomerDto>();
            actual.Value.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public async Task GetCustomerById_ShouldReturnNotFound_WhenCustomerDoesNotExist()
        {
            _service.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).ReturnsNull();

            var result = await _sut.GetCustomerById(_sampleCount, CancellationToken.None);
            var actual = result as NotFoundResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [Fact]
        public async Task GetCustomerById_ShouldThrowAnException_WhenServiceThrowsException()
        {
            _service.GetByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Throws<Exception>();

            Func<Task> actual = async () => await _sut.GetCustomerById(_sampleCount, CancellationToken.None);

            await actual.Should().ThrowAsync<Exception>();
        }
        
        [Fact]
        public async Task AddCustomer_ShouldReturnAddedCustomer_WhenCustomerIsAdded()
        {
            var newCustomerDto = new CustomerDto { Name = new string('a', _sampleCount) };
            var expected = new Customer { Id = 0, Name = newCustomerDto.Name };
            _service.AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()).Returns(true);

            var result = await _sut.AddCustomer(newCustomerDto, CancellationToken.None);
            var actual = result as CreatedAtActionResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status201Created);
            actual.RouteValues[nameof(expected.Id)].Should().Be(expected.Id);
            actual.Value.Should().BeOfType<Customer>();
            actual.Value.Should().BeEquivalentTo(expected);
        }
        
        [Fact]
        public async Task AddCustomer_ShouldReturnInternalServiceError_WhenCustomerIsNotAdded()
        {
            _service.AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()).Returns(false);

            var result = await _sut.AddCustomer(new CustomerDto(), CancellationToken.None);
            var actual = result as ObjectResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
        }
        
        [Theory(Skip = "Validation isn't hooking in")]
        [MemberData(nameof(MalformedPayload))]
        public async Task AddCustomer_ShouldReturnBadRequest_WhenPayloadIsMalformed(CustomerDto malformedPayload)
        {
            _service.AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()).Returns(true);

            var result = await _sut.AddCustomer(malformedPayload, CancellationToken.None);
            var actual = result as BadRequestObjectResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
        
        [Fact]
        public async Task AddCustomer_ShouldThrowAnException_WhenServiceThrowsException()
        {
            _service.AddAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()).Throws<Exception>();

            Func<Task> actual = async () => await _sut.AddCustomer(new CustomerDto(), CancellationToken.None);

            await actual.Should().ThrowAsync<Exception>();
        }

        [Fact]
        public async Task UpdateCustomer_ShouldReturnNoContent_WhenCustomerIsUpdated()
        {
            _service.UpdateAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()).Returns(true);

            var result = await _sut.UpdateCustomer(_sampleCount, new CustomerDto(), CancellationToken.None);
            var actual = result as NoContentResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
        
        [Fact]
        public async Task UpdateCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
        {
            _service.UpdateAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()).Returns(false);

            var result = await _sut.UpdateCustomer(_sampleCount, new CustomerDto(), CancellationToken.None);
            var actual = result as NotFoundResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [Theory(Skip = "Validation isn't hooking in")]
        [MemberData(nameof(MalformedPayload))]
        public async Task UpdateCustomer_ShouldReturnBadRequest_WhenPayloadIsMalformed(CustomerDto malformedPayload)
        {
            _service.UpdateAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()).Returns(true);

            var result = await _sut.UpdateCustomer(_sampleCount, malformedPayload, CancellationToken.None);
            var actual = result as BadRequestObjectResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        }
        
        [Fact]
        public async Task UpdateCustomer_ShouldThrowAnException_WhenServiceThrowsException()
        {
            _service.UpdateAsync(Arg.Any<Customer>(), Arg.Any<CancellationToken>()).Throws<Exception>();

            Func<Task> actual = async () => await _sut.UpdateCustomer(_sampleCount, new CustomerDto(), CancellationToken.None);

            await actual.Should().ThrowAsync<Exception>();
        }
        
        [Fact]
        public async Task RemoveCustomer_ShouldReturnNoContent_WhenCustomerIsRemoved()
        {
            _service.RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(true);

            var result = await _sut.RemoveCustomer(_sampleCount, CancellationToken.None);
            var actual = result as NoContentResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status204NoContent);
        }
        
        [Fact]
        public async Task RemoveCustomer_ShouldReturnNotFound_WhenCustomerDoesNotExist()
        {
            _service.RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(false);

            var result = await _sut.RemoveCustomer(_sampleCount, CancellationToken.None);
            var actual = result as NotFoundResult;

            actual.Should().NotBeNull();
            actual.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        }
        
        [Fact]
        public async Task RemoveCustomer_ShouldThrowAnException_WhenServiceThrowsException()
        {
            _service.RemoveAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Throws<Exception>();

            Func<Task> actual = async () => await _sut.RemoveCustomer(_sampleCount, CancellationToken.None);

            await actual.Should().ThrowAsync<Exception>();
        }
    }
}