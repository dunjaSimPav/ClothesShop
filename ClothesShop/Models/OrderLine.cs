namespace ClothesShop.Models
{
    public class OrderLine
    {
        public long OrderLineId { get; set; }
        
        public long OrderId { get; set; }
        public Order Order { get; set; }

        public long ArticleId { get; set; }
        public Article Article { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
