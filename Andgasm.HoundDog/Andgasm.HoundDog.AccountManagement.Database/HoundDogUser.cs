using Microsoft.AspNetCore.Identity;

namespace Andgasm.HoundDog.AccountManagement.Database
{
    public class HoundDogUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public byte[] ProfileAvatar { get; set; }
    }
}
