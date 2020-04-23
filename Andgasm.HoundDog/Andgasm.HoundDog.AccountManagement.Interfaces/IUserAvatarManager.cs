using Andgasm.HoundDog.API.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public interface IUserAvatarManager
    {
        Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> UploadAvatar(string userid, byte[] payload);
        Task<(byte[] ImageData, IEnumerable<FieldValidationErrorDTO> Errors)> GetAvatar(string userid);
    }
}
