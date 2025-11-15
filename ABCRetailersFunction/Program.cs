using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;
using Azure.Storage.Files.Shares;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        var connectionString = "DefaultEndpointsProtocol=https;AccountName=cldv6212storagepoe;AccountKey=PkqZq31D5cGco5IX6j8RzUMoZoDCqbNBhOIco74AOj8RIZY2SbSTTFDRHkDjyJn4pvExGOj0Hdjv+AStqr7iGg==;EndpointSuffix=core.windows.net";

        // Register Azure clients with proper lifetime
        //services.AddSingleton<TableServiceClient>(_ => new TableServiceClient(connectionString));
        //services.AddSingleton<BlobServiceClient>(_ => new BlobServiceClient(connectionString));
        //services.AddSingleton<QueueServiceClient>(_ => new QueueServiceClient(connectionString));
        //services.AddSingleton<ShareServiceClient>(_ => new ShareServiceClient(connectionString));
    })
    .Build();

host.Run();