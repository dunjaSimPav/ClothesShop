using ClothesShop.Enums;
using ClothesShop.Models;
using Stripe;
using System.Threading.Tasks;

namespace ClothesShop.Services
{
    public interface IPaymentService
    {
        Task<PaymentIntent> GetPaymentIntent(string sessionId);
        Task<(PaymentStatus status, string redirectUrl)> ProcessPayment(Order order, string successUrl, string cancelUrl, string failureUrl);
    }
}
