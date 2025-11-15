using ABCRetailers.Models;
using ABCRetailers.Models.ViewModels;
using ABCRetailers.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ABCRetailers.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<HomeController> _logger;

        public HomeController(IFunctionsApi functionsApi, ILogger<HomeController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        // MAIN HOME PAGE (Public)
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _functionsApi.GetAllEntitiesAsync<Product>("Products") ?? new List<Product>();
                var customers = await _functionsApi.GetAllEntitiesAsync<Customer>("Customers") ?? new List<Customer>();
                var orders = await _functionsApi.GetAllEntitiesAsync<Order>("Orders") ?? new List<Order>();

                var viewModel = new HomeViewModel
                {
                    FeaturedProducts = products.Take(8).ToList(),
                    ProductCount = products.Count,
                    CustomerCount = customers.Count,
                    OrderCount = orders.Count
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load products for Home page.");

                var viewModel = new HomeViewModel
                {
                    FeaturedProducts = new List<Product>(),
                    ProductCount = 0,
                    CustomerCount = 0,
                    OrderCount = 0
                };

                TempData["Error"] = "Could not load products. Please check if Azure Functions are running.";
                return View(viewModel);
            }
        }

  
        // ADMIN DASHBOARD
        
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDashboard()
        {
            try
            {
                var products = await _functionsApi.GetAllEntitiesAsync<Product>("Products") ?? new List<Product>();
                var customers = await _functionsApi.GetAllEntitiesAsync<Customer>("Customers") ?? new List<Customer>();
                var orders = await _functionsApi.GetAllEntitiesAsync<Order>("Orders") ?? new List<Order>();

                var model = new
                {
                    TotalProducts = products.Count,
                    TotalCustomers = customers.Count,
                    TotalOrders = orders.Count,
                    RecentProducts = products.Take(5).ToList(),
                    RecentCustomers = customers.Take(5).ToList()
                };

                ViewBag.AdminSummary = model;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Admin Dashboard data.");
                TempData["Error"] = "Could not load Admin Dashboard data. Please check Azure Functions connectivity.";
                return View();
            }
        }

       
        //  CUSTOMER DASHBOARD
       
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> CustomerDashboard()
        {
            try
            {
                var userEmail = User.Identity?.Name;
                var products = await _functionsApi.GetAllEntitiesAsync<Product>("Products") ?? new List<Product>();
                var orders = await _functionsApi.GetAllEntitiesAsync<Order>("Orders") ?? new List<Order>();

                // Filter orders for current customer if needed
                var customerOrders = orders.Where(o => o.CustomerEmail == userEmail).ToList();

                ViewBag.UserEmail = userEmail;
                ViewBag.AvailableProducts = products.Count;
                ViewBag.CustomerOrderCount = customerOrders.Count;
                ViewBag.FeaturedProducts = products.Take(4).ToList();

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Customer Dashboard data.");
                TempData["Error"] = "Could not load your dashboard. Please try again.";
                return View();
            }
        }
        
       
        //  PRIVACY PAGE (Public)
    
        [AllowAnonymous]
        public IActionResult Privacy() => View();

 
        //  STORAGE INITIALIZATION (Admin)
  
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> InitializeStorage()
        {
            try
            {
                // Test connectivity to all services
                var products = await _functionsApi.GetAllEntitiesAsync<Product>("Products") ?? new List<Product>();
                var customers = await _functionsApi.GetAllEntitiesAsync<Customer>("Customers") ?? new List<Customer>();
                var orders = await _functionsApi.GetAllEntitiesAsync<Order>("Orders") ?? new List<Order>();

                TempData["Success"] = $"Azure Functions connected successfully! Loaded {products.Count} products, {customers.Count} customers, and {orders.Count} orders.";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing storage connectivity");
                TempData["Error"] = $"Failed to connect to Azure Functions: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        
        // HEALTH CHECK (Public)
                [AllowAnonymous]
        public async Task<IActionResult> Health()
        {
            try
            {
                // Test basic connectivity
                var products = await _functionsApi.GetAllEntitiesAsync<Product>("Products");
                var status = products != null ? "Healthy" : "Degraded";

                return Json(new
                {
                    status = status,
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development",
                    productsCount = products?.Count ?? 0,
                    services = new
                    {
                        azureFunctions = products != null ? "Connected" : "Disconnected"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in health check");
                return Json(new
                {
                    status = "Unhealthy",
                    error = ex.Message,
                    timestamp = DateTime.UtcNow,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development"
                });
            }
        }

        
        // ERROR PAGE (Public)
      
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }

        /*
        //temporary fix: bypass azure functions
        // SIMPLE ADMIN DASHBOARD (no Azure Functions)
        [Authorize(Roles = "Admin")]
        public IActionResult AdminDashboard()
        {
            // Simple version without Azure Functions data
            ViewBag.Message = "Welcome to Admin Dashboard";
            return View();
        }

        // SIMPLE CUSTOMER DASHBOARD (no Azure Functions)  
        [Authorize(Roles = "Customer")]
        public IActionResult CustomerDashboard()
        {
            // Simple version without Azure Functions data
            ViewBag.Message = "Welcome to Customer Dashboard";
            return View();
        }

        */
    }
}