using Microsoft.AspNetCore.Authentication.JwtBearer;
using dotenv.net;
using ChatHub.Controllers;
using Microsoft.AspNetCore.SignalR;
using MongoDB.Driver;
using Azure.Messaging.ServiceBus;

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.ConfigureAppConfiguration((configBuilder) => {
    configBuilder.Sources.Clear();
    DotEnv.Load();
    configBuilder.AddEnvironmentVariables();
});

builder.Services.AddSingleton<IMongoClient>(sp => {
    string connectionStr = builder.Configuration
            .GetValue<string>("MONGODB_CONNECTION_STR");
    return new MongoClient(connectionStr);
});

builder.Services.AddSingleton<ServiceBusClient>(sp => {
    string clientURL = builder
            .Configuration
            .GetValue<string>("AZURE_SERVICE_BUS_CONNECTION_STR");
    return new ServiceBusClient(clientURL);
});

builder.Services.AddSingleton<IEncryptionHelper>(sp => {
    string key = builder.Configuration
            .GetValue<string>("ENCRYPTION_KEY");
    return new EncryptionHelper(key);
});

builder.Services.AddHostedService<ServiceBusListener>();
builder.Services.AddSingleton<IUserIdProvider, Auth0UserIdProvider>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR()
    .AddMessagePackProtocol();

builder.Services.AddCors(options => {
    string clientUrl = builder.Configuration
        .GetValue<string>("CLIENT_ORIGIN_URL");

    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(clientUrl)
            .AllowAnyHeader()
            .WithMethods("GET", "POST")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
    });
});

builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults
                                        .AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults
                                        .AuthenticationScheme;
})
.AddJwtBearer(options => {
    var configs = builder.Configuration;
    var domain = configs.GetValue<string>("AUTH0_DOMAIN");
    var audience = configs.GetValue<string>("AUTH0_AUDIENCE");

    options.TokenValidationParameters = new Microsoft.IdentityModel
                                    .Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = domain,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true
    };

    options.Authority = $"https://{domain}/";;
    options.Audience = audience;
    options.Events = new JwtBearerEvents {
        OnMessageReceived = context => {
            var path = context.HttpContext.Request.Path;
            var accessToken = context.Request.Query["access_token"];
            string token = context.Request.Headers.Authorization;
            
            if (!string.IsNullOrEmpty(accessToken))
            {
                // Read the token out of the query string
                context.Token = accessToken;
            }
            else if (!string.IsNullOrEmpty(token))
            {
                context.Token = token.Replace("Bearer ", "");
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options => {
    options.AddPolicy("read:messages", 
            policy => policy
                .RequireClaim("scope", "read:messages"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}

string port = app.Configuration
            .GetValue<string>("PORT");
app.Urls.Add($"http://+:{port}");

app.UseHttpsRedirection();
app.UseCors();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<ChatService>("/chatHub");
});

app.MapControllers();
app.Run();