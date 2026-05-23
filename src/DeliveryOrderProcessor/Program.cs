using DeliveryOrderProcessor.Respository;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services.AddSingleton( sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    string connectionString = configuration["CosmosDb:ConnectionString"] 
        ?? throw new InvalidOperationException("CosmosDb:ConnectionString is missing");

    return new CosmosClient(connectionString);
}
);

builder.Services.AddSingleton<IOrderRepository, CosmosOrderRepository>();

builder.Build().Run();
