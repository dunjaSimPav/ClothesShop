using System.Linq;
using ClothesShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;

namespace ClothesShop.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private DatabaseContext _context;

        private readonly SemaphoreSlim _semaphore = new(1, 1);

        public OrderRepository(DatabaseContext ctx) => _context = ctx;

        public async Task<List<Order>> GetOrders()
        {
            return await _context.Orders
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Article)
                .ToListAsync();
        }

        public async Task<List<Order>> GetOrdersPaginatedAdmin(int currentPage, int itemsPerPage, bool shipped)
        {
            await _semaphore.WaitAsync();
            var orders = await _context.Orders
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Article)
                .AsNoTracking()
                .Where(x => x.Shipped == shipped)
                .OrderByDescending(x => x.OrderId)
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();

            _semaphore.Release();
            return orders;
        }

        public async Task<long> GetOrdersPaginatedAdminCount(bool shipped)
        {
            await _semaphore.WaitAsync();
            long totalOrders = await _context.Orders
                .AsNoTracking()
                .Where(x => x.Shipped == shipped)
                .CountAsync();

            _semaphore.Release();
            return totalOrders;
        }


        public async Task<List<Order>> GetOrdersByUser(long userId)
        {
            return await _context.Orders
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Article)
                .Where(x => x.UserProfileId == userId)
                .ToListAsync();
        }

        public async Task<Order> GetOrderById(long orderId)
        {
            return await _context.Orders
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Article)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.OrderId == orderId);
        }

        public Order Remove(long orderId)
        {
            Order o = _context.Orders.Include(x => x.Lines)
                .ThenInclude(x => x.Article)
                .FirstOrDefault(x => x.OrderId == orderId);

            if(o != null)
            {
                _context.Orders.Remove(o);
            }

            _context.SaveChanges();

            return o;
        }

        public Order SaveOrder(Order order)
        {

            if(order.OrderId == 0)
            {
                _context.Orders.Add(order);
            }

            _context.AddRange(order.Lines);

            _context.SaveChanges();

            return _context.Orders.Include(x => x.Lines)
                .ThenInclude(x => x.Article)
                .AsNoTracking()
                .FirstOrDefault(x => x.OrderId == order.OrderId);
        }

        public Order UpdateOrder(Order order)
        {
            var lines = order.Lines;

            var existingOrder = _context
                .Orders
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Article)
                .FirstOrDefault(x => x.OrderId == order.OrderId);

            if (existingOrder != null)
            {
                UpdateOrderFields(order, existingOrder);

                var existingLines = existingOrder.Lines;

                var linesToRemove = new List<OrderLine>();

                foreach (var existingLine in existingLines)
                {
                    var newLine = lines.FirstOrDefault(x => x.OrderLineId == existingLine.OrderLineId);
                    if (newLine != null)
                    {
                        if (newLine.Quantity > 0)
                        {
                            existingLine.Quantity = newLine.Quantity;
                        }
                        else
                        {
                            linesToRemove.Add(existingLine);
                        }
                    }
                    else
                    {
                        linesToRemove.Add(existingLine);
                    }
                }
                foreach (var lineToRemove in linesToRemove)
                {
                    existingOrder.Lines.Remove(lineToRemove);
                }

                _context.SaveChanges();
            }

            return existingOrder;
        }

        private static void UpdateOrderFields(Order order, Order existingOrder)
        {
            existingOrder.City = order.City;
            existingOrder.Zip = order.Zip;
            existingOrder.Address = order.Address;
            existingOrder.Country = order.Country;
            existingOrder.GiftWrap = order.GiftWrap;
            existingOrder.State = order.State;
            existingOrder.Note = order.Note;
            existingOrder.Status = order.Status;
            existingOrder.Shipped = order.Shipped;
        }
    }
}
