using System.Collections.Generic;

namespace ClothesShop.Models.ViewModels;

public class AdminArticleListViewModel
{
    public IEnumerable<Article> Articles { get; set; }

    public int PageSize { get; set; } = 10;
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int TotalCount { get; set; } = 0;
}
