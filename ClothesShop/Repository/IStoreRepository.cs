using System.Linq;
using ClothesShop.Models;

namespace ClothesShop.Repository
{
    public interface IStoreRepository
    {
        IQueryable<Article> Articles { get; }
        IQueryable<ArticleType> ArticleTypes { get; }

        void SaveArticle(Article entity);
        void CreateArticle(Article entity);
        void DeleteArticle(Article entity);

        void SaveArticleType(ArticleType entity);
        void CreateArticleType(ArticleType entity);
        void DeleteArticleType(ArticleType gentity);
    }
}
