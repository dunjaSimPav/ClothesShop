using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using ClothesShop.Repository;
using System.Linq;

namespace ClothesShop.Components
{
    public class ArticlesFilterViewComponent : ViewComponent
    {
        private IStoreRepository storeRepository;

        public ArticlesFilterViewComponent(IStoreRepository repo) => storeRepository = repo;

        public ViewViewComponentResult Invoke()
        {
            string ArticleTypeKey = RouteData?.Values["ArticleType"]?.ToString();
            long.TryParse(ArticleTypeKey, out long ArticleType);

            if (ArticleType > 0)
                ViewBag.SelectedArticleType = ArticleType;
            else
                ViewBag.SelectedArticleType = null;


            return View(storeRepository.ArticleTypes.Distinct()
                .OrderBy(x => x.Name));
        }
    }
}
