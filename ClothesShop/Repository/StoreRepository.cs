using System.Linq;
using ClothesShop.Models;

namespace ClothesShop.Repository
{
    public class StoreRepository : IStoreRepository
    {
        private DatabaseContext _context;

        public StoreRepository(DatabaseContext ctx) => _context = ctx;

        public IQueryable<Article> Articles => _context.Articles;

        public IQueryable<ArticleType> ArticleTypes => _context.ArticleTypes;

        public void CreateArticle(Article entity)
        {
            _context.Add(entity);
            _context.SaveChanges();
        }

        public void SaveArticle(Article entity)
        {
            _context.SaveChanges();
        }

        public void DeleteArticle(Article entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
        }

        public void CreateArticleType(ArticleType entity)
        {
            _context.Add(entity);
            _context.SaveChanges();
        }

        public void SaveArticleType(ArticleType entity)
        {
            _context.SaveChanges();
        }

        public void DeleteArticleType(ArticleType entity)
        {
            _context.Remove(entity);
            _context.SaveChanges();
        }
    }
}
