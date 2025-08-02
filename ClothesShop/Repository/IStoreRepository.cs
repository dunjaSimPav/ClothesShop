using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClothesShop.Models;

namespace ClothesShop.Repository
{
    public interface IStoreRepository
    {
        Task<Article> GetArticleById(long id);
        Task SaveArticle(Article entity);
        Task CreateArticle(Article entity);
        Task DeleteArticle(Article entity);
        Task<List<Article>> GetArticlesPaginated(int pageNumber, int pageSize, long? articleGroupId = null, long? articleSubGroupId = null, int sortOrder = 0, 
            decimal? minPrice = null, decimal? maxPrice = null);
        Task<int> GetArticlesPaginatedCount(long? articleGroupId = null, long? articleSubGroupId = null, decimal? minPrice = null, decimal? maxPrice = null);
        Task<Article> GetArticleWithDetails(long id);
        Task<int> GetMinPriceFiltered(long? articleGroupId = null, long? articleSubGroupId = null);
        Task<int> GetMaxPriceFiltered(long? articleGroupId = null, long? articleSubGroupId = null);
        Task<List<Article>> GetArticlesPaginatedAdmin(int pageNumber, int pageSize);
        Task<long> GetArticlesPaginatedAdminCount();
    }
}
