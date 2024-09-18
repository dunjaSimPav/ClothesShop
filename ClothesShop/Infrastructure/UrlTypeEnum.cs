namespace ClothesShop.Infrastructure
{
    public enum UrlTypeEnum
    {
        [System.ComponentModel.Description("Create")]
        Create,
        [System.ComponentModel.Description("Edit")]
        Edit,
        [System.ComponentModel.Description("Details")]
        Details,
        [System.ComponentModel.Description("Delete")]
        Delete,
        [System.ComponentModel.Description("List")]
        List
    }
    
    public static class UrlHelper
    {
        public static string GetAdminUrl(this UrlTypeEnum urlType, string prefix, long? id = null)
        {
            string url = $"/admin/{prefix}";

            if (urlType != UrlTypeEnum.List)
                url += $"/{urlType}";

            if (id != null)
                url += $"/{id}";

            return url;
        }
    }
}
