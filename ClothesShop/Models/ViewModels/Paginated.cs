using System.Collections.Generic;

namespace ClothesShop.Models.ViewModels
{
    public class Paginated<T>
    {
        public int CurrentPage { get; set; }
        public int ItemsPerPage { get; set; }

        public int TotalItems { get; set; }
        public int TotalPages { get; set; }

        public List<T> Items { get; set; }
    }
}
