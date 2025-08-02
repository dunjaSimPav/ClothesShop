using System.Linq;
using ClothesShop.Repository;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.AspNetCore.Mvc;
using ClothesShop.Models.ViewModels;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using ClothesShop.Models;
using System.Text.RegularExpressions;
using ClothesShop.Infrastructure;
using ClothesShop.Services;

namespace ClothesShop.Components
{
    public class NavigationMenuViewComponent : ViewComponent
    {
        private readonly List<NavigationEntryModel> _entries;

        private Cart _cart;
        private readonly Localizer _l;
        public NavigationMenuViewComponent(Cart cartService, Localizer localizer)
        {
            _cart = cartService;
            _l = localizer;
            _entries = new List<NavigationEntryModel>()
            {
                new NavigationEntryModel(1, "Home", "Index", _l["Home"], param: "header", internalId: "#header", isLocal: true),
                new NavigationEntryModel(2, "Home", "Index", _l["Portfolio"], param: "portfolio", internalId: "#portfolio", isLocal: true),
                new NavigationEntryModel(3, "Home", "Index", _l["ContactUs"], param: "contact", internalId: "#contact", isLocal: true),
                new NavigationEntryModel(4, "Cart", "", _l["Cart"]),
                //new NavigationEntryModel(5, "Account", "Login", "Get Started", internalId: "#", isButton: true, isLogin: true),
            };
        }

        public ViewViewComponentResult Invoke()
        {
            PathString url = HttpContext.Request.Path;

            List<PathString> paths = new List<PathString>()
            {
                "/",
                "/Home"
            };

            bool home = paths.Any(x => url.StartsWithSegments(x)) || HttpContext.Request.Path.Equals("/");
            string prevRequest = HttpContext.Request.Headers["Referer"]!.ToString();

            string requestWithoutHost = prevRequest.Replace(HttpContext.Request.Host.ToString(), "");
            requestWithoutHost = Regex.Replace(requestWithoutHost, "(http|https)://", "");

            bool prevPageMatched = url.StartsWithSegments(requestWithoutHost);

            _entries.ForEach(e =>
            {
                e.IsLocal = home && prevPageMatched;
                e.FullUrl = e.GetNavUrl();
                if (e.FullUrl.StartsWith(HttpContext.Request.Path.Value, System.StringComparison.OrdinalIgnoreCase))
                {
                    e.AddClass("active");
                }
            });

            var cart = _entries.First(x => x.Id == 4);
            if (_cart.Lines.Count > 0)
            {
                var total = _cart.ComputeTotalValue();
                cart.Title += " (" + total.ToString("#,###.00") + " RSD)";
            }
            
            var model = new NavigationPageModel()
            {
                NavigationEntries = [.. _entries]
            };
            return View(model);
        }
    }
}
