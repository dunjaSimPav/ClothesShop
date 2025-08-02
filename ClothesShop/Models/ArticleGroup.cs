using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components.Forms;
using System.ComponentModel.DataAnnotations.Schema;
using System.Threading.Tasks;

namespace ClothesShop.Models
{
    public class ArticleGroup : IValidatableObject
    {
        public long ArticleGroupId { get; set; }

        public long? ParentArticleGroupId { get; set; }
        public ArticleGroup? ParentArticleGroup { get; set; }


        [Required(ErrorMessage = "Please enter a valid name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter a valid description")]
        public string Description { get; set; }

        public string? Image { get; set; }

        [NotMapped]
        public IBrowserFile? ImageFile { get; set; }

        public virtual List<ArticleGroupItem> ArticleGroupItems { get; set; }
        public virtual List<ArticleGroup> ArticleGroups { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (this.ParentArticleGroupId is null && string.IsNullOrEmpty(Image))
            {
                yield return new ValidationResult(
                    $"Molimo Vas unesite sliku grupe artikala",
                    [nameof(this.Image)]);
            }
        }
    }
}
