using System;
using System.Text.Json.Serialization;
using ClothesShop.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace ClothesShop.Models
{
    public class SessionCart : Cart
    {
        public static Cart GetCart(IServiceProvider provider)
        {
            ISession session = provider.GetRequiredService<IHttpContextAccessor>()?.HttpContext
                .Session;

            SessionCart cart = session?.GetJson<SessionCart>("cart")
                ?? new SessionCart();

            cart.Session = session;
            return cart;
        }

        [JsonIgnore]
        public ISession Session { get; set; }

        public override void AddItem(Article Article, int quantity)
        {
            base.AddItem(Article, quantity);
            Session.SetJson("cart", this);
        }

        public override void RemoveLine(Article Article)
        {
            base.RemoveLine(Article);
            Session.SetJson("cart", this);
        }

        public override void Clear()
        {
            base.Clear();
            Session.Remove("cart");
        }
    }
}
