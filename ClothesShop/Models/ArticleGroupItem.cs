using System.Collections.Generic;

namespace ClothesShop.Models
{
    public class ArticleGroupItem
    {
        public int Id { get; set; }

        public long ArticleGroupId { get; set; }
        public ArticleGroup ArticleGroup { get; set; }

        public long ArticleId { get; set; }
        public Article Article { get; set; }
    }
}
