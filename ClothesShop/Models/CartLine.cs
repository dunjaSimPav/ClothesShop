namespace ClothesShop.Models
{
    public class CartLine
    {
        public long ArticleId { get; set; }
        public string ArticleName { get; set; }
        public string ArticleGroupName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
