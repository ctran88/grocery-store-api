using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using GroceryStoreAPI.Models;
using GroceryStoreAPI.Services;
using Mapster;
using Microsoft.AspNetCore.Mvc;

namespace GroceryStoreAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly IService<Customer> _customerService;
        
        public CustomersController(IService<Customer> customerService) => _customerService = customerService;

        [HttpGet]
        public async Task<IActionResult> GetCustomers(CancellationToken cancellationToken)
        {
            var customers = await _customerService.GetAllAsync(cancellationToken);
            return Ok(customers);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCustomerById(int id, CancellationToken cancellationToken)
        {
            var customer = await _customerService.GetByIdAsync(id, cancellationToken);
            return customer == null ? NotFound() : Ok(customer.Adapt<CustomerDto>());
        }
        
        [HttpPost]
        public async Task<IActionResult> AddCustomer(CustomerDto customerDto, CancellationToken cancellationToken)
        {
            var customer = customerDto.Adapt<Customer>();
            var added = await _customerService.AddAsync(customer, cancellationToken);

            if (!added)
            {
                var message = $"Customer {customer.Name} could not be added";
                return StatusCode((int)HttpStatusCode.InternalServerError, message);
            }
            
            return CreatedAtAction(nameof(GetCustomerById), new { customer.Id }, customer);
        }
        
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCustomer(int id, CustomerDto customerDto, CancellationToken cancellationToken)
        {
            var customer = customerDto.Adapt<Customer>();
            customer.Id = id;
            
            var updated = await _customerService.UpdateAsync(customer, cancellationToken);
            return !updated ? NotFound() : NoContent();
        }
        
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> RemoveCustomer(int id, CancellationToken cancellationToken)
        {
            var removed = await _customerService.RemoveAsync(id, cancellationToken);
            return !removed ? NotFound() : NoContent();
        }
    }
}