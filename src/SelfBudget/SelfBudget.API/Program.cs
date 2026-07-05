using SelfBudget.API;
using Wolverine;
using Wolverine.Postgresql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


var services = builder.Services;
var configuration = builder.Configuration;

builder.Host.UseWolverine(opts =>
{
    opts.UseRuntimeCompilation();
    opts.UsePostgresqlPersistenceAndTransport(configuration.GetConnectionString("Database"));
});

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
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
