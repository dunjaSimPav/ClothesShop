using ClothesShop.Models.ViewModels;

namespace ClothesShop.Infrastructure
{
    public static class RelativeUrlHelper
    {
        public static string GetNavUrl(this NavigationEntryModel model)
        {
            string controller = model.Controller;
            string action = model.Action;
            string urlPositionParam = !string.IsNullOrEmpty(model.InternalId)
                ? model.InternalId
                : string.Empty;

            string idParam = !string.IsNullOrEmpty(model.Param)
                ? $"/{model.Param}"
                : string.Empty;

            if (string.IsNullOrEmpty(controller)) return "#";

            //if (isLocal) return urlPositionParam;

            if (string.IsNullOrEmpty(action)) return $"/{controller}";

            return $"/{controller}/{action}{idParam}{urlPositionParam}";
        }
    }
}
