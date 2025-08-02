using System;

namespace ClothesShop.Models.ViewModels
{
    public class PagingInfo
    {
        public long TotalItems { get; set; }
        public int ItemsPerPage { get; set; }
        public int CurrentPage { get; set; }
        public long TotalPages => (long)Math.Ceiling((decimal)TotalItems / ItemsPerPage);

        public long? CurrentGroup { get; set; }
        public long? CurrentSubGroup { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is null || obj is not PagingInfo)
            {
                return false;
            }

            var other = (PagingInfo)obj;
            var hc1 = GetHashCode();
            var hc2 = other.GetHashCode();
            return hc1 == hc2;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(TotalItems, ItemsPerPage, CurrentPage, TotalPages);
        }
    }
}
