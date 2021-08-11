using FluentValidation.TestHelper;
using GroceryStoreAPI.Models;
using GroceryStoreAPI.Validators;
using Xunit;

namespace GroceryStoreAPI.Tests
{
    public class CustomerDtoValidatorTests
    {
        private readonly CustomerDtoValidator _sut;

        public CustomerDtoValidatorTests()
        {
            _sut = new CustomerDtoValidator();
        }

        [Fact]
        public void CustomerDtoValidator_ShouldHaveNullError_WhenNameIsNull()
        {
            var customer = new CustomerDto { Name = null };
            
            var result = _sut.TestValidate(customer);

            result.ShouldHaveValidationErrorFor(c => c.Name);
        }
        
        [Fact]
        public void CustomerDtoValidator_ShouldHaveNullError_WhenNameIsEmpty()
        {
            var customer = new CustomerDto { Name = string.Empty };
            
            var result = _sut.TestValidate(customer);

            result.ShouldHaveValidationErrorFor(c => c.Name);
        }
        
        [Fact]
        public void CustomerDtoValidator_ShouldHaveNullError_WhenNameIsLongerThanMaxLimit()
        {
            const int maxCharacterLimit = 255;
            var customer = new CustomerDto { Name = new string('a', maxCharacterLimit + 1) };
            
            var result = _sut.TestValidate(customer);

            result.ShouldHaveValidationErrorFor(c => c.Name);
        }
    }
}