using Andgasm.HoundDog.AccountManagement.Interfaces;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;
using Andgasm.HoundDog.API.Interfaces;
using Andgasm.HoundDog.Core.Email.Interfaces;
using Microsoft.Extensions.Configuration;
using Andgasm.HoundDog.AccountManagement.Database;
using System.Web;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class UserPasswordManager : IUserPasswordManager
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<UserManager> _logger;
        private readonly IEmailSender _emailer;
        private readonly UserManager<HoundDogUser> _userManager;
        private readonly IUserTwoFactorAuthManager _userauthManager;
        #endregion

        #region Constructor
        public UserPasswordManager(IConfiguration config,
                                         ILogger<UserManager> logger,
                                         IEmailSender emailer,
                                         IUserTwoFactorAuthManager userauthManager,
                                         UserManager<HoundDogUser> userManager)
        {
            _config = config;
            _logger = logger;
            _userManager = userManager;
            _emailer = emailer;
            _userauthManager = userauthManager;
        }
        #endregion

        #region Password Change
        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ChangePassword(string userid, string oldpassword, string newpassword, string verifycode)
        {
            if (string.IsNullOrWhiteSpace(userid)) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide user id to change your password!") });
            if (string.IsNullOrWhiteSpace(oldpassword) || string.IsNullOrWhiteSpace(newpassword))
                return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserDTO.OldPasswordClear), "You must provide both old and new passwords in order to change your password!") });

            var user = await _userManager.FindByIdAsync(userid);
            if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!") });

            if (user.TwoFactorEnabled)
            {
                var tfaresult = await _userauthManager.ConfirmAuthenticatorCode(user.Id, verifycode);
                if (!tfaresult.Succeeded) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.VerificationCode), "Your authentication code could not be validated!") });
            }

            var passchangeresult = await _userManager.ChangePasswordAsync(user, oldpassword, newpassword);
            if (!passchangeresult.Succeeded) return (false, passchangeresult.Errors.Select(x => new FieldValidationErrorDTO(PasswordFieldMappingHelper.MapErrorCodeToKey(x.Code), x.Description)));

            var changeconfresult = await GeneratePasswordChangeNotification(user);
            if (!changeconfresult.Succeeded) return (false, changeconfresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Key), x.Description)));

            return (true, new List<FieldValidationErrorDTO>());
        }

        private async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> GeneratePasswordChangeNotification(HoundDogUser user)
        {
            try
            {
                // TODO: buid body
                await _emailer.SendEmailAsync(_config.GetSection("AppSettings:SendingFromAddress")?.Value, user.Email, "HoundDog password change notification", "This is a notification to tell you that your password has been successfully changed, if you did not instigate this action please contact HoundDog security at once!");

                return (true, new List<FieldValidationErrorDTO>());
            }
            catch (Exception ex)
            {
                return (false, null);
            }
        }
        #endregion

        #region Password Reset
        public async Task<(bool Succeeded, IEnumerable<FieldValidationErrorDTO> Errors)> ResetPassword(Guid userid, string username, string password, string resettoken, string verifycode)
        {
            if (string.IsNullOrWhiteSpace(username) || userid == Guid.Empty || string.IsNullOrWhiteSpace(password))
                return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserPasswordChangeDTO.OldPassword), "You must provide both username, userid & new password in order to reset password!") });

            var user = await _userManager.FindByIdAsync(userid.ToString());
            if (user == null) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserPasswordChangeDTO.Username), "Specified user does not exist!") });
            if (user.UserName.ToLowerInvariant() != username.ToLowerInvariant()) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserPasswordChangeDTO.Username), "Specified username does not match the password reset request!") });

            if (user.TwoFactorEnabled) 
            {
                var tfaresult = await _userauthManager.ConfirmAuthenticatorCode(user.Id, verifycode);
                if (!tfaresult.Succeeded) return (false, new List<FieldValidationErrorDTO>() { new FieldValidationErrorDTO(nameof(UserSignInDTO.VerificationCode), "Your authentication code could not be validated!") });
            }

            var resetresult = await _userManager.ResetPasswordAsync(user, resettoken, password);
            if (!resetresult.Succeeded) return (false, resetresult.Errors.Select(x => new FieldValidationErrorDTO(x.Code, x.Description)));

            var changeconfresult = await GeneratePasswordChangeNotification(user);
            if (!changeconfresult.Succeeded) return (false, changeconfresult.Errors.Select(x => new FieldValidationErrorDTO(FieldMappingHelper.MapErrorCodeToKey(x.Key), x.Description)));

            return (true, new List<FieldValidationErrorDTO>());
        }

        public async Task<(bool Succeeded, FieldValidationErrorDTO Error)> GeneratePasswordReset(string username, string verificationcode, string email)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null) return (false, new FieldValidationErrorDTO(nameof(UserDTO.UserName), "Specified user does not exist!"));
            if (user.Email.ToLowerInvariant() != email.ToLowerInvariant()) return (false, new FieldValidationErrorDTO(nameof(UserPasswordChangeDTO.Username), "Specified username and email do not correlate, please ensure you provide email address associated with the specified username!"));

            if (user.TwoFactorEnabled)
            {
                var tfaresult = await _userauthManager.ConfirmAuthenticatorCode(user.Id, verificationcode);
                if (!tfaresult.Succeeded) return (false, new FieldValidationErrorDTO(nameof(UserSignInDTO.VerificationCode), "Your authentication code could not be validated!"));
            }

            return await SendPasswordResetLink(user);
        }

        private async Task<(bool Succeeded, FieldValidationErrorDTO Error)> SendPasswordResetLink(HoundDogUser user)
        {
            try
            {
                var confirmToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedtoken = HttpUtility.UrlEncode(confirmToken); // WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(confirmToken));
                var callbackUrl = $"{_config.GetSection("WebClientLinks:BaseUrl")?.Value}resetpassword?userid={user.Id}&token={encodedtoken}";
                var body = $"Please follow the below link to reset your password;\n\n{callbackUrl}";
                await _emailer.SendEmailAsync(_config.GetSection("AppSettings:SendingFromAddress")?.Value, user.Email, "HoundDog password reset request", body);
                return (true, new FieldValidationErrorDTO());
            }
            catch (Exception ex)
            {
                return (false, new FieldValidationErrorDTO(string.Empty, ex.Message));
            }
        }
        #endregion
    }
}
