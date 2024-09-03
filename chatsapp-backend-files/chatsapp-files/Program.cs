using Azure.Messaging.ServiceBus;
using AzureServiceBusConsumerWebApi.Services;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((configBuilder) => {
    configBuilder.Sources.Clear();
    DotEnv.Load();
    configBuilder.AddEnvironmentVariables();
});

var configs = builder.Configuration;
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    var domain = configs.GetValue<string>("AUTH0_DOMAIN");
    var audience = $"https://{domain}/api/v2/";

    options.TokenValidationParameters = new Microsoft.IdentityModel
                                    .Tokens.TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = domain,
        ValidateAudience = true,
        ValidAudience = audience,
        ValidateLifetime = true,
    };
    options.Authority = $"https://{domain}/";;
    options.Audience = audience;
    options.Events = new JwtBearerEvents {
        OnMessageReceived = context => {
            string token = context.Request.Headers.Authorization;
            if (!string.IsNullOrEmpty(token)) {
                context.Token = token.Replace("Bearer ", "");
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    string clientURL = builder
            .Configuration
            .GetValue<string>("CLIENT_ORIGIN_URL");

    options.AddDefaultPolicy(policy => {
        policy.WithOrigins(clientURL)
            .AllowAnyHeader()
            .WithMethods("GET")
            .AllowCredentials()
            .SetPreflightMaxAge(TimeSpan.FromSeconds(86400));
    });
});

// Add services to the container.
builder.Services.AddSingleton<ServiceBusClient>(sp => {
    string clientURL = builder
            .Configuration
            .GetValue<string>("AZURE_SERVICE_BUS_CONNECTION_STR");
    return new ServiceBusClient(clientURL);
});

builder.Services.AddHostedService<AzureServiceBusConsumerService>();

builder.Services.AddSingleton<IEncryptionHelper>(sp => {
    string key = builder.Configuration
            .GetValue<string>("ENCRYPTION_KEY");
    return new EncryptionHelper(key);
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddResponseCompression();

builder.Services.AddSingleton<IMongoClient>(sp => {
    string connectionStr = builder
        .Configuration
        .GetValue<string>("MONGODB_CONNECTION_STR");
    return new MongoClient(connectionStr);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string port = app.Configuration
        .GetValue<string>("PORT");
app.Urls.Add($"http://+:{port}");

app.UseHttpsRedirection();
app.UseResponseCompression();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();