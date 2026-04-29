using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using TrackCubed.Api.Auth;
using TrackCubed.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var authMode = builder.Configuration["TrackCubed:Auth:Mode"];
if (builder.Environment.IsDevelopment() &&
    authMode?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true)
{
    builder.Services.AddAuthentication(DevelopmentAuthHandler.SchemeName)
        .AddScheme<AuthenticationSchemeOptions, DevelopmentAuthHandler>(
            DevelopmentAuthHandler.SchemeName,
            options => { });
}
else
{
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
            .EnableTokenAcquisitionToCallDownstreamApi()
                .AddDownstreamApi("DownstreamApi", builder.Configuration.GetSection("DownstreamApi"))
                .AddInMemoryTokenCaches();
}


// 1. Get the connection string. Local development uses a dedicated key so older
// Azure SQL user secrets do not accidentally override the Docker SQL database.
var useDevelopmentAuth = builder.Environment.IsDevelopment() &&
    authMode?.Equals("Development", StringComparison.OrdinalIgnoreCase) == true;

var connectionString = useDevelopmentAuth
    ? builder.Configuration["TrackCubed:LocalDevelopment:ConnectionString"]
    : builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("No database connection string is configured.");
}

// 2. Register ApplicationDbContext with the services container.
// This tells the application how to create an instance of your DbContext
// and specifies that it should use SQL Server with the connection string we provided.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlServerOptionsAction: sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure();
    }));

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!useDevelopmentAuth)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
