using ClothesShop.Models.ViewModels;
using ClothesShop.Repository;
using ClothesShop.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ClothesShop.Controllers
{
    public class HomeController : Controller
    {
        private IStoreRepository _storeRepository;
        private IArticleGroupRepository _articleGroupRepository;

        private readonly Localizer L;

        public HomeController(IStoreRepository repo, IArticleGroupRepository articleGroupRepository,
            Localizer l)
        {
            _storeRepository = repo;
            _articleGroupRepository = articleGroupRepository;
            L = l;
        }

        public async Task<ViewResult> Index(int page = 1, int? itemsPerPage = 6, int? articleGroup = null, int? articleSubGroup = null, int sortOrder = 0,
            decimal? minPrice = null, decimal? maxPrice = null)
        {
            (page, itemsPerPage, ArticleListViewModel ArticleListViewModel) = await GetArticlesByFilter(page, itemsPerPage, articleGroup, articleSubGroup, sortOrder,
                minPrice, maxPrice);
            return View(ArticleListViewModel);
        }

        private async Task<(int page, int? pageSize, ArticleListViewModel ArticleListViewModel)> GetArticlesByFilter(int page, int? pageSize, int? articleGroup, int? articleSubGroup, int sortOrder, decimal? minPrice, decimal? maxPrice)
        {
            if (page < 1 || page >= Int32.MaxValue)
                page = 1;

            if (pageSize == null || pageSize < 1 || pageSize > 6)
            {
                pageSize = 6;
            }

            ArticleListViewModel ArticleListViewModel = new ArticleListViewModel();

            ArticleListViewModel.Articles = await _storeRepository.GetArticlesPaginated(page, pageSize.Value, articleGroup, articleSubGroup, sortOrder, minPrice, maxPrice);
            ArticleListViewModel.ArticleGroups = await _articleGroupRepository.GetParentArticleGroups();
            ArticleListViewModel.CurrentArticleGroupId = articleGroup;
            ArticleListViewModel.CurrentArticleSubGroupId = articleSubGroup;
            ArticleListViewModel.SortOrder = sortOrder;
            ArticleListViewModel.MinPrice = minPrice;
            ArticleListViewModel.MaxPrice = maxPrice;
            ArticleListViewModel.MinAllowedPrice = await _storeRepository.GetMinPriceFiltered(articleGroup, articleSubGroup);
            ArticleListViewModel.MaxAllowedPrice = await _storeRepository.GetMaxPriceFiltered(articleGroup, articleSubGroup);

            ArticleListViewModel.PagingInfo = new PagingInfo
            {
                CurrentPage = page,
                ItemsPerPage = pageSize.Value,
                TotalItems = await _storeRepository.GetArticlesPaginatedCount(articleGroup, articleSubGroup, minPrice, maxPrice),
                CurrentGroup = articleGroup,
                CurrentSubGroup = articleSubGroup
            };
            return (page, pageSize, ArticleListViewModel);
        }
    }
}
