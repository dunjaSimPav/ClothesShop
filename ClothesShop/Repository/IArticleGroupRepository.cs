using System.Collections.Generic;
using System.Threading.Tasks;
using ClothesShop.Models;

namespace ClothesShop.Repository
{
    public interface IArticleGroupRepository
    {
        Task<ArticleGroup> Create(ArticleGroup group);
        Task<bool> Delete(long id);
        Task<ArticleGroup> GetArticleGroupById(long id);
        Task<List<ArticleGroup>> GetArticleGroupsByParentId(long? parentId = null);
        Task<List<ArticleGroup>> GetArticleGroupsPaginatedAdmin(int currentPage, int itemsPerPage);
        Task<long> GetArticleGroupsPaginatedAdminCount();
        Task<List<ArticleGroup>> GetParentArticleGroups();
    }
}
