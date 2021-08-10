using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GroceryStoreAPI.Middleware
{
    public class ExceptionHandlerFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            
            context.Result = new ObjectResult(new ExceptionResponseMessage())
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            
            context.ExceptionHandled = true;
        }
    }

    public class ExceptionResponseMessage
    {
        public string Message { get; set; } = "Oops, something isn't quite right. Please try again!";
    }
}