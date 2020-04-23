using System;
using System.Collections.Generic;
using System.Text;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public interface ITokenGenerator
    {
        public static readonly string TokenConfigName = "Security:TokenKey";
        public static readonly string TokenExpiryConfigName = "Security:TokenExpiryHours";

        string GenerateToken(UserDTO authenticateduser);
    }
}
