using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Andgasm.HoundDog.API.Core;

namespace Andgasm.HoundDog.AccountManagement.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : APIController
    {
        #region Fields
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserAuthenticationManager _authmanager;
        private readonly ITokenGenerator _tokenManager;
        #endregion

        #region Constructors
        public AuthenticationController(ILogger<AuthenticationController> logger,
                                        ITokenGenerator tokenManager,
                                        IUserAuthenticationManager authManager)
        {
            _logger = logger;
            _tokenManager = tokenManager;
            _authmanager = authManager;
        }
        #endregion

        #region Authenticate
        [HttpGet("{useridentifier}")]
        public async Task<IActionResult> Requires2FA(string useridentifier)
        {
            return Ok(await _authmanager.UsernameRequires2FAVerification(useridentifier));
        }

        [HttpPost]
        public async Task<IActionResult> Authenticate(UserSignInDTO userdata)
        {
            var loginresult = await _authmanager.AuthenticateUserCredentials(userdata.SuppliedUserName.ToLower(), userdata.SuppliedPassword, userdata.VerificationCode);
            if (loginresult.ValidatedUser == null) return CreateUnauthorizedError(loginresult.Error);
            loginresult.ValidatedUser.Token = _tokenManager.GenerateToken(loginresult.ValidatedUser);
            return Ok(loginresult.ValidatedUser);
        }
        #endregion
    }
}
