using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using ClothesShop.Enums;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace ClothesShop.Models
{
    public class Order
    {
        public long OrderId { get; set; }

        public virtual List<OrderLine> Lines { get; set; }

        [Required(ErrorMessage = "Please enter a name")]
        [DisplayName("Ime i prezime")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please enter an email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter the address")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Please enter a city name")]
        public string City { get; set; }

        [Required(ErrorMessage = "Please enter a state name")]
        public string State { get; set; }

        public string Zip { get; set; }

        [Required(ErrorMessage = "Please enter a country name")]
        public string Country { get; set; }

        public bool GiftWrap { get; set; }

        [BindNever]
        public bool Shipped { get; set; }

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public string Note { get; set; }

        public int? UserProfileId { get; set; }

        public UserProfile UserProfile { get; set; }
    }
}