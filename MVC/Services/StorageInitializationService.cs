using ABCRetailers.Models;

namespace ABCRetailers.Services
{
    public class StorageInitializationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<StorageInitializationService> _logger;
        private readonly IConfiguration _configuration;
        private readonly bool _autoInitialize;

        public StorageInitializationService(
            IServiceProvider serviceProvider,
            ILogger<StorageInitializationService> logger,
            IConfiguration configuration)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _configuration = configuration;
            _autoInitialize = _configuration.GetValue<bool>("AzureStorage:AutoInitialize", true);
        }

        public async Task InitializeStorageAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var functionsApi = scope.ServiceProvider.GetRequiredService<IFunctionsApi>();
            var dataSeedingService = scope.ServiceProvider.GetRequiredService<IDataSeedingService>();

            try
            {
                _logger.LogInformation("Initializing Azure storage resources...");

                // Test connectivity to all tables
                var tablesToTest = new[] { "Products", "Customers", "Orders" };
                var healthStatus = new StorageHealthStatus { Timestamp = DateTime.UtcNow, IsHealthy = true };

                foreach (var tableName in tablesToTest)
                {
                    try
                    {
                        switch (tableName)
                        {
                            case "Products":
                                var products = await functionsApi.GetAllEntitiesAsync<Product>(tableName);
                                healthStatus.Tables[tableName] = true;
                                _logger.LogInformation("{Table} table: Connected - {Count} items found", tableName, products?.Count ?? 0);

                                // Seed data if no products exist
                                if (products == null || !products.Any())
                                {
                                    _logger.LogInformation("No products found, seeding sample data...");
                                    await dataSeedingService.SeedInitialDataAsync();
                                }
                                break;
                            case "Customers":
                                var customers = await functionsApi.GetAllEntitiesAsync<Customer>(tableName);
                                healthStatus.Tables[tableName] = true;
                                _logger.LogInformation("{Table} table: Connected - {Count} items found", tableName, customers?.Count ?? 0);
                                break;
                            case "Orders":
                                var orders = await functionsApi.GetAllEntitiesAsync<Order>(tableName);
                                healthStatus.Tables[tableName] = true;
                                _logger.LogInformation("{Table} table: Connected - {Count} items found", tableName, orders?.Count ?? 0);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        healthStatus.Tables[tableName] = false;
                        healthStatus.IsHealthy = false;
                        _logger.LogWarning(ex, "{Table} table: Failed to connect", tableName);
                    }
                }

                if (healthStatus.IsHealthy)
                {
                    _logger.LogInformation("Azure storage initialization completed successfully");
                }
                else
                {
                    _logger.LogWarning("Azure storage initialization completed with some failures");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Azure storage");
                throw;
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_autoInitialize)
            {
                try
                {
                    _logger.LogInformation("Auto-initializing storage...");
                    await InitializeStorageAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Auto-initialization failed");
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}