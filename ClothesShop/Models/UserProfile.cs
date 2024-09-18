using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClothesShop.Models
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }

        public string Zip { get; set; }

        public string State { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string Country { get; set; }

        public string AccountId { get; set; }

        [NotMapped]
        public IdentityUser Account { get; set; }

    }
}
