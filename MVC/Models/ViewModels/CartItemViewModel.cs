using Azure.Storage.Files.Shares.Models;

namespace ABCRetailers.Models.ViewModels
{
    public class CartItemViewModel
    {
        public string ProductId { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public double UnitPrice { get; set; }
        public double Subtotal => Quantity * UnitPrice;


    }

    
}
