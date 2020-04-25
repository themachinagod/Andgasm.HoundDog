using Andgasm.HoundDog.AccountManagement.Interfaces;
using System.Linq;
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
            var user = await FindUserForIdentifier(username);
            if (user == null) return (null, new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedUserName), "Specified user does not exist!"));

            var signinresult = await _signinManager.PasswordSignInAsync(user, password, true, false);
            if (signinresult.Succeeded)
            {
                var userdto = await MapUserToDTOWithRoles(user);
                return (userdto, null);
            }
            else if (signinresult.RequiresTwoFactor)
            {
                var tfaresult = await _userauthManager.ConfirmAuthenticatorCode(user.Id, verificationCode);
                if (!tfaresult.Succeeded) return (null, new FieldValidationErrorDTO(nameof(UserSignInDTO.VerificationCode), "Your authentication code could not be validated!"));

                var userdto = await MapUserToDTOWithRoles(user);                
                return (userdto, null);
            }
            else if (signinresult.IsLockedOut) return (null, new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedPassword), "This account is locked!"));
            else 
            {
                return (null, new FieldValidationErrorDTO(nameof(UserSignInDTO.SuppliedPassword), "Specified password is not correct for user!"));
            }
        }

        public async Task<bool> UsernameRequires2FAVerification(string useridentifier)
        {
            var user = await FindUserForIdentifier(useridentifier);
            if (user == null) return false;
            return user.TwoFactorEnabled;
        }
        #endregion

        private async Task<UserDTO> MapUserToDTOWithRoles(HoundDogUser user)
        {
            var userdto = _mapper.Map<UserDTO>(user);
            userdto.Roles = string.Join(", ", await _signinManager.UserManager.GetRolesAsync(user));
            return userdto;
        }

        private async Task<HoundDogUser> FindUserForIdentifier(string useridentifier)
        {
            var user = await _signinManager.UserManager.FindByNameAsync(useridentifier);
            if (user == null) user = await _signinManager.UserManager.FindByEmailAsync(useridentifier);
            if (user == null) user = await _signinManager.UserManager.FindByIdAsync(useridentifier);
            return user;
        }
    }
}
