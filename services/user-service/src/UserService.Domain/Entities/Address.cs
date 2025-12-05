using System;
using UserService.Domain.Common;
using UserService.Domain.Enums;

namespace UserService.Domain.Entities
{
    public class Address : BaseEntity
    {
        public Guid AddressId { get; set; }
        public Guid UserId { get; set; }
        public AddressType Type { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public bool IsDefault { get; set; }

        public User User { get; set; }
    }
}
