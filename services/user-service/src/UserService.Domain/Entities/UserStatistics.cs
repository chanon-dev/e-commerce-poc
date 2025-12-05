using System;
using UserService.Domain.Common;

namespace UserService.Domain.Entities
{
    public class UserStatistics : BaseEntity
    {
        public Guid StatisticsId { get; set; }
        public Guid UserId { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalSpent { get; set; }
        public int WishlistCount { get; set; }
        public DateTime? LastOrderDate { get; set; }
        public DateTime? LastLoginDate { get; set; }

        public User User { get; set; }
    }
}
