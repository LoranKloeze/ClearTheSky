using System.Text;
using Backend.Web.Data;
using Backend.Web.Models;
using Backend.Web.Services;
using Backend.Web.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();
var dataSource = dataSourceBuilder.Build();
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(dataSource);
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "JWT Authentication",
        Description = "Enter your JWT token in this field",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    };
    c.AddSecurityDefinition("Bearer", securityScheme);
    var securityRequirement = new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    };

    c.AddSecurityRequirement(securityRequirement);
    
});
builder.Services.AddSerilog();
builder.Services.AddControllers();

var configuration = builder.Configuration;
var authOptions = new AuthOptions();
configuration.GetSection(AuthOptions.Auth).Bind(authOptions);
if (string.IsNullOrWhiteSpace(authOptions.PrivateKey))
{
    throw new Exception("Private key is not set, set it in configuration key Auth:PrivateKey");
}
builder.Services
    .AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(x =>
    {
        x.RequireHttpsMetadata = false;
        x.SaveToken = true;
        x.TokenValidationParameters = new TokenValidationParameters
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(authOptions.PrivateKey)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });
builder.Services.AddAuthorization();


builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IFishyFlipService, FishyFlipService>();
builder.Services.AddSingleton<IEncryptionService, EncryptionService>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    var encryptionKey = config["EncryptionSettings:Key"];
    if (string.IsNullOrWhiteSpace(encryptionKey))
    {
        throw new Exception("Encryption key is not set, set it in configuration key EncryptionSettings:Key");
    }
    return new EncryptionService(encryptionKey);
});


var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

