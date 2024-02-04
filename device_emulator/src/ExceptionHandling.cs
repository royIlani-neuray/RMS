/***
** Copyright (C) 2020-2024 neuRay Labs. All rights reserved.
**
** The information and source code contained herein is the exclusive 
** property of neuRay Labs and may not be disclosed, examined, reproduced, redistributed, used in source and binary forms, in whole or in part  
** without explicit written authorization from the company.
**
***/
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

public class WebServiceException : Exception {
    public int StatusCode { get; set; }
    public WebServiceException(string message) : base(message) 
    {
        StatusCode = StatusCodes.Status500InternalServerError;
    }
}

public class NotFoundException : WebServiceException {
    public NotFoundException(string message) : base(message) {
        StatusCode = StatusCodes.Status404NotFound;
    }
}

public class BadRequestException : WebServiceException {
    public BadRequestException(string message) : base(message) {
        StatusCode = StatusCodes.Status400BadRequest;
    }
}


public class ExceptionHandling
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandling> _logger;

    public ExceptionHandling(RequestDelegate next, ILogger<ExceptionHandling> logger)
    {
        _next = next;
        _logger = logger;
    }
    
    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var response = context.Response;

        var errorResponse = new 
        {
            error = exception.Message
        };

        if (exception is WebServiceException)
        {
            WebServiceException webException = (WebServiceException) exception; 
            context.Response.StatusCode = webException.StatusCode;
        }

        _logger.LogError(exception.Message);
        var result = JsonSerializer.Serialize(errorResponse);
        await context.Response.WriteAsync(result);
    }
}