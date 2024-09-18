using System.Linq;
using ClothesShop.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace ClothesShop.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private DatabaseContext _context;

        public OrderRepository(DatabaseContext ctx) => _context = ctx;

        public IQueryable<Order> Orders => _context.Orders
            .Include(x => x.Lines)
                .ThenInclude(x => x.Article);

        public Order Remove(int orderId)
        {
            Order o = _context.Orders.Include(x => x.Lines)
                .ThenInclude(x => x.Article)
                .ThenInclude(x => x.ArticleType)
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
            _context.AttachRange(order.Lines.Select(x => x.Article));

            if(order.OrderId == 0)
            {
                _context.Orders.Add(order);
            }

            _context.SaveChanges();

            return _context.Orders.Include(x => x.Lines)
                .ThenInclude(x => x.Article)
                .ThenInclude(x => x.ArticleType)
                .FirstOrDefault(x => x.OrderId == order.OrderId);
        }

        public Order UpdateOrder(Order order)
        {
            var lines = order.Lines;

            var existingOrder = _context
                .Orders
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Article)
                    .ThenInclude(x => x.ArticleType)
                .Where(x => x.OrderId == order.OrderId).FirstOrDefault();

            if (existingOrder != null)
            {
                UpdateOrderFields(order, existingOrder);

                var existingLines = existingOrder.Lines;

                var linesToRemove = new List<CartLine>();

                foreach (var existingLine in existingLines)
                {
                    var newLine = lines.FirstOrDefault(x => x.CartLineId == existingLine.CartLineId);
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
            existingOrder.Line1 = order.Line1;
            existingOrder.Line2 = order.Line2;
            existingOrder.Line3 = order.Line3;
            existingOrder.Country = order.Country;
            existingOrder.GiftWrap = order.GiftWrap;
            existingOrder.State = order.State;
            existingOrder.Note = order.Note;
        }
    }
}
