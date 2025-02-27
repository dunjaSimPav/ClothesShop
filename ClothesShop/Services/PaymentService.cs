using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClothesShop.Services
{
    public class PaymentService : IPaymentService
    {
        public async Task<string> ProcessPayment(Order order, string successUrl, string cancelUrl)
        {
            var options = new SessionCreateOptions()
            {
                PaymentMethodTypes = new List<string>() { "card" },
                LineItems = MapItems(order.Lines),
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl
            };

            var service = new SessionService();
            var session = await service.CreateAsync(options);

            return session.Url;
        }

        private List<SessionLineItemOptions> MapItems(List<CartLine> lines)
        {
            return lines.Select(x => new SessionLineItemOptions()
            {
                PriceData = new SessionLineItemPriceDataOptions()
                {
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions()
                    {
                        Name = x.Article.Name,
                        Description = x.Article.Description,
                    },
                    UnitAmount = (long)x.Article.Price * 100
                },
                Quantity = x.Quantity
            }).ToList();
        }
    }
}
