using System.Collections.Generic;
using ClothesShop.Components;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ClothesShop.Models.ViewModels
{
    public class AssortmentFilterViewModel
    {
        public long? CurrentGroupId { get; init; }
        public long? CurrentSubGroupId { get; init; }

        public List<CategoryFilterViewModel> Categories { get; init; }

        public int ItemsPerPage { get; set; } = 6;
        public int SortOrder { get; set; } = 0;
        public decimal? MinPrice { get; set; } = null;
        public decimal? MaxPrice { get; set; } = null;

        public int MinAllowedPrice { get; set; } = 0;
        public int MaxAllowedPrice { get; set; } = 10000;

        public List<SelectListItem> ItemsPerPageOptions {
            get
            {

                return [
                    new SelectListItem("3", 3.ToString(), ItemsPerPage == 3),
                    new SelectListItem("6", 6.ToString(), ItemsPerPage == 6),
                    new SelectListItem("12", 12.ToString(), ItemsPerPage == 12),
                ];
            }
        }
    }
}
