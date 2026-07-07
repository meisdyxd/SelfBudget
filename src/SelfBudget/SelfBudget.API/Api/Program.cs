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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
