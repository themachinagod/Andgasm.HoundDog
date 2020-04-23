using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Andgasm.HoundDog.API.Core;
using Andgasm.HoundDog.AccountManagement.API.Auth;

namespace Andgasm.HoundDog.AccountManagement.API
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : APIController
    {
        #region Fields
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _config;
        private readonly IUserManager _userManager;
        #endregion

        #region Constructors
        public UserController(ILogger<AuthenticationController> logger,
                              IConfiguration config,
                              IUserManager userManager)
        {
            _logger = logger;
            _config = config;
            _userManager = userManager;
        }
        #endregion

        #region Connection Tests
        [HttpGet("amirunning")]
        public IActionResult TestRunning()
        {
            return Ok("We are up and running!");
        }

        [HttpGet("amiauthenticated")]
        [Authorize]
        public IActionResult TestAuthenticated()
        {
            return Ok($"We are authenticated as user '{HttpContext.User.Identity.Name}'!");
        }

        [HttpGet("amiauthenticatedadmin")]
        [Authorize(Roles = "administrator")]
        public IActionResult TestAuthenticatedAdmin()
        {
            return Ok($"We are authenticated as administrator user '{HttpContext.User.Identity.Name}'!");
        }
        #endregion

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var user = await _userManager.GetCurrentUser(HttpContext.User);
            return Ok(user.User);
        }

        [HttpGet("{userid}")]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> GetCurrentUserById(string userid)
        {
            if (string.IsNullOrWhiteSpace(userid)) return CreateBadRequestError(string.Empty, "No user data was supplied on the request!");
            var user = await _userManager.GetCurrentUser(HttpContext.User); // TODO: update manager to allow get by id!
            return Ok(user.User);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] UserDTO userdata)
        {
            // NOTE: this is open endpoint due to registration requirements - any way we could stop bot spamming accounts?
            if (userdata == null) return CreateBadRequestError(string.Empty, "No user data was supplied on the request!");
            var result = await _userManager.CreateStandardUser(userdata);
            if (!result.Succeeded) return CreateBadRequestError(result.Errors);
            return StatusCode(201);
        }

        [HttpPut("{userid}")]
        [Authorize(Policy = Constants.MatchesCurrentUserPolicy)]
        public async Task<IActionResult> UpdateAsync(string userid, [FromBody] UserDTO userdata)
        {
            if (userdata == null || string.IsNullOrWhiteSpace(userid)) return CreateBadRequestError(string.Empty, "No user data was supplied on the request!");
            var result = await _userManager.UpdateUser(userdata);
            if (!result.Succeeded) return CreateBadRequestError(result.Errors);
            return Ok(userdata);
        }
    }
}
