using System.Collections.Generic;

namespace ClothesShop.Models.ViewModels.ArticleGroupItems
{
    public class ReassignArticleGroupsToItem
    {
        public long ArticleId { get; set; }

        public List<long> ArticleGroups { get; set; }
    }
}
