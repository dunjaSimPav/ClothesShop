using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;

namespace ClothesShop.Models
{
    public class Article
    {
        public long ArticleId { get; set; }
        [Required(ErrorMessage = "Molimo Vas unesite naziv proizvoda!")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Molimo Vas unesite opis proizvoda!")]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Molimo Vas unesite cenu proizvoda veću od 0!")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Molimo Vas unesite sliku proizvoda!")]
        public string? Image { get; set; }

        [NotMapped]
        public IBrowserFile? ImageFile { get; set; }

        public string Tags { get; set; }

        public virtual List<ArticleGroupItem> ArticleGroupItems { get; set; }

        public virtual List<OrderLine> OrderLines { get; set; }
    }
}
