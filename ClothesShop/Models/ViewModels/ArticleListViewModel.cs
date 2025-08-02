using System.Collections.Generic;

namespace ClothesShop.Models.ViewModels
{
    public class ArticleListViewModel
    {
        public IEnumerable<Article> Articles { get; set; }
        public IEnumerable<ArticleGroup> ArticleGroups { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public long? CurrentArticleGroupId { get; set; }
        public long? CurrentArticleSubGroupId { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }

        public int MinAllowedPrice { get; set; } = 0;
        public int MaxAllowedPrice { get; set; } = 10000;
        public int SortOrder { get; set; }
    }
}
