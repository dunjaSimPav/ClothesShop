using System.Threading.Tasks;
using ClothesShop.Models;
using ClothesShop.Models.ViewModels;
using ClothesShop.Models.ViewModels.ArticleGroupItems;

namespace ClothesShop.Repository
{
    public interface IArticleGroupItemsRepository
    {
        Task<bool> DeleteByGroupId(int id);
        Task<Paginated<ArticleGroupItem>> GetArticleGroupItemsByArticleId(long articleId);
        Task<Paginated<ArticleGroupItem>> GetArticleGroupItemsByGroupId(int? groupId = null, int currentPage = 1, int itemsPerPage = 6);
        Task Reassign(ReassignArticleGroupsToItem model);
    }
}
