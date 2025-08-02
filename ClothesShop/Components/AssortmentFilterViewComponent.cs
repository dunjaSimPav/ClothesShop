using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClothesShop.Models.ViewModels;
using ClothesShop.Repository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace ClothesShop.Components
{
    public class AssortmentFilterViewComponent : ViewComponent
    {
        private readonly IArticleGroupRepository articleGroupRepository;

        public AssortmentFilterViewComponent(IArticleGroupRepository repo) => articleGroupRepository = repo;

        [Parameter]
        public ArticleListViewModel ArticlesFilter { get; set; } = new();

        public async Task<ViewViewComponentResult> InvokeAsync(ArticleListViewModel articlesFilter)
        {
            ArticlesFilter = articlesFilter;
            if (ArticlesFilter == null)
            {
                ArticlesFilter = new ArticleListViewModel();
            }

            var parentArticleGroup = await articleGroupRepository.GetArticleGroupById(articlesFilter.CurrentArticleGroupId.Value);
            var articleGroups = await articleGroupRepository.GetArticleGroupsByParentId(articlesFilter.CurrentArticleGroupId);
            var articleGroupFilters = articleGroups.Select(x => new CategoryFilterViewModel()
            {
                Id = x.ArticleGroupId,
                ParentGroupId = x.ParentArticleGroupId ?? 0,
                Name = x.Name,
                SubCategories = x.ArticleGroups.Select(y => new CategoryFilterViewModel()
                {
                    Id = y.ArticleGroupId,
                    Name = y.Name,
                    IsParentCategory = true
                }).ToList()
            });

            var finalArticleGroups = new List<CategoryFilterViewModel>()
            {
                new CategoryFilterViewModel()
                {
                    Id = null,
                    ParentGroupId = articlesFilter.CurrentArticleGroupId.Value,
                    Name = $"Svi artikli - {parentArticleGroup.Name}",
                    IsParentCategory = true,
                    SubCategories = articleGroupFilters.ToList()
                }
            };
            finalArticleGroups.AddRange(articleGroupFilters);

            var viewResult = new AssortmentFilterViewModel()
            {
                CurrentGroupId = articlesFilter.CurrentArticleGroupId,
                CurrentSubGroupId = articlesFilter.CurrentArticleSubGroupId,
                Categories = [.. finalArticleGroups],
                MinPrice = articlesFilter.MinPrice,
                MaxPrice = articlesFilter.MaxPrice,
                SortOrder = articlesFilter.SortOrder,
                MinAllowedPrice = articlesFilter.MinAllowedPrice,
                MaxAllowedPrice = articlesFilter.MaxAllowedPrice
            };
            return View(viewResult);
        }
    }
}