using GDB.App.Application.Services.Contracts;
using GDB.App.Application.Services.Implementations;
using GDB.App.Infrastructure.Repositories.Contracts;
using GDB.App.Infrastructure.Repositories.Implementations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register application services and repositories
builder.Services.AddScoped<IHomeService, HomeService>();
builder.Services.AddScoped<IHomeRepository, HomeRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();