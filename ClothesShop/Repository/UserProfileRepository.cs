using Microsoft.EntityFrameworkCore;
using ClothesShop.Models;
using System.Linq;

namespace ClothesShop.Repository
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private DatabaseContext _context;

        public UserProfileRepository(DatabaseContext ctx) => _context = ctx;

        public IQueryable<UserProfile> UserProfiles => _context.Profiles;

        public UserProfile SaveProfile(UserProfile userProfile)
        {

            if (userProfile.Id == 0)
            {
                _context.Profiles.Add(userProfile);
            }

            _context.SaveChanges();

            return _context.Profiles
                .FirstOrDefault(x => x.Id == userProfile.Id);
        }

    
}
}
