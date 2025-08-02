using System.Threading.Tasks;
using ClothesShop.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewComponents;

namespace ClothesShop.Components
{
    public class RangeSliderViewComponent : ViewComponent
    {
        public async Task<ViewViewComponentResult> InvokeAsync(AssortmentFilterViewModel articlesFilter)
        {
            return View(articlesFilter);
        }
    }
}