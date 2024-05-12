using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Authentication.BearerToken;
using webapi;
using webapi.Services;
using webapi.Dao.DBDao;
using System.Text;
using Microsoft.AspNetCore.Diagnostics;
using webapi.ClientCommunityModel;
using webapi.Dao;
using webapi.Model.ClientCommunityModel;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var configuration = builder.Configuration;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddResponseCompression((configureCompression) => {
    configureCompression.EnableForHttps = true;
});
var authenticationBuilder = builder.Services.AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
    .AddCookie();

authenticationBuilder.AddBearerToken((configureAuthentication) =>
{
    configureAuthentication.BearerTokenExpiration = TimeSpan.FromMinutes(240);
    configureAuthentication.ClaimsIssuer = configuration.GetValue<string>("Authentication:Issuer") ?? "";
});
    
authenticationBuilder.AddJwtBearer(options => {
    var audience = configuration.GetValue<string>("Authentication:Audience") ?? "";
    var issuer = configuration.GetValue<string>("Authentication:Issuer") ?? "";
    var signingKey = configuration.GetValue<string>("Authentication:SecretKey") ?? "";

    var symmetricSecurityKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
    
    options.Audience = audience;
    options.ClaimsIssuer = issuer;
    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters {
        ValidIssuer = issuer,
        ValidAudience = audience,
        IssuerSigningKey = symmetricSecurityKey,
        ValidateIssuer = true,
        RequireExpirationTime = true,
        ValidateAudience = true,
        RequireAudience = true,
        ValidateLifetime = false,
        RequireSignedTokens = true, 
    };
});

builder.Services.AddMvc(option => {
    option.EnableEndpointRouting = false;
    option.Filters.Add<LoggingMiddleWare>();
})
.AddControllersAsServices()
.AddJsonOptions(jsonConfig => {
    jsonConfig.AllowInputFormatterExceptionMessages = true;
    jsonConfig.JsonSerializerOptions.WriteIndented = true;
    jsonConfig.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserDao>();
builder.Services.AddSingleton<MailDao>();

var connectionString = builder.Configuration.GetValue<string>("DBConnections:MSSQLConnectionString") ?? "";  

if(string.IsNullOrWhiteSpace(connectionString)) {
    throw new ArgumentException("Connection string must not be null or empty");
} 

builder.Services.AddSingleton(typeof(webapi.DBContext.DBContext), new webapi.DBContext.DBContext(connectionString));

if(builder.Environment.IsDevelopment()) {
    builder.Services.AddSwaggerGen();
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseStaticFiles();

app.Logger.LogInformation("Starting application");
app.UseHsts();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseHostFiltering();
app.MapControllers();
app.UseMvc();
app.UseExceptionHandler(exceptionHandlerApp  => {
    exceptionHandlerApp.Run(async context => {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "application/json; charset=utf-8";
        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();
        if(exceptionHandler is not null) {
            if(exceptionHandler.Error is Exception) {
                var logger = context.RequestServices.GetRequiredService<Logger<Exception>>();
                logger.Log(LogLevel.Error, exceptionHandler.Error.Message, exceptionHandler.Error);
            }
        }
        await context.Response.WriteAsJsonAsync(new ClientCommunityModel<object> {
            Code = (long)ReturnCode.ERROR,
            Data = null
        });
    });
});
#if DEBUG
app.Logger.LogInformation("Application is running in DEBUG mode");
#elif RELEASE
app.Logger.LogInformation("Application is running in RELEASE mode");
#endif
app.Run();

class CustomHttpLoggingInterceptor : IHttpLoggingInterceptor
{
    private int random;
    private LocalDataStoreSlot dataStoreSlot;
    public CustomHttpLoggingInterceptor(){
        random = new Random().Next(0, 10000);
        dataStoreSlot = Thread.AllocateNamedDataSlot("response_body_store_slot");
    }
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext logContext)
    {
            System.Console.WriteLine($"Request: {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffffffK")}");
            // logContext.HttpContext.Request.Body.Position = 0;
            Thread.SetData(dataStoreSlot, logContext.HttpContext.Response.Body);
            logContext.HttpContext.Response.Body = new MemoryStream();
            return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        var memoryStream = logContext.HttpContext.Response.Body;
        var bodyStream = Thread.GetData(dataStoreSlot) as Stream;
        logContext.HttpContext.Response.OnCompleted(() => {
            if(logContext.HttpContext.Response.Body.CanRead) {
                logContext.HttpContext.Response.Body.Position = 0;
                logContext.HttpContext.Response.Body = bodyStream;
                memoryStream.CopyTo(bodyStream);
                memoryStream.Position = 0;
                var streamReader = new StreamReader(memoryStream);
                var resBody = streamReader.ReadToEnd();
                streamReader.Close();
                memoryStream.Close();
                bodyStream.Close();
                System.Console.WriteLine($"Response Body: {resBody}");
            }
            return Task.CompletedTask;
        });
        return ValueTask.CompletedTask;
    }
}
