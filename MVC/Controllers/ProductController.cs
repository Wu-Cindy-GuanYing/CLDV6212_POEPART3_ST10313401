using Microsoft.AspNetCore.Mvc;
using ABCRetailers.Models;
using ABCRetailers.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace ABCRetailers.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IFunctionsApi _functionsApi;
        private readonly ILogger<ProductController> _logger;

        public ProductController(IFunctionsApi functionsApi, ILogger<ProductController> logger)
        {
            _functionsApi = functionsApi;
            _logger = logger;
        }

        // =====================================================
        // ================ ADMIN + CUSTOMER ===================
        // =====================================================

        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var products = await _functionsApi.GetAllEntitiesAsync<Product>("Products");
                return View(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving products");
                TempData["Error"] = "Error retrieving products";
                return View(new List<Product>());
            }
        }

        [Authorize(Roles = "Admin,Customer")]
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Product ID is required";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var product = await _functionsApi.GetEntityAsync<Product>("Products", "Product", id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product details. ID: {ProductId}", id);
                TempData["Error"] = "Error retrieving product details";
                return RedirectToAction(nameof(Index));
            }
        }

        // =====================================================
        // ==================== ADMIN ONLY ======================
        // =====================================================

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            // Manual price parsing for decimal handling
            if (Request.Form.TryGetValue("Price", out var priceFormValue))
            {
                _logger.LogInformation("Raw price from form: '{PriceFormValue}'", priceFormValue.ToString());
                if (double.TryParse(priceFormValue, out var parsedPrice))
                {
                    product.Price = parsedPrice;
                    _logger.LogInformation("Successfully parsed price: {Price}", parsedPrice);
                }
                else
                {
                    _logger.LogWarning("Failed to parse price: {PriceFormValue}", priceFormValue.ToString());
                    ModelState.AddModelError("Price", "Please enter a valid price");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Additional validation
                    if (product.Price <= 0)
                    {
                        ModelState.AddModelError("Price", "Price must be greater than $0.00");
                        return View(product);
                    }

                    if (product.StockAvailable < 0)
                    {
                        ModelState.AddModelError("StockAvailable", "Stock available cannot be negative");
                        return View(product);
                    }

                    // Upload image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _functionsApi.UploadBlobAsync(imageFile, "product-images");
                        product.ImageUrl = imageUrl;
                    }

                    await _functionsApi.AddEntityAsync("Products", product);
                    TempData["Success"] = $"Product '{product.ProductName}' created successfully with price {product.Price:C}!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating product");
                    ModelState.AddModelError("", $"Error creating product: {ex.Message}");
                }
            }

            return View(product);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Product ID is required";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var product = await _functionsApi.GetEntityAsync<Product>("Products", "Product", id);
                if (product == null)
                {
                    TempData["Error"] = "Product not found";
                    return RedirectToAction(nameof(Index));
                }

                return View(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving product for editing. ID: {ProductId}", id);
                TempData["Error"] = "Error loading product for editing";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(Product product, IFormFile? imageFile)
        {
            // Manual price parsing for edit 
            if (Request.Form.TryGetValue("Price", out var priceFormValue))
            {
                if (double.TryParse(priceFormValue, out var parsedPrice))
                {
                    product.Price = parsedPrice;
                    _logger.LogInformation("Edit: Successfully parsed price: {Price}", parsedPrice);
                }
                else
                {
                    ModelState.AddModelError("Price", "Please enter a valid price");
                }
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Get the original product to preserve ETag and ensure existence
                    var originalProduct = await _functionsApi.GetEntityAsync<Product>("Products", "Product", product.RowKey);
                    if (originalProduct == null)
                    {
                        TempData["Error"] = "Product not found";
                        return RedirectToAction(nameof(Index));
                    }

                    // Additional validation
                    if (product.Price <= 0)
                    {
                        ModelState.AddModelError("Price", "Price must be greater than $0.00");
                        return View(product);
                    }

                    if (product.StockAvailable < 0)
                    {
                        ModelState.AddModelError("StockAvailable", "Stock available cannot be negative");
                        return View(product);
                    }

                    // Update fields
                    originalProduct.ProductName = product.ProductName;
                    originalProduct.Description = product.Description;
                    originalProduct.Price = product.Price;
                    originalProduct.StockAvailable = product.StockAvailable;

                    // Upload new image if provided
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var imageUrl = await _functionsApi.UploadBlobAsync(imageFile, "product-images");
                        originalProduct.ImageUrl = imageUrl;
                    }

                    await _functionsApi.UpdateEntityAsync("Products", originalProduct);
                    TempData["Success"] = $"Product '{product.ProductName}' updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating product ID: {ProductId}", product.ProductId);
                    ModelState.AddModelError("", $"Error updating product: {ex.Message}");
                }
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "Product ID is required";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _functionsApi.DeleteEntityAsync("Products", "Product", id);
                TempData["Success"] = "Product deleted successfully!";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting product ID: {ProductId}", id);
                TempData["Error"] = $"Error deleting product: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // =====================================================
        // ==================== AJAX METHODS ===================
        // =====================================================

        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<JsonResult> CheckStock(string productId)
        {
            try
            {
                var product = await _functionsApi.GetEntityAsync<Product>("Products", "Product", productId);
                if (product != null)
                {
                    return Json(new
                    {
                        success = true,
                        productName = product.ProductName,
                        stockAvailable = product.StockAvailable,
                        price = product.Price,
                        imageUrl = product.ImageUrl
                    });
                }
                return Json(new { success = false, message = "Product not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking stock for product ID: {ProductId}", productId);
                return Json(new { success = false, message = "Error retrieving product information" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Customer")]
        public async Task<JsonResult> GetAllProducts()
        {
            try
            {
                var products = await _functionsApi.GetAllEntitiesAsync<Product>("Products");
                return Json(new { success = true, products = products });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all products for AJAX");
                return Json(new { success = false, message = "Error retrieving products" });
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<JsonResult> GetProductInfo(string id)
        {
            try
            {
                var product = await _functionsApi.GetEntityAsync<Product>("Products", "Product", id);
                if (product != null)
                {
                    return Json(new
                    {
                        success = true,
                        product = new
                        {
                            id = product.ProductId,
                            name = product.ProductName,
                            price = product.Price,
                            stock = product.StockAvailable,
                            description = product.Description,
                            imageUrl = product.ImageUrl
                        }
                    });
                }
                return Json(new { success = false, message = "Product not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product info for ID: {ProductId}", id);
                return Json(new { success = false, message = "Error retrieving product information" });
            }
        }
    }
}