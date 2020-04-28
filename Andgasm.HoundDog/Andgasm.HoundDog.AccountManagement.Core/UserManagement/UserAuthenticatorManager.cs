using Andgasm.HoundDog.AccountManagement.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;
using Andgasm.HoundDog.API.Interfaces;
using Andgasm.HoundDog.AccountManagement.Database;
using System.Text;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class UserAuthenticatorManager : IUserTwoFactorAuthManager
    {
        #region Fields
        private readonly ILogger<UserAuthenticatorManager> _logger;
        private readonly UserManager<HoundDogUser> _userManager;
        #endregion

        #region Constructor
        public UserAuthenticatorManager(ILogger<UserAuthenticatorManager> logger,
                                         UserManager<HoundDogUser> userManager)
        {
            _logger = logger;
            _userManager = userManager;
        }
        #endregion

        #region 2FA Confirmation
        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ConfirmAuthenticatorCode(string userid, string token)
        {
            if (string.IsNullOrWhiteSpace(userid) || string.IsNullOrWhiteSpace(token))
                return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide a user id  and authenticator token to confirm authenticator code!") });

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });

            var authsuccess = await _userManager.VerifyTwoFactorTokenAsync(user, _userManager.Options.Tokens.AuthenticatorTokenProvider, token);
            if (!authsuccess) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.VerificationCode), "Authenticator code could not be validated!") });

            return (true, new List<FieldValidationErrorDTO>());
        }

        public async Task<(AuthenticatorPayloadDTO GeneratedCode, FieldValidationErrorDTO Error)> GenerateAuthenticatorSharedKey(string userid)
        {
            if (string.IsNullOrWhiteSpace(userid))
                return (null, new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide a user id to generate the shared key!"));

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (null, new FieldValidationErrorDTO(nameof(UserDTO.UserName), "Specified user does not exist!"));
            return await GenerateAuthenticatorSharedKey(user);
        }

        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> Enable2FA(string userid, string token)
        {
            if (string.IsNullOrWhiteSpace(userid) || string.IsNullOrWhiteSpace(token))
                return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide a user id & authenticator token to enable 2FA!") });

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });
            if (user.TwoFactorEnabled) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user is already enrolled for 2FA!") });

            var authresult = await ConfirmAuthenticatorCode(userid, token);
            if (authresult.Succeeded)
            {
                if (!user.TwoFactorEnabled)
                {
                    var updateResult = await _userManager.SetTwoFactorEnabledAsync(user, true);
                    if (!updateResult.Succeeded) return (false, updateResult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));
                }
                return (true, new List<FieldValidationErrorDTO>());
            }
            return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.VerificationCode), "Authenticator code could not be validated!") });
        }

        public async Task<(bool Success, IEnumerable<FieldValidationErrorDTO> Errors)> Disable2FA(string userid)
        {
            if (string.IsNullOrWhiteSpace(userid))
                return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide a user id to enable 2FA!") });

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });
            if (!user.TwoFactorEnabled) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user is not enrolled for 2FA!") });

            var disableresult = await _userManager.SetTwoFactorEnabledAsync(user, false);
            if (!disableresult.Succeeded) return (false, disableresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

            var resetresult =  await _userManager.ResetAuthenticatorKeyAsync(user);
            if (!resetresult.Succeeded) return (false, resetresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

            return (true, new List<FieldValidationErrorDTO>());
        }

        private async Task<(AuthenticatorPayloadDTO GeneratedCode, FieldValidationErrorDTO Error)> GenerateAuthenticatorSharedKey(HoundDogUser user)
        {
            var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
            var formattedkey = FormatKey(unformattedKey);
            var qrcodeuri = GenerateQrCodeUri(user.Email, unformattedKey);
            return (new AuthenticatorPayloadDTO() { SharedKey = formattedkey, QrCodeUri = qrcodeuri }, null);
        }
        #endregion

        #region Shared Key Helpers
        private string FormatKey(string unformattedKey)
        {
            var result = new StringBuilder();
            int currentPosition = 0;
            while (currentPosition + 4 < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition, 4)).Append(" ");
                currentPosition += 4;
            }
            if (currentPosition < unformattedKey.Length)
            {
                result.Append(unformattedKey.Substring(currentPosition));
            }

            return result.ToString().ToLowerInvariant();
        }

        private string GenerateQrCodeUri(string email, string unformattedKey)
        {
            return string.Format(
                "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits=6",
                "HoundDog",
                email,
                unformattedKey);
        }
        #endregion
    }
}
