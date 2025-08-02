using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClothesShop.Models;
using ClothesShop.Models.ViewModels;
using ClothesShop.Models.ViewModels.ArticleGroupItems;
using Microsoft.EntityFrameworkCore;

namespace ClothesShop.Repository
{
    public class ArticleGroupItemsRepository : BaseRepository, IArticleGroupItemsRepository
    {
        public ArticleGroupItemsRepository(DatabaseContext context) : base(context)
        {
        }

        public async Task<Paginated<ArticleGroupItem>> GetArticleGroupItemsByGroupId(int? groupId = null, int currentPage = 1, int itemsPerPage = 6)
        {
            if (itemsPerPage < 6)
                itemsPerPage = 6;

            if (currentPage < 1)
                currentPage = 1;

            var totalCount = await GetArticleGroupItemsByGroupIdTotalCount(groupId);

            var offset = CalculateItemsToSkip(currentPage, itemsPerPage);
            if (totalCount <= offset)
            {
                currentPage = 1;
                offset = CalculateItemsToSkip(currentPage, itemsPerPage);
            }

            var items = await _context.ArticleGroupItems
                .AsNoTracking()
                .Where(x => groupId == null || x.ArticleGroupId == groupId)
                .Include(x => x.Article)
                .OrderBy(x => x.Id)
                .Skip(offset)
                .Take(itemsPerPage)
                .ToListAsync();

            return new Paginated<ArticleGroupItem>()
            {
                Items = items,
                CurrentPage = currentPage,
                ItemsPerPage = itemsPerPage,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((decimal)(totalCount / itemsPerPage))
            };
        }

        public async Task<Paginated<ArticleGroupItem>> GetArticleGroupItemsByArticleId(long articleId)
        {
            var items = await _context.ArticleGroupItems
                .Include(x => x.ArticleGroup)
                    .ThenInclude(x => x.ParentArticleGroup)
                .AsNoTracking()
                .Where(x => x.ArticleId == articleId)
                .ToListAsync();

            return new Paginated<ArticleGroupItem>()
            {
                Items = items,
            };
        }

        private int CalculateItemsToSkip(int currentPage, int itemsPerPage) 
            => (currentPage - 1) * itemsPerPage;

        private async Task<int> GetArticleGroupItemsByGroupIdTotalCount(int? groupId = null)
        {
            return await _context.ArticleGroupItems
                .AsNoTracking()
                .Where(x => groupId == null || x.ArticleGroupId == groupId)
                .CountAsync();
        }

        public async Task Reassign(ReassignArticleGroupsToItem model)
        {
            var article = await _context.Articles.AsNoTracking().FirstOrDefaultAsync(x => x.ArticleId == model.ArticleId);
            if (article == null) throw new Exception($"Article with id {model.ArticleId} was not found!");

            var articleGroupIds = await _context.ArticleGroups.AsNoTracking()
                .Where(x => model.ArticleGroups.Contains(x.ArticleGroupId))
                .Select(x => x.ArticleGroupId).Distinct().ToListAsync();

            var existingArticleGroupAssignments = await _context.ArticleGroupItems
                .AsNoTracking()
                .Where(x => x.ArticleId == model.ArticleId)
                .ToListAsync();

            var allExistingGroupIds = existingArticleGroupAssignments.Select(x => x.ArticleGroupId)
                .Distinct().ToList();

            var overlappingGroupIds = allExistingGroupIds.Intersect(articleGroupIds).ToList();
            var newGroupIds = articleGroupIds.Except(overlappingGroupIds).ToList();
            var removedGroupIds = allExistingGroupIds.Except(overlappingGroupIds).ToList();

            var itemsToDelete = await _context.ArticleGroupItems.Where(x => x.ArticleId == model.ArticleId && removedGroupIds.Contains(x.ArticleGroupId))
                .ToListAsync();

            var itemsToAdd = newGroupIds.Select(x => new ArticleGroupItem()
            {
                ArticleId = model.ArticleId,
                ArticleGroupId = x
            }).ToList();


            _context.ArticleGroupItems.RemoveRange(itemsToDelete);
            await _context.ArticleGroupItems.AddRangeAsync(itemsToAdd);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteByGroupId(int id)
        {
            var groupById = await _context.ArticleGroups.Where(x => x.ArticleGroupId == id)
                .Include(x => x.ArticleGroupItems)
                .Include(x => x.ArticleGroups)
                    .ThenInclude(x => x.ArticleGroupItems)
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
