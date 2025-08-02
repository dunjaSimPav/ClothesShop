using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClothesShop.Models;

namespace ClothesShop.Repository
{
    public interface IOrderRepository
    {
        Order SaveOrder(Order order);

        Order Remove(long orderId);
        Order UpdateOrder(Order order);
        Task<Order> GetOrderById(long orderId);
        Task<List<Order>> GetOrders();

        Task<List<Order>> GetOrdersPaginatedAdmin(int currentPage, int itemsPerPage, bool shipped);
        Task<long> GetOrdersPaginatedAdminCount(bool shipped);

        Task<List<Order>> GetOrdersByUser(long userId);
    }
}
