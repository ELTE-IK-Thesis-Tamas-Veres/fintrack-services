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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            builder.AllowAnyHeader();
            builder.AllowAnyOrigin();
            builder.AllowAnyMethod();
            /*builder.WithOrigins(
                "*",
                "https://*.graphisoft.hu",
                "http://localhost:4200",
                "https://localhost:4200",
                "https://127.0.0.1:4200",
                "https://127.0.0.1:4200",
                "http://buildforge-local:4200",
                "https://buildforge-local:4200"
                )
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    //.WithExposedHeaders(HeaderNames.Location, "Location")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();*/
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
builder.Services.AddSwaggerGen();

string databaseConnString = builder.Configuration.GetValue<string>("DATABASE_CONN_STRING")!;

builder.Services.AddDbContext<FinTrackDatabaseContext>(options =>
{
    options.UseMySql(databaseConnString, ServerVersion.AutoDetect(databaseConnString));
});

builder.Services.AddTransient<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddTransient<IUserRepository, UserRepository>();
builder.Services.AddTransient<ICategoryRepository, CategoryRepository>();

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

//app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<UserMiddleware>();

app.UseCors();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
