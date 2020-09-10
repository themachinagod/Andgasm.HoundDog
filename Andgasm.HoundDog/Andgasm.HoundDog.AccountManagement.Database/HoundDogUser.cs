using Microsoft.AspNetCore.Identity;
using System;

namespace Andgasm.HoundDog.AccountManagement.Database
{
    public class HoundDogUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public DateTime RegisteredTimestamp { get; set; }
        public DateTime? EmailConfirmedTimestamp { get; set; }
        public byte[] ProfileAvatar { get; set; }
    }
}
