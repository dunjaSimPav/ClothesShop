using ClothesShop.Models;
using System.Linq;

namespace ClothesShop.Repository
{
    public interface IUserProfileRepository
    {
        IQueryable<UserProfile> UserProfiles { get; }
        UserProfile SaveProfile(UserProfile userProfile);
    }
}
