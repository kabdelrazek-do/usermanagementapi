using UserManagementAPI.Services;
using UserManagementAPI.Middleware;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add User Service
builder.Services.AddScoped<IUserService, UserService>();

// Add Controllers
builder.Services.AddControllers();

// Add Logging
builder.Logging.AddConsole();
builder.Logging.AddDebug();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Configure middleware pipeline in the correct order:
// 1. Error-handling middleware first (to catch all exceptions)
// 2. Authentication middleware next (to validate tokens)
// 3. Logging middleware last (to log all requests and responses)

// Step 1: Error-handling middleware first
app.UseErrorHandling();

// Step 2: Authentication middleware
app.UseAuthenticationMiddleware();

// Step 3: Logging middleware
app.UseRequestLogging();

app.UseHttpsRedirection();

// Map Controllers
app.MapControllers();

app.Run();
