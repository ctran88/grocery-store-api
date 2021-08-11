using GroceryStoreAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Serilog;

namespace GroceryStoreAPI.Middleware
{
    public class ExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            Log.Error(context.Exception.ToString());
            
            context.Result = new ObjectResult(new ExceptionResponseMessage())
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
            
            context.ExceptionHandled = true;
        }
    }
}