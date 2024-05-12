using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace webapi;

public class LoggingMiddleWare : IAsyncActionFilter
{
    //  private RequestDelegate next;
    // public LoggingMiddleWare(RequestDelegate next) {
    //     this.next = next;
    // }

    // public async Task Invoke(HttpContext context) {
    //     Stream orriginalStream = context.Response.Body;
    //     try {
    //          using(var memStream = new MemoryStream()) {
    //             context.Response.Body = memStream;
    //             await next(context);
    //             memStream.Position = 0;
    //             string repsponseString = new StreamReader(memStream).ReadToEnd();

    //             memStream.Position = 0;
    //             await memStream.CopyToAsync(orriginalStream);
    //             // memStream.Position = 0;
    //             if(context.Response.ContentType?.Contains("application/json") == true) {
    //                 _ = Task.Run(() => {
    //                     try {
    //                         var serializerOptions = new JsonSerializerOptions();
    //                         serializerOptions.WriteIndented = true;
    //                         serializerOptions.PropertyNameCaseInsensitive = true;
    //                         var responseData = JsonSerializer.Deserialize<List<WeatherForecast>>(repsponseString, serializerOptions);
    //                         repsponseString = JsonSerializer.Serialize(responseData, serializerOptions);
    //                         System.Console.WriteLine($"Response Body: {repsponseString}");
    //                     } catch (Exception e) {
    //                         System.Console.WriteLine($"Response Body: {e.Message}");
    //                         System.Console.WriteLine(e);
    //                     }
    //                 });
    //             }
    //         }
    //     } finally {
    //         context.Response.Body = orriginalStream;
    //     }
    // }

    // public void OnActionExecuted(ActionExecutedContext context)
    // {
    //     System.Console.WriteLine($"{nameof(LoggingMiddleWare)}.{nameof(OnActionExecuted)}");
    //     context.HttpContext.Request.Body.Position = 0;
    //     using(var sr = new StreamReader(context.HttpContext.Request.Body)) { 
    //         var json = sr.ReadToEnd();
    //         var controllerType = context.Controller.GetType();
    //         var actionInfo = controllerType.GetMethod(context.ActionDescriptor.DisplayName ?? "Index");
    //         if(actionInfo != null) {
    //             Type? requestTypeForLog = null;
    //             var parameters = actionInfo.GetParameters();
    //             foreach(var parameter in parameters) {
    //                 var fromBodyAnnotation = parameter.GetCustomAttribute(typeof(FromBodyAttribute));
    //                 if(fromBodyAnnotation != null) {
    //                     requestTypeForLog = parameter.ParameterType;
    //                     break;
    //                 }
    //             }
    //             if(requestTypeForLog != null) {
    //                 var requestObjectForLog = JsonSerializer.Deserialize(json, requestTypeForLog);
    //             }
    //         }

    //     }
    // }

    // public void OnActionExecuting(ActionExecutingContext context)
    // {
    // }

    public Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        System.Console.WriteLine($"{nameof(LoggingMiddleWare)}.{nameof(OnActionExecutionAsync)}");
        var responseBodyStream = context.HttpContext.Response.Body;
        context.HttpContext.Response.Body = new MemoryStream();
        return next.Invoke().ContinueWith(t =>
        {
            using (var sr = new StreamReader(context.HttpContext.Response.Body))
            {
                var json = sr.ReadToEnd();
                context.HttpContext.Response.Body.Position = 0;
                context.HttpContext.Response.Body.CopyTo(responseBodyStream);
                context.HttpContext.Response.Body = responseBodyStream;
                var actionInfo = next.Method;
                if (actionInfo != null)
                {
                    Type? requestTypeForLog = null;
                    var parameters = actionInfo.GetParameters();
                    foreach (var parameter in parameters)
                    {
                        var fromBodyAnnotation = parameter.GetCustomAttribute(typeof(FromBodyAttribute));
                        if (fromBodyAnnotation != null)
                        {
                            requestTypeForLog = parameter.ParameterType;
                            break;
                        }
                    }
                    if (requestTypeForLog != null)
                    {
                        var requestObjectForLog = JsonSerializer.Deserialize(json, requestTypeForLog);
                        
                    }
                }

            }
        });


    }
}
