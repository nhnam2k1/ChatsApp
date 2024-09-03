using Auth0.ManagementApi;
using Auth0.ManagementApi.Models;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureAppConfiguration((configBuilder) => {
    configBuilder.Sources.Clear();
    DotEnv.Load();
    configBuilder.AddEnvironmentVariables();
});

// Add services to the container.
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

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddHttpClient();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
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

builder.Services.AddSingleton<IManagementConnection, HttpClientManagementConnection>();
builder.Services.AddSingleton<ManagementApiClient>(sp => 
{
    var domain = configs.GetValue<string>("AUTH0_DOMAIN");
    var clientID = configs.GetValue<string>("AUTH0_CLIENT_ID");
    var clientSecret = configs.GetValue<string>("AUTH0_CLIENT_SECRET");

    var connection = sp.GetRequiredService<IManagementConnection>();
    var tokenClient = new Auth0.AuthenticationApi
                        .AuthenticationApiClient(new Uri($"https://{domain}/"));

    var tokenRequest = tokenClient.GetTokenAsync(new Auth0.AuthenticationApi
                                        .Models.ClientCredentialsTokenRequest {
        ClientId = clientID,
        ClientSecret = clientSecret,
        Audience = $"https://{domain}/api/v2/"
    }).Result;

    return new ManagementApiClient(tokenRequest.AccessToken, 
                domain, connection);
});

builder.Services.AddResponseCompression();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

string port = app.Configuration.GetValue<string>("PORT");
app.Urls.Add($"http://+:{port}");

app.UseHttpsRedirection();
app.UseResponseCompression();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/users", [Authorize] 
    async (HttpRequest request, ILogger<UserService> logger,
    ManagementApiClient managementApiClient) =>
{
    var userService = new UserService(managementApiClient, logger);
    return await userService.GetUsers(request);
})
.WithName("GetUsers")
.RequireAuthorization();

app.Run();