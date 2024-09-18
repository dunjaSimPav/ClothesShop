using System.Linq;
using ClothesShop.Models.ViewModels;
using ClothesShop.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;

namespace ClothesShop.Controllers
{
    public class HomeController : Controller
    {
        private IStoreRepository _storeRepository;

        public int PageSize = Int32.MaxValue;

        public HomeController(IStoreRepository repo) => _storeRepository = repo;

        public ViewResult Index(int ArticlePage = 1, int? ArticleType = null)
        {
            if (ArticlePage > 1) PageSize = 6;

            ArticleListViewModel ArticleListViewModel = new ArticleListViewModel();

            var Articles = _storeRepository.Articles
                    .Include(x => x.ArticleType)
                    .Where(x => ArticleType == null || x.ArticleTypeId == ArticleType)
                    .OrderBy(x => x.ArticleId)
                    .Skip((ArticlePage - 1) * PageSize)
                    .Take(PageSize);

            ArticleListViewModel.Articles = Articles;
            ArticleListViewModel.CurrentArticleType = ArticleType;

            ArticleListViewModel.PagingInfo = new PagingInfo
            {
                CurrentPage = ArticlePage,
                ItemsPerPage = PageSize,
                TotalItems = _storeRepository.Articles.Where(x => ArticleType == null || x.ArticleTypeId == ArticleType).Count()
            };
            return View(ArticleListViewModel);
        }
    }
}
