//using Microsoft.AspNetCore.Diagnostics;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Data.SqlClient;
//using Microsoft.EntityFrameworkCore;
//using System.ComponentModel.DataAnnotations;

//namespace Ecom.API.Middlewares
//{
//    public class GlobalExceptionHandler : IExceptionHandler
//    {
//        private readonly ILogger<GlobalExceptionHandler> _logger;
//        private readonly IProblemDetailsService _problemDetailsService;

//        public GlobalExceptionHandler(
//            ILogger<GlobalExceptionHandler> logger,
//            IProblemDetailsService problemDetailsService)
//        {
//            _logger = logger;
//            _problemDetailsService = problemDetailsService;
//        }

//        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken ct)
//        {
//            _logger.LogError(exception, "Unhandled exception occurred");

//            var (statusCode, title) = exception switch
//            {
//                SqlException => (StatusCodes.Status503ServiceUnavailable, "Database is temporarily unavailable."),
//                DbUpdateException => (StatusCodes.Status409Conflict, "A data conflict occurred."),
//                _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred.")
//            };

//            httpContext.Response.StatusCode = statusCode;
//            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
//            {
//                HttpContext = httpContext,
//                Exception = exception,
//                ProblemDetails = new ProblemDetails
//                {
//                    Title = title,
//                    Status = statusCode
//                }
//            });
//        }
//    }
//}
