using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using TodoApi.Api.Middleware;
using TodoApi.Application.Interfaces;
using TodoApi.Application.Services;
using TodoApi.Infrastructure.Persistence;
using TodoApi.Infrastructure.Repositories;
using TodoApi.Api.Models;
using TodoApi.Infrastructure.Auth;
using TodoApi.Infrastructure.Jwt;

var builder = WebApplication.CreateBuilder(args);

#region Database

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        options.UseInMemoryDatabase("TodoApiDb");
    }
    else
    {
        options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});

#endregion

#region Dependency Injection

builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ITodoService, TodoService>();

#endregion

#region Controllers

builder.Services.AddControllers();

#endregion

#region Problem Details

builder.Services.AddProblemDetails();

#endregion

#region CORS

builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

#endregion

#region Health Checks

builder.Services.AddHealthChecks();

#endregion

#region OpenTelemetry

builder.Services.AddOpenTelemetry()
    .WithTracing(tracerProviderBuilder =>
    {
        tracerProviderBuilder
            .SetResourceBuilder(
                ResourceBuilder.CreateDefault()
                    .AddService("TodoApi"))
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddConsoleExporter();
    });

#endregion

#region JWT Authentication

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException("Jwt:Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
                  ?? throw new InvalidOperationException("Jwt:Audience is missing.");

// Bind Jwt settings and register token generator
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

// Determine signing key: prefer PFX RSA when provided (dev/test), otherwise fall back to symmetric key
SecurityKey? issuerSigningKey = null;
if (!string.IsNullOrEmpty(jwtSettings.PfxPath))
{
    // Load certificate and use its public key for validation
    var cert = new X509Certificate2(jwtSettings.PfxPath, jwtSettings.PfxPassword, X509KeyStorageFlags.EphemeralKeySet);
    var publicRsa = cert.GetRSAPublicKey() ?? throw new InvalidOperationException("PFX does not contain an RSA public key.");
    issuerSigningKey = new RsaSecurityKey(publicRsa) { KeyId = cert.Thumbprint };
}
else
{
    var jwtKey = jwtSettings.Key ?? throw new InvalidOperationException("Jwt:Key is missing.");
    issuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,

            IssuerSigningKey = issuerSigningKey
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddSingleton(jwtSettings);
// Register IJwtTokenGenerator: prefer PFX RSA constructor when PfxPath provided
builder.Services.AddSingleton<IJwtTokenGenerator>(sp =>
{
    if (!string.IsNullOrEmpty(jwtSettings.PfxPath))
    {
        return new JwtTokenGenerator(jwtSettings.PfxPath, jwtSettings.PfxPassword ?? string.Empty, jwtSettings.Issuer, jwtSettings.Audience, true);
    }

    return new JwtTokenGenerator(jwtSettings.Key, jwtSettings.Issuer, jwtSettings.Audience);
});

// Register a simple auth service (in-memory). Replace with a real user store in production.
builder.Services.AddSingleton<TodoApi.Application.Interfaces.IAuthService, InMemoryAuthService>();

#endregion

#region Swagger

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Todo API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter JWT token",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            Array.Empty<string>()
        }
    });
});

#endregion

var app = builder.Build();

#region Middleware Pipeline

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
    app.UseMiddleware<GlobalExceptionMiddleware>();
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("ApiPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

#endregion

app.Run();

public partial class Program
{
}
