using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using TravelAdvisor.Application.Exceptions;
using TravelAdvisor.Application.Interfaces;
using ApplicationException = TravelAdvisor.Application.Exceptions.ApplicationException;

namespace TravelAdvisor.API.Middleware
{
    public class ExceptionHandlingMiddleware : IMiddleware
    {
        private readonly IApplicatonLogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(IApplicatonLogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (ApplicationNotFoundException e)
            {
                _logger.LogError(e.Message);
                context.Response.StatusCode = (int) HttpStatusCode.NotFound;
                await context.Response.WriteAsync(e.Message);
                //  await HandleExceptionAsync(context, e);
            }
            catch (ApplicationValidationException e)
            {
                _logger.LogError(e.Message);
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(e.Message);
            }
        }
     
    }
}