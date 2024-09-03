using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.ConfigureAppConfiguration((configBuilder) => {
    configBuilder.Sources.Clear();
    DotEnv.Load();
    configBuilder.AddEnvironmentVariables();
});

// Add services to the container.
builder.Services.AddSingleton<IMongoClient>(sp => {
    string connectionStr = builder.Configuration
            .GetValue<string>("MONGODB_CONNECTION_STR");
    return new MongoClient(connectionStr);
});

builder.Services.AddSingleton<IEncryptionHelper>(sp => {
    string key = builder.Configuration
            .GetValue<string>("ENCRYPTION_KEY");
    return new EncryptionHelper(key);
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options => {
    string clientURL = builder.Configuration
                    .GetValue<string>("CLIENT_ORIGIN_URL");

    options.AddDefaultPolicy(policy => {
        policy.WithOrigins(clientURL)
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

builder.Services.AddResponseCompression();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) {
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

app.MapControllers();

app.Run();