using Microsoft.AspNetCore.Mvc;
using ClothesShop.Models;
using ClothesShop.Repository;
using System.Threading.Tasks;
using ClothesShop.Services;

namespace ClothesShop.Controllers
{
    public class CartController : Controller
    {
        private readonly IStoreRepository repository;
        private readonly Cart cartService;


        public CartController(IStoreRepository repository, Cart cartService)
        {
            this.repository = repository;
            this.cartService = cartService;
        }

        [HttpGet]
        [Route("Cart")]
        public IActionResult Index()
        {
            return View(cartService);
        }


        [HttpPost]
        [Route("Cart/Post")]
        public async Task<IActionResult> Post([FromForm] Article article)
        {
            var articleFromDb = await repository.GetArticleById(article.ArticleId);
            if (articleFromDb != null)
            {
                cartService.AddItem(articleFromDb, 1);
            }
            return RedirectToAction("Index", "Home", "portfolio#portfolio");
        }

        [HttpPost]
        [Route("Cart/Delete")]
        public async Task<IActionResult> Delete([FromForm] Article article)
        {
            var articleFromDb = await repository.GetArticleById(article.ArticleId);
            if (articleFromDb != null)
            {
                cartService.RemoveLine(articleFromDb);
            }
            return RedirectToAction("", "Cart");
        }
    }
}
