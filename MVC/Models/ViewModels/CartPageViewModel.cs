using System.Collections.Generic;
using System.Linq;

namespace ABCRetailers.Models.ViewModels
{
    public class CartPageViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public double Total => Items.Sum(i => i.Subtotal);
    }
}