using ClothesShop.Enums;
using ClothesShop.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClothesShop.Services
{
    public class PaymentService : IPaymentService
    {
        public async Task<(PaymentStatus status, string redirectUrl)> ProcessPayment(Order order, string successUrl, string cancelUrl, string failureUrl)
        {
            try
            {
                Guid checkoutId = Guid.NewGuid();
                var options = new SessionCreateOptions()
                {
                    PaymentMethodTypes = new List<string>() { "card" },
                    LineItems = MapItems(order.Lines),
                    Mode = "payment",
                    ClientReferenceId = checkoutId.ToString(),
                    PaymentIntentData = new SessionPaymentIntentDataOptions()
                    {
                        Metadata = new Dictionary<string, string>()
                        {
                            ["orderId"] = order.OrderId.ToString()
                        }
                    },
                    SuccessUrl = successUrl,
                    CancelUrl = cancelUrl
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);
                string sessionUrl = session.Url;

                PaymentStatus status = session.PaymentStatus.Equals("paid", StringComparison.OrdinalIgnoreCase) 
                    ? PaymentStatus.Paid 
                    : sessionUrl.Equals(cancelUrl, StringComparison.OrdinalIgnoreCase)
                        ? PaymentStatus.Cancelled
                        : PaymentStatus.Failed;


                return (status, sessionUrl);
            }
            catch (Exception ex)
            {
                return (PaymentStatus.Failed, failureUrl);
            }
        }

        public async Task<PaymentIntent?> GetPaymentIntent(string sessionId)
        {
            try
            {
                var service = new PaymentIntentService();
                var intent = await Task.Run(() => service.Get(sessionId));
                return intent;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private List<SessionLineItemOptions> MapItems(List<OrderLine> lines)
        {
            return lines.Select(x => new SessionLineItemOptions()
            {
                PriceData = new SessionLineItemPriceDataOptions()
                {
                    Currency = "rsd",
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
