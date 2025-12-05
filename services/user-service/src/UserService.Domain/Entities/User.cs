using System;
using System.Collections.Generic;
using UserService.Domain.Common;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities
{
    public class User : BaseEntity
    {
        public Guid UserId { get; set; }
        public string KeycloakId { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string ProfilePictureUrl { get; set; }
        public UserStatus Status { get; set; }

        public ICollection<Address> Addresses { get; set; }
        public UserPreference Preference { get; set; }
        public UserStatistics Statistics { get; set; }
    }
}
