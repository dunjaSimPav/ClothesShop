using System.Collections.Generic;

namespace ClothesShop.Models.ViewModels
{
    public class NavigationPageModel
    {
        public int? ActiveId { get; set; }
        public List<NavigationEntryModel> NavigationEntries { get; set; }
    }

    public class NavigationEntryModel
    {
        public int Id { get; set; }
        public string Param { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string Title { get; set; }
        public bool IsLocal { get; set; }
        public string InternalId { get; set; }
        public bool IsButton { get; set; }

        public string FullUrl { get; set; }

        public string ClassList { get; set; }

        public bool IsLogin { get; set; }

        public NavigationEntryModel(int id, string controller, string action, string title, string param = "", bool isLocal = false, string internalId = null, bool isButton = false, string fullUrl = "", bool isLogin = false)
        {
            Id = id;
            Controller = controller;
            Action = action;
            Param = param;
            Title = title;
            IsLocal = isLocal;
            InternalId = internalId;
            IsButton = isButton;
            FullUrl = fullUrl;
            ClassList = IsButton ? "getstarted" : "nav-link";
            if (IsLocal) ClassList += "scrollto";

            IsLogin = isLogin;
        }

        public void AddClass(string cls)
        {
            ClassList += " " + cls;
        }
    }
}
