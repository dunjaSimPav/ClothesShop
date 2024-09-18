using System.Collections.Generic;
using System.Linq;

namespace ClothesShop.Models
{
    public class Cart
    {
        public List<CartLine> Lines { get; set; } = new List<CartLine>();

        public virtual void AddItem(Article Article, int quantity)
        {
            CartLine line = Lines
                .Where(x => x.Article.ArticleId == Article.ArticleId)
                .FirstOrDefault();

            if (line == null)
                Lines.Add(new CartLine
                {
                    Article = Article,
                    Quantity = quantity
                });
            else
                line.Quantity += quantity;
        }

        public virtual void RemoveLine(Article Article) =>
            Lines.RemoveAll(x => x.Article.ArticleId == Article.ArticleId);

        public virtual decimal ComputeTotalValue() =>
            Lines.Sum(x => x.Article.Price * x.Quantity);

        public virtual void Clear() => Lines.Clear();
    }

    public class CartLine
    {
        public int CartLineId { get; set; }
        public Article Article { get; set; }
        public int Quantity { get; set; }
    }
}
