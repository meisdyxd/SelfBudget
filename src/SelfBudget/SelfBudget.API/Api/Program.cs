using SelfBudget.API;
using SelfBudget.API.Application.Services;
using Wolverine;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


var services = builder.Services;
var configuration = builder.Configuration;
var host = builder.Host;

host.ConfigureWolverine(configuration);

services
    .AddInfrastructure(configuration)
    .AddApplication(configuration)
    .ConfigureServices(configuration)
    .AddCors(options => options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173", "http://127.0.0.1:5173")
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod()))
    .AddOpenApi()
    .AddSwaggerGen()
    .AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();

    await using var scope = app.Services.CreateAsyncScope();
    var seeder = scope.ServiceProvider.GetRequiredService<DbSeeder>();
    await seeder.SeedAsync();
}

app.UseCors("Frontend");

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
