using Microsoft.EntityFrameworkCore;
using fintrack_database.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using fintrack_api_business_logic;
using fintrack_common.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using fintrack_common.Providers;
using System.Security.Claims;
using fintrack_api.Middlewares;
using fintrack_common.Repositories;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyHeader();
            builder.AllowAnyOrigin();
            builder.AllowAnyMethod();
        }
    );
});

builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(BusinessLogicAssemblyMarker).Assembly);
});

builder.Services.Configure<AuthenticationConfiguration>(config =>
{
    config.Secret = builder.Configuration.GetSection("JWT_SECRET").Value!;
    config.Lifetime = 10;
    config.Auth0Authority = builder.Configuration.GetSection("AUTH0_Authority").Value!;
    config.Auth0Audience = builder.Configuration.GetSection("AUTH0_Audience").Value!;
});

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });

    // Bearer token authentication
    OpenApiSecurityScheme securityDefinition = new OpenApiSecurityScheme()
    {
        Name = "Bearer",
        BearerFormat = "JWT",
        Scheme = "bearer",
        Description = "Specify the authorization token.",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
    };
    c.AddSecurityDefinition("jwt_auth", securityDefinition);

    // Make sure swagger UI requires a Bearer token specified
    OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme()
    {
        Reference = new OpenApiReference()
        {
            Id = "jwt_auth",
            Type = ReferenceType.SecurityScheme
        }
    };
    OpenApiSecurityRequirement securityRequirements = new OpenApiSecurityRequirement()
{
    {securityScheme, new string[] { }},
};
    c.AddSecurityRequirement(securityRequirements);
});

string databaseConnString = builder.Configuration.GetValue<string>("DATABASE_CONN_STRING")!;

builder.Services.AddDbContext<FinTrackDatabaseContext>(options =>
{
    options.UseMySql(databaseConnString, ServerVersion.AutoDetect(databaseConnString));
});

builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();
builder.Services.AddTransient<IRecordRepository, RecordRepository>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = builder.Configuration.GetSection("AUTH0_Authority").Value!;
    options.Audience = builder.Configuration.GetSection("AUTH0_Audience").Value!;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

var app = builder.Build();

var maxMigrationRetries = 10;
var retryDelay = TimeSpan.FromSeconds(5);
var migrationRetries = 0;
var logger = app.Services.GetRequiredService<ILogger<Program>>();

while (true)
{
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<FinTrackDatabaseContext>();
            dbContext.Database.Migrate();
        }
        logger.LogInformation("Database migrated successfully.");
        break;
    }
    catch (Exception)
    {
        migrationRetries++;
        logger.LogError("Migration attempt {Retry}/{MaxRetries} failed. Retrying in {Delay} seconds...", migrationRetries, maxMigrationRetries, retryDelay.TotalSeconds);

        if (migrationRetries >= maxMigrationRetries)
        {
            logger.LogCritical("Maximum retry attempts reached. Exiting.");
            throw;
        }

        await Task.Delay(retryDelay);
    }
}

//app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<UserMiddleware>();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment() || true)
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

//foreach (var kvp in builder.Configuration.AsEnumerable())
//{
//    logger.LogInformation("{Key}: {Value}", kvp.Key, kvp.Value);
//}

app.Run();
