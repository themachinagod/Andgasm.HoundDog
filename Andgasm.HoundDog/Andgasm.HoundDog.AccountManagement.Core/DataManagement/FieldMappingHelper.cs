using Andgasm.HoundDog.AccountManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public static class FieldMappingHelper
    {
        public static string MapErrorCodeToKey(string code)
        {
            if (code.ToLowerInvariant().Contains("username")) return nameof(UserDTO.UserName);
            if (code.ToLowerInvariant().Contains("password")) return nameof(UserDTO.PasswordClear);
            if (code.ToLowerInvariant().Contains("email")) return nameof(UserDTO.Email);
            return "";
        }
    }

    public static class PasswordFieldMappingHelper
    {
        public static string MapErrorCodeToKey(string code)
        {
            if (code.ToLowerInvariant().Contains("passwordmismatch")) return nameof(UserDTO.OldPasswordClear);
            if (code.ToLowerInvariant().Contains("password")) return nameof(UserDTO.PasswordClear);
            return "";
        }
    }
}
