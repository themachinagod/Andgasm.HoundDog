using Andgasm.HoundDog.API.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public interface IUserManager
    {
        Task<(UserDTO User, IEnumerable<FieldValidationErrorDTO> Errors)> GetCurrentUser(ClaimsPrincipal currentuser);
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> CreateStandardUser(UserDTO userdata);
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> UpdateUser(UserDTO userdata);
    }
}
