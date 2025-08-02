using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClothesShop.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ClothesShop.Repository
{
    public class StoreRepository : IStoreRepository
    {
        private readonly Func<DatabaseContext, long?, IQueryable<Article>> _articlesQuery = (context, articleSubGroupId)
            => context.ArticleGroupItems.Where(x => x.Article != null && (articleSubGroupId == null || x.ArticleGroupId == articleSubGroupId))
                .Include(x => x.Article)
                    .ThenInclude(x => x.ArticleGroupItems)
                        .ThenInclude(x => x.ArticleGroup)
                            .ThenInclude(x => x.ParentArticleGroup)
                .AsNoTracking()
                .AsQueryable()
                .Select(x => x.Article)
                .Distinct();
            

        private readonly DatabaseContext _context;

        public StoreRepository(DatabaseContext ctx) => _context = ctx;

        public async Task CreateArticle(Article entity)
        {
            await _context.Articles.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task SaveArticle(Article entity)
        {
            var existingArticle = await _context.Articles.FirstOrDefaultAsync(x => x.ArticleId == entity.ArticleId);
            if (existingArticle == null)
            {
                await CreateArticle(entity);
                return;
            }

            existingArticle.Price = entity.Price;
            existingArticle.Tags = entity.Tags;
            existingArticle.Name = entity.Name;
            existingArticle.Description = entity.Description;
            existingArticle.Image = entity.Image;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteArticle(Article entity)
        {
            _context.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Article> GetArticleById(long id)
        {
            return await _context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.ArticleId == id);
        }

        private IQueryable<Article> GetArticlesByParentGroupId(long parentGroupId)
        {
            return _context.ArticleGroupItems.Where(x => x.Article != null && x.ArticleGroup.ParentArticleGroupId == parentGroupId)
                .Include(x => x.Article)
                .AsNoTracking()
                .AsQueryable()
                .Select(x => x.Article)
                .Distinct();
        }

        public async Task<int> GetMinPriceFiltered(long? articleGroupId = null, long? articleSubGroupId = null)
        {
            IQueryable<Article> articlesQuery;

            if (articleGroupId is not null && articleSubGroupId is null)
            {
                articlesQuery = GetArticlesByParentGroupId(articleGroupId.Value);
            }
            else
            {
                articlesQuery = _articlesQuery(_context, articleSubGroupId);
            }

            var minPrice = await articlesQuery.OrderBy(x => x.Price).Select(x => x.Price).FirstOrDefaultAsync();

            if (minPrice == 0)
                minPrice = 0;

            return (int)Math.Floor(Math.Round(minPrice, 0));
        }

        public async Task<int> GetMaxPriceFiltered(long? articleGroupId = null, long? articleSubGroupId = null)
        {
            IQueryable<Article> articlesQuery;

            if (articleGroupId is not null && articleSubGroupId is null)
            {
                articlesQuery = GetArticlesByParentGroupId(articleGroupId.Value);
            }
            else
            {
                articlesQuery = _articlesQuery(_context, articleSubGroupId);
            }

            var maxPrice = await articlesQuery.OrderByDescending(x => x.Price).Select(x => x.Price).FirstOrDefaultAsync();

            if (maxPrice == 0)
                maxPrice = 100000;

            return (int)Math.Ceiling(Math.Round(maxPrice, 2));
        }


        public async Task<List<Article>> GetArticlesPaginatedAdmin(int pageNumber, int pageSize)
        {
            IQueryable<Article> articlesQuery = _context.Articles;

            return await articlesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<long> GetArticlesPaginatedAdminCount()
        {
            IQueryable<Article> articlesQuery = _context.Articles;

            return await articlesQuery.CountAsync();
        }

        public async Task<List<Article>> GetArticlesPaginated(int pageNumber, int pageSize, long? articleGroupId = null, long? articleSubGroupId = null,
            int sortOrder = 0, decimal? minPrice = null, decimal? maxPrice = null)
        {
            IQueryable<Article> articlesQuery;

            if (articleGroupId is not null && articleSubGroupId is null)
            {
                articlesQuery = GetArticlesByParentGroupId(articleGroupId.Value);
            }
            else
            {
                articlesQuery = _articlesQuery(_context, articleSubGroupId);
            }

            articlesQuery = sortOrder switch
            {
                0 => articlesQuery.OrderBy(x => x.Name),
                1 => articlesQuery.OrderBy(x => x.Price),
                -1 => articlesQuery.OrderByDescending(x => x.Price),
                _ => articlesQuery.OrderBy(x => x.Name),
            };

            if (minPrice != null)
            {
                articlesQuery = articlesQuery.Where(x => x.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                articlesQuery = articlesQuery.Where(x => x.Price <= maxPrice);
            }

            return await articlesQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetArticlesPaginatedCount(long? articleGroupId = null, long? articleSubGroupId = null, 
            decimal? minPrice = null, decimal? maxPrice = null)
        {
            IQueryable<Article> articlesQuery;


            if (articleGroupId is not null && articleSubGroupId is null)
            {
                articlesQuery = GetArticlesByParentGroupId(articleGroupId.Value);
            }
            else
            {
                articlesQuery = _articlesQuery(_context, articleSubGroupId);
            }

            if (minPrice != null)
            {
                articlesQuery = articlesQuery.Where(x => x.Price >= minPrice);
            }

            if (maxPrice != null)
            {
                articlesQuery = articlesQuery.Where(x => x.Price <= maxPrice);
            }

            return await articlesQuery.CountAsync();
        }

        public async Task<Article> GetArticleWithDetails(long id)
        {
            return await _context.Articles
                .Include(x => x.ArticleGroupItems)
                    .ThenInclude(x => x.ArticleGroup)
                        .ThenInclude(x => x.ParentArticleGroup)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ArticleId == id);
        }
    }
}
