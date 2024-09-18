using System.Linq;
using ClothesShop.Models;

namespace ClothesShop.Repository
{
    public interface IOrderRepository
    {
        IQueryable<Order> Orders { get; }
        Order SaveOrder(Order order);

        Order Remove(int orderId);
        Order UpdateOrder(Order order);
    }
}
