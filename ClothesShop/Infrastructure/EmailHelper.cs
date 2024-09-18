using ClothesShop.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;

namespace ClothesShop.Infrastructure
{
    public static class EmailHelper
    {
        public static string PrepareOrderEmail(Order order, bool wasUpdated = false, bool? wasShipped = null, bool? wasCancelled = null)
        {
            string OrderTemplate = PrepareOrderEmailContent(order, wasUpdated, wasShipped, wasCancelled);

            foreach(var line in order.Lines)
            {
                OrderTemplate += string.Format("{0} - {1} - {2} - {3:C} <br>",
                    line.Article.Name, line.Article.ArticleType.Name, line.Quantity, line.Quantity * line.Article.Price);
            }

            var total = order.Lines.Select(x => x.Quantity * x.Article.Price).Sum();

            OrderTemplate += string.Format("Total: {0:C}", total);

            return OrderTemplate;
        }

        private static string PrepareOrderEmailContent(Order order, bool wasUpdated = false, bool? wasShipped = null, bool? wasCancelled = null)
        {
            if (wasUpdated)
            {
                string updateDetail = "updated";
                if (wasCancelled == true)
                {
                    updateDetail = "cancelled by an administrator";
                }
                else if (wasShipped == true)
                {
                    updateDetail = "shipped";
                }
                else if (wasShipped == false)
                {
                    updateDetail = "returned to queue";
                }
                return $"Hello! Order has been {updateDetail}! Order was made by {order.Email}!<br>" +
                    $"Name: {order.Name}<br>" +
                    $"Delivery address: {order.Line1}<br>" +
                    $"Note: {order.Note}<br>" +
                    (wasShipped == true ? "Order Status: Shipped<br>" : "") +
                    (wasShipped == false && wasUpdated ? "Order Status: Returned to queue<br>" : "") +
                    (wasCancelled == true ? "Order Status: Cancelled<br>" : "") +
                    $"Products: ";
            }
            else
            {
                return $"Hello! A new order was recieved from {order.Email}!<br>" +
                    $"Name: {order.Name}<br>" +
                    $"Delivery address: {order.Line1}<br>" +
                    $"Products: ";
            }
        }
    }
}
