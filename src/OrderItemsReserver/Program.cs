using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Azure.Core;
using OrderItemsReserver.Interfaces;
using OrderItemsReserver.Services;
using Azure.Storage.Blobs;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((hostContext, services) =>
    {
        // Register your services here
        var config = hostContext.Configuration;

        services.AddSingleton(_ =>
        {
            var blobConnectionString = config["BlobConnectionString"] ?? throw new InvalidOperationException("AzureWebJobsStorage not defined");

            var options = new BlobClientOptions
            {
                Retry =
                {
                    MaxRetries = 3,
                    Delay = TimeSpan.FromSeconds(3),
                    MaxDelay = TimeSpan.FromSeconds(15),
                    Mode = RetryMode.Exponential
                }
            };

            return new BlobServiceClient(blobConnectionString, options);
        });

        //services.AddSingleton(_ =>
        //{
        //    var serviceBusConnectionString = config["ServiceBusConnectionString"] ?? throw new InvalidOperationException("ServiceBusConnectionString not defined");

        //    return new ServiceBusClient(serviceBusConnectionString);
        //});


        services.AddScoped<IOrderRequestBlobUploader, OrderRequestBlobUploader>();
        // Add other dependencies as needed
    })
    .Build();


host.Run();
