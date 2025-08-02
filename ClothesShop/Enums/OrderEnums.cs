using System.ComponentModel;

namespace ClothesShop.Enums
{
    public enum PaymentStatus
    {
        [Description("Neuspela naplata")]
        Failed = -2,
        [Description("Otkazano")]
        Cancelled = -1,
        [Description("Na čekanju")]
        Pending = 0,
        [Description("Plaćeno")]
        Paid = 1
    }
}
