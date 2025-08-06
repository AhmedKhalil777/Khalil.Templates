using TestCorp.Application.Mappings;
using TestCorp.Domain.Interfaces;
using TestCorp.Infrastructure.Data;
using TestCorp.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (builder.Configuration.GetValue<bool>("UseLocalDB"))
{
    connectionString = builder.Configuration.GetConnectionString("LocalConnection");
}

builder.Services.AddDbContext<TodoDbContext>(options =>
    options.UseSqlServer(connectionString));

// Configure MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TodoMappingProfile).Assembly));

// Configure AutoMapper
builder.Services.AddAutoMapper(typeof(TodoMappingProfile));

// Configure repositories
builder.Services.AddScoped<ITodoRepository, TodoRepository>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient",
        policy =>
        {
            policy.WithOrigins("https://localhost:5001", "http://localhost:5000")
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Ensure database is created
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<TodoDbContext>();
    context.Database.EnsureCreated();
}

app.UseHttpsRedirection();
app.UseCors("AllowBlazorClient");
app.UseAuthorization();
app.MapControllers();

app.Run();

public partial class Program { } 