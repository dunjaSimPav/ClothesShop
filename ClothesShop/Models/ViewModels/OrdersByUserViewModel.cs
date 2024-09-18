using System.Collections.Generic;

namespace ClothesShop.Models.ViewModels
{
    public class OrdersByUserViewModel
    {
        public List<Order> Orders { get; set; }
        public List<Order> ShippedOrders { get; set; }
    }
}
