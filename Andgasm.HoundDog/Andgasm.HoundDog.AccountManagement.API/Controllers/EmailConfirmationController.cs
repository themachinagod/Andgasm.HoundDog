using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Andgasm.HoundDog.API.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Andgasm.HoundDog.AccountManagement.API.Auth;

namespace Andgasm.HoundDog.AccountManagement.API
{
    [ApiController]
    [Route("api/user/{userid}/[controller]")]
    public class EmailConfirmationController : APIController
    {
        #region Fields
        private readonly IConfiguration _config;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IUserEmailManager _userEmailManager;
        #endregion

        #region Constructors
        public EmailConfirmationController(ILogger<AuthenticationController> logger,
                                           IConfiguration config,
                                           IUserEmailManager emailManager)
        {
            _config = config;
            _logger = logger;
            _userEmailManager = emailManager;
        }
        #endregion

        #region Mail Conformation Management
        [HttpGet]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> SendEmailConfirmation(string userid)
        {
            var result = await _userEmailManager.GenerateEmailConfirmation(userid);
            if (!result.Succeeded) return CreateBadRequestError(result.Error.Key, result.Error.Description);
            return StatusCode(201);
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmEmail(string userid, [FromQuery] string token)
        {
            // TODO: we should prob ask user to confirm the username of the account to provide at least a little security!
            if (string.IsNullOrWhiteSpace(token)) return CreateBadRequestError(string.Empty, "Confirmation token must be provided to successfully confirm email for account!");
            var result = await _userEmailManager.ConfirmEmailAddress(userid, token);
            return this.Redirect(BuildConfirmationRedirectUrl(result.Succeeded));
        }
        #endregion

        #region Redirect Construction
        private string BuildConfirmationRedirectUrl(bool success)
        {
            // TODO: instead oredirecting to success page... perhaps we route to the login page and show a toast message to inform user of success or fail
            //       if user is logged in??? we dont want to go to login - we want to go to home which would have to managed on client side - we still need to see the toast message tho
            //       in the event of failure the user should be able to 
            var baseurl = _config.GetSection("WebClientLinks:BaseUrl")?.Value;
            var weburl = $"{baseurl}mailconfirmation?success={success}";
            return weburl;
        }
        #endregion
    }
}
