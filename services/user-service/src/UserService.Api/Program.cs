using Microsoft.EntityFrameworkCore;
using UserService.Api.Middleware;
using UserService.Application.Interfaces;
using UserService.Application.UseCases;
using UserService.Domain.Repositories;
using UserService.Infrastructure.Persistence;
using UserService.Infrastructure.Persistence.Repositories;
using UserService.Application.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Persistence
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Services (Application)
builder.Services.AddScoped<IUserService, UserService.Application.UseCases.UserService>();

// AutoMapper
builder.Services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());

var app = builder.Build();

// Configure the HTTP request pipeline.
// Global exception handler (must be first in pipeline)
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
