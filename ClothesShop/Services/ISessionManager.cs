namespace ClothesShop.Services
{
    public interface ISessionManager
    {
        public string GetByKey(string key);
        public void SetByKey(string key, string value);
        public void DeleteByKey(string key);
    }
}
