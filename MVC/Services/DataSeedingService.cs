using ABCRetailers.Models;

namespace ABCRetailers.Services
{
    public interface IDataSeedingService
    {
        Task SeedInitialDataAsync();
    }

    public class DataSeedingService : IDataSeedingService
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<DataSeedingService> _logger;

        public DataSeedingService(IFunctionsApi functionsApi, ILogger<DataSeedingService> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        public async Task SeedInitialDataAsync()
        {
            try
            {
                _logger.LogInformation("Starting data seeding...");

                // Check if products already exist
                var existingProducts = await _functionsApi.GetAllEntitiesAsync<Product>("Products");
                if (existingProducts?.Any() == true)
                {
                    _logger.LogInformation("Products already exist, skipping product seeding");
                    return;
                }

                await SeedProductsAsync();
                await SeedSampleCustomersAsync();

                _logger.LogInformation("Data seeding completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during data seeding");
                throw;
            }
        }

        private async Task SeedProductsAsync()
        {
            var products = new List<Product>
            {
                new Product
                {
                    PartitionKey = "Product",
                    RowKey = "PROD001",
                    ProductName = "Wireless Bluetooth Headphones",
                    Description = "High-quality wireless headphones with noise cancellation and 30-hour battery life",
                    Price = 99.99,
                    StockAvailable = 50,
                    ImageUrl = "/images/headphones.jpg",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                },
                new Product
                {
                    PartitionKey = "Product",
                    RowKey = "PROD002",
                    ProductName = "Smartphone X Pro",
                    Description = "Latest smartphone with advanced triple camera system and 5G connectivity",
                    Price = 899.99,
                    StockAvailable = 25,
                    ImageUrl = "/images/smartphone.jpg",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                },
                new Product
                {
                    PartitionKey = "Product",
                    RowKey = "PROD003",
                    ProductName = "Gaming Laptop Elite",
                    Description = "High-performance gaming laptop with RTX graphics and 16GB RAM",
                    Price = 1499.99,
                    StockAvailable = 15,
                    ImageUrl = "/images/laptop.jpg",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                },
                new Product
                {
                    PartitionKey = "Product",
                    RowKey = "PROD004",
                    ProductName = "Smart Fitness Watch",
                    Description = "Advanced fitness tracker with heart rate monitor and GPS",
                    Price = 249.99,
                    StockAvailable = 75,
                    ImageUrl = "/images/smartwatch.jpg",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                },
                new Product
                {
                    PartitionKey = "Product",
                    RowKey = "PROD005",
                    ProductName = "Tablet Mini Pro",
                    Description = "Compact tablet perfect for reading, browsing, and media consumption",
                    Price = 399.99,
                    StockAvailable = 30,
                    ImageUrl = "/images/tablet.jpg",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                },
                new Product
                {
                    PartitionKey = "Product",
                    RowKey = "PROD006",
                    ProductName = "Wireless Earbuds",
                    Description = "True wireless earbuds with charging case and 24-hour battery",
                    Price = 79.99,
                    StockAvailable = 100,
                    ImageUrl = "/images/earbuds.jpg",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                },
                new Product
                {
                    PartitionKey = "Product",
                    RowKey = "PROD007",
                    ProductName = "4K Ultra HD TV",
                    Description = "55-inch 4K Smart TV with HDR and streaming apps",
                    Price = 699.99,
                    StockAvailable = 10,
                    ImageUrl = "/images/tv.jpg",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                },
                new Product
                {
                    PartitionKey = "Product",
                    RowKey = "PROD008",
                    ProductName = "Digital Camera",
                    Description = "Professional mirrorless camera with 4K video and interchangeable lenses",
                    Price = 1299.99,
                    StockAvailable = 8,
                    ImageUrl = "/images/camera.jpg",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                }
            };

            foreach (var product in products)
            {
                try
                {
                    await _functionsApi.AddEntityAsync("Products", product);
                    _logger.LogInformation("Added product: {ProductName}", product.ProductName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add product: {ProductName}", product.ProductName);
                }
            }
        }

        private async Task SeedSampleCustomersAsync()
        {
            var customers = new List<Customer>
            {
                new Customer
                {
                    PartitionKey = "Customer",
                    RowKey = "CUST001",
                    Name = "John",
                    Surname = "Smith",
                    Username = "john.smith",
                    Email = "john.smith@email.com",
                    ShippingAddress = "123 Main Street, Johannesburg, 2000",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                },
                new Customer
                {
                    PartitionKey = "Customer",
                    RowKey = "CUST002",
                    Name = "Sarah",
                    Surname = "Johnson",
                    Username = "sarah.j",
                    Email = "sarah.j@email.com",
                    ShippingAddress = "456 Oak Avenue, Cape Town, 8000",
                    Timestamp = DateTimeOffset.UtcNow,
                    ETag = Azure.ETag.All
                }
            };

            foreach (var customer in customers)
            {
                try
                {
                    await _functionsApi.AddEntityAsync("Customers", customer);
                    _logger.LogInformation("Added customer: {CustomerName}", $"{customer.Name} {customer.Surname}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add customer: {CustomerName}", $"{customer.Name} {customer.Surname}");
                }
            }
        }
    }
}