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
                .Where(x => x.ArticleId == Article.ArticleId)
                .FirstOrDefault();

            if (line == null)
            {
                Lines.Add(new CartLine
                {
                    ArticleId = Article.ArticleId,
                    ArticleName = Article.Name,
                    ArticleGroupName = Article.ArticleGroupItems?.FirstOrDefault()?.ArticleGroup?.ParentArticleGroup?.Name ?? "Groups not loaded...",
                    Quantity = quantity,
                    Price = Article.Price
                });
            }
            else
            {
                line.Quantity += quantity;
                line.Price = Article.Price;
            }
        }

        public virtual void RemoveLine(Article Article) =>
            Lines.RemoveAll(x => x.ArticleId == Article.ArticleId);

        public virtual decimal ComputeTotalValue() =>
            Lines.Sum(x => x.Price * x.Quantity);

        public virtual void Clear() => Lines.Clear();
    }
}
