using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ClothesShop.Models;
using ClothesShop.Repository;
using System.Linq;

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
        public IActionResult Post([FromForm] Article article)
        {
            var articleFromDb = repository.Articles.Include(x => x.ArticleType).FirstOrDefault(x => x.ArticleId == article.ArticleId);
            if (articleFromDb != null)
            {
                cartService.AddItem(articleFromDb, 1);
            }
            return RedirectToAction("Index", "Home", "portfolio#portfolio");
        }

        [HttpPost]
        [Route("Cart/Delete")]
        public IActionResult Delete([FromForm] Article article)
        {
            var articleFromDb = repository.Articles.Include(x => x.ArticleType).FirstOrDefault(x => x.ArticleId == article.ArticleId);
            if (articleFromDb != null)
            {
                cartService.RemoveLine(articleFromDb);
            }
            return RedirectToAction("", "Cart");
        }
    }
}
