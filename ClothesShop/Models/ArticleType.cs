using System.ComponentModel.DataAnnotations;

namespace ClothesShop.Models
{
    public class ArticleType
    {
        public long ArticleTypeId { get; set; }
        [Required(ErrorMessage = "Please enter a valid name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter a valid description")]
        public string Description { get; set; }
    }
}
