using Microsoft.AspNetCore.Http;

namespace ClothesShop.Services
{
    public class SessionManager : ISessionManager
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SessionManager(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ISession GetSession() => _httpContextAccessor.HttpContext.Session;

        public void DeleteByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var session = GetSession();
            if (string.IsNullOrEmpty(session.GetString(key)))
                return;

            session.Remove(key);
        }

        public string GetByKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return string.Empty;

            var session = GetSession();
            return session.GetString(key);
        }

        public void SetByKey(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            var session = GetSession();
            if (session == null)
                return;

            session.SetString(key, value);
        }
    }
}
