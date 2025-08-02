using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClothesShop.Models;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Repository
{
    public class ArticleGroupRepository : BaseRepository, IArticleGroupRepository
    {
        public ArticleGroupRepository(DatabaseContext context) : base(context) { }

        public async Task<ArticleGroup> GetArticleGroupById(long id)
        {
            return await _context.ArticleGroups
                .Include(x => x.ParentArticleGroup)
                .Include(x => x.ArticleGroups)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ArticleGroupId == id) ?? throw new System.Exception($"Article group with id {id} not found!");
        }

        public async Task<List<ArticleGroup>> GetArticleGroupsByParentId(long? parentId = null)
        {
            IQueryable<ArticleGroup> query = _context.ArticleGroups
                .Include(x => x.ParentArticleGroup)
                .Include(x => x.ArticleGroups)
                .Where(x => parentId == null || x.ParentArticleGroupId == parentId.Value);

            var articleGroups = await query
                .AsNoTracking()
                .ToListAsync();
            articleGroups = articleGroups.DistinctBy(x => x.ArticleGroupId)
                .ToList();

            return articleGroups;
        }

        public async Task<List<ArticleGroup>> GetParentArticleGroups()
        {
            IQueryable<ArticleGroup> query = _context.ArticleGroups
                .Include(x => x.ArticleGroups)
                .Where(x => x.ParentArticleGroupId == null);

            var articleGroups = await query
                .AsNoTracking()
                .ToListAsync();
            articleGroups = articleGroups.DistinctBy(x => x.ArticleGroupId)
                .ToList();

            return articleGroups;
        }

        public async Task<List<ArticleGroup>> GetArticleGroupsPaginatedAdmin(int currentPage, int itemsPerPage)
        {
            return await _context.ArticleGroups.Include(x => x.ParentArticleGroup)
                .AsNoTracking()
                .Skip((currentPage - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToListAsync();
        }

        public async Task<long> GetArticleGroupsPaginatedAdminCount()
        {
            return await _context.ArticleGroups
                .CountAsync();
        }

        public async Task<ArticleGroup> Create(ArticleGroup group)
        {
            group.ArticleGroupId = 0;
            var entry = await _context.ArticleGroups.AddAsync(group);
            await _context.SaveChangesAsync();

            return entry.Entity;
        }

        public async Task<bool> Delete(long id)
        {
            var groupById = await _context.ArticleGroups.Where(x => x.ArticleGroupId == id)
                .Include(x => x.ArticleGroups)
                .FirstOrDefaultAsync();

            if (groupById != null)
            {
                _context.ArticleGroups.Remove(groupById);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
