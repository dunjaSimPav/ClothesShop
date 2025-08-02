using System.Collections.Generic;

namespace ClothesShop.Components
{
    public class CategoryFilterViewModel
    {
        public long? Id { get; init; }
        public string Name { get; init; }
        public bool IsParentCategory { get; init; } = true;

        public long ParentGroupId { get; init; }

        public List<CategoryFilterViewModel> SubCategories { get; set; } = new List<CategoryFilterViewModel>();

        public Dictionary<string, string> Params => new Dictionary<string, string>
        {
            { "page", "1" },
            { "pageSize", "12" },
            { "articleGroup", ParentGroupId.ToString() },
            { "articleSubGroup", Id?.ToString() }
        };
    }
}
