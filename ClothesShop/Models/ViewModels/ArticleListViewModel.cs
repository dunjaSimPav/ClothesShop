using System.Collections.Generic;

namespace ClothesShop.Models.ViewModels
{
    public class ArticleListViewModel
    {
        public IEnumerable<Article> Articles { get; set; }
        public PagingInfo PagingInfo { get; set; }
        public int? CurrentArticleType { get; set; }
    }
}
