using Microsoft.EntityFrameworkCore;
using fintrack_database.Entities;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
