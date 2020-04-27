using Andgasm.HoundDog.AccountManagement.Interfaces;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity;
using Andgasm.HoundDog.API.Interfaces;
using Microsoft.Extensions.Configuration;
using Andgasm.HoundDog.AccountManagement.Database;
using AutoMapper;

namespace Andgasm.HoundDog.AccountManagement.Core.AuthManagement
{
    public class UserAuthenticationManager : IUserAuthenticationManager
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<UserAuthenticationManager> _logger;
        private readonly SignInManager<HoundDogUser> _signinManager;
        private readonly IUserTwoFactorAuthManager _userauthManager;
        private readonly IMapper _mapper;
        #endregion

        #region Constructor
        public UserAuthenticationManager(IConfiguration config,
                                         ILogger<UserAuthenticationManager> logger,
                                         IMapper mapper,
                                         IUserTwoFactorAuthManager userauthManager,
                                         SignInManager<HoundDogUser> signinManager)
        {
            _config = config;
            _logger = logger;
            _signinManager = signinManager;
            _mapper = mapper;
            _userauthManager = userauthManager;
        }
        #endregion

        #region Authentication
        public async Task<(UserDTO ValidatedUser, FieldValidationErrorDTO Error)> AuthenticateUserCredentials(string username, string password, string verificationCode)
        {
            _logger.LogDebug($"Authenticating for user {username}");
            var user = await FindUserForIdentifier(username);
            if (user == null) return UserDoesNotExist(username);

            var signinresult = await _signinManager.PasswordSignInAsync(user, password, true, false);
            if (signinresult.Succeeded)
            {
                var userdto = await MapUserToDTOWithRoles(user);
                return (userdto, null);
            }
            else if (signinresult.RequiresTwoFactor)
            {
                var tfaresult = await _userauthManager.ConfirmAuthenticatorCode(user.Id, verificationCode);
                if (!tfaresult.Succeeded) return Failed2FAChallenge(verificationCode, user.Id);

                var userdto = await MapUserToDTOWithRoles(user);
                return (userdto, null);
            }
            else if (signinresult.IsLockedOut) return AccountLockedOut(user.Id);
            else return AuthenticationFailed(user.Id);
        }

        public async Task<bool> UsernameRequires2FAVerification(string useridentifier)
        {
            _logger.LogDebug($"Determining 2FA status for user {useridentifier}");
            var user = await FindUserForIdentifier(useridentifier);
            if (user == null) return false;
            return user.TwoFactorEnabled;
        }

        #endregion

        #region Error Responses
        private (UserDTO ValidatedUser, FieldValidationErrorDTO Error) UserDoesNotExist(string username)
        {
            _logger.LogError($"Specified username {username} does not exist!");
            return (null, new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!"));
        }

        private (UserDTO ValidatedUser, FieldValidationErrorDTO Error) Failed2FAChallenge(string code, string userid)
        {
            _logger.LogError($"The authentication code {code} could not be validated for user {userid}!");
            return (null, new FieldValidationErrorDTO(nameof(UserSignInDTO.VerificationCode), "Your authentication code could not be validated!"));
        }

        private (UserDTO ValidatedUser, FieldValidationErrorDTO Error) AccountLockedOut(string userid)
        {
            _logger.LogError($"Attempt to sign in to account for user {userid} is locked!!");
            return (null, new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "This account is locked!"));
        }

        private (UserDTO ValidatedUser, FieldValidationErrorDTO Error) AuthenticationFailed(string userid)
        {
            _logger.LogError($"Attempt to sign in to account for user {userid} failed!!");
            return (null, new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedPassword), "Specified password is not correct for user!"));
        }
        #endregion

        #region Mappings
        private async Task<UserDTO> MapUserToDTOWithRoles(HoundDogUser user)
        {
            _logger.LogDebug($"Mapping db user {user.Id} to dto user");
            var userdto = _mapper.Map<UserDTO>(user);
            userdto.Roles = string.Join(", ", await _signinManager.UserManager.GetRolesAsync(user));
            return userdto;
        }

        private async Task<HoundDogUser> FindUserForIdentifier(string useridentifier)
        {
            _logger.LogDebug($"Finding user by id {useridentifier}");
            var user = await _signinManager.UserManager.FindByNameAsync(useridentifier);
            if (user == null) user = await _signinManager.UserManager.FindByEmailAsync(useridentifier);
            if (user == null) user = await _signinManager.UserManager.FindByIdAsync(useridentifier);
            return user;
        }
        #endregion
    }
}
