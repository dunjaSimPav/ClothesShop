using ClothesShop.Enums;

namespace ClothesShop.Infrastructure
{
    public static class EnumHelpers
    {
        public static string Translate(this PaymentStatus status)
        {
            return status switch
            {
                PaymentStatus.Cancelled => "Otkazano",
                PaymentStatus.Paid => "Plaćeno",
                PaymentStatus.Pending => "Na čekanju",
                PaymentStatus.Failed => "Neuspešna naplata"
            };
        }
    }
}
