using ClothesShop.Models;
using System.Threading.Tasks;

namespace ClothesShop.Services
{
    public interface IPaymentService
    {
        Task<string> ProcessPayment(Order order, string successUrl, string cancelUrl);
    }
}
