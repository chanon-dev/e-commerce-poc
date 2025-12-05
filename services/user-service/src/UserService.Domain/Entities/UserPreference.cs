using System;
using UserService.Domain.Common;

namespace UserService.Domain.Entities
{
    public class UserPreference : BaseEntity
    {
        public Guid PreferenceId { get; set; }
        public Guid UserId { get; set; }
        public string Language { get; set; }
        public string Timezone { get; set; }
        public string Currency { get; set; }
        public bool EmailNotifications { get; set; }
        public bool SmsNotifications { get; set; }
        public bool MarketingConsent { get; set; }

        public User User { get; set; }
    }
}
