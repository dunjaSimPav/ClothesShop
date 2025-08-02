using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using ClothesShop.Repository;
using System.Linq;
using System;

namespace ClothesShop.Components
{
    public class ArticlesFilterViewComponent : ViewComponent
    {
        private IStoreRepository storeRepository;

        public ArticlesFilterViewComponent(IStoreRepository repo) => storeRepository = repo;

        public ViewViewComponentResult Invoke()
        {
            throw new NotImplementedException();
        }
    }
}
