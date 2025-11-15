using ABCRetailers.Models;

namespace ABCRetailers.Services
{
    public interface IManualStorageInitializationService
    {
        Task InitializeStorageAsync();
    }

    public class ManualStorageInitializationService : IManualStorageInitializationService
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly IDataSeedingService _dataSeedingService;
        private readonly ILogger<ManualStorageInitializationService> _logger;

        public ManualStorageInitializationService(
            IFunctionsApi functionsApi,
            IDataSeedingService dataSeedingService,
            ILogger<ManualStorageInitializationService> logger)
        {
            _functionsApi = functionsApi;
            _dataSeedingService = dataSeedingService;
            _logger = logger;
        }

        public async Task InitializeStorageAsync()
        {
            try
            {
                _logger.LogInformation("Manually initializing storage...");

                // Test connectivity and seed data if needed
                var products = await _functionsApi.GetAllEntitiesAsync<Product>("Products");

                if (products == null || !products.Any())
                {
                    _logger.LogInformation("No products found, seeding data...");
                    await _dataSeedingService.SeedInitialDataAsync();
                }

                _logger.LogInformation("Manual storage initialization completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Manual storage initialization failed");
                throw;
            }
        }
    }
}