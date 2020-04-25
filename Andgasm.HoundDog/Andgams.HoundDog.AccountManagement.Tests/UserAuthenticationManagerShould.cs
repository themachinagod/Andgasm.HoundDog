using Andgasm.HoundDog.AccountManagement.Core.AuthManagement;
using Andgasm.HoundDog.AccountManagement.Database;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Andgams.HoundDog.AccountManagement.Tests
{
    public class UserAuthenticationManagerShould
    {
        #region Test Fields
        HoundDogUser _testUser = new HoundDogUser()
        {
            UserName = "TestUser",
            Email = "TestUser@TestEmail.com",
        };
        HoundDogUser _testUser2FA = new HoundDogUser()
        {
            UserName = "TestUser2",
            Email = "TestUser@TestEmail.com",
            TwoFactorEnabled = true
        };
        UserDTO _testUserDto = new UserDTO()
        {
            UserName = "TestUser",
            Email = "TestUser@TestEmail.com",
        };
        #endregion

        #region Determine 2FA Status Tests
        [Fact]
        public async Task SuccessfullyDetermineTrue2FAStatus_WhenValidUsernameSupplied()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.UsernameRequires2FAVerification("TestUser2");
            Assert.True(result);
        }

        [Fact]
        public async Task SuccessfullyDetermineFalse2FAStatus_WhenValidUsernameSupplied()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.UsernameRequires2FAVerification("TestUser");
            Assert.False(result);
        }

        [Fact]
        public async Task ReturnFalse2FAStatus_WhenInvalidUsernameSupplied()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.UsernameRequires2FAVerification("TestUserInvalid");
            Assert.False(result);
        }
        #endregion

        #region Basic Authenticate Tests
        [Fact]
        public async Task SuccessfullyAuthenticate_WhenValidUserCredentialsRecieved()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.AuthenticateUserCredentials("TestUser", "TestPassword", "123456");
            Assert.NotNull(result.ValidatedUser);
            Assert.Equal("TestUser", result.ValidatedUser.UserName);
            Assert.Equal("User", result.ValidatedUser.Roles);
        }

        [Fact]
        public async Task FailedAuthenticate_WhenInvalidUserPasswordRecieved()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.AuthenticateUserCredentials("TestUser", "TestPasswordWRONG", "123456");
            Assert.Null(result.ValidatedUser);
            Assert.Equal("Specified password is not correct for user!", result.Error.Description);
        }

        [Fact]
        public async Task FailedAuthenticate_WhenInvalidUserNameRecieved()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.AuthenticateUserCredentials("TestUserInvalid", "TestPassword", "123456");
            Assert.Null(result.ValidatedUser);
            Assert.Equal("Specified user does not exist!", result.Error.Description);
        }
        #endregion

        #region 2FA Authentication Tests
        [Fact]
        public async Task SuccessfullyAuthenticate_WhenValidUserCredentialsValid2FARecieved()
        {
            // TODO: need to mock 2fa validation cases!
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.AuthenticateUserCredentials("TestUser", "TestPassword2FA", "999999");
            Assert.NotNull(result.ValidatedUser);
            Assert.Equal("TestUser", result.ValidatedUser.UserName);
            Assert.Equal("User", result.ValidatedUser.Roles);
        }

        [Fact]
        public async Task Fail2FAChallenge_WhenValidUserCredentialsInvalid2FARecieved()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.AuthenticateUserCredentials("TestUser", "TestPassword2FA", "123456");
            Assert.Null(result.ValidatedUser);
            Assert.Equal("Your authentication code could not be validated!", result.Error.Description);
        }

        [Fact]
        public async Task Fail2FAChallenge_WhenInvalidUserCredentialsValid2FARecieved()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.AuthenticateUserCredentials("TestUser", "TestPasswordWrong", "999999");
            Assert.Null(result.ValidatedUser);
            Assert.Equal("Specified password is not correct for user!", result.Error.Description);
        }
        #endregion

        #region Lockout Authentication Tests
        [Fact]
        public async Task LockedAuthenticate_WhenValidUserCredentialsLockedRecieved()
        {
            var _authmanager = InitialiseAuthenticationManager();
            var result = await _authmanager.AuthenticateUserCredentials("TestUser", "TestPasswordLocked", "123456");
            Assert.Null(result.ValidatedUser);
            Assert.Equal("This account is locked!", result.Error.Description);
        }
        #endregion

        #region Mock Helpers
        private IUserAuthenticationManager InitialiseAuthenticationManager()
        {
            var config = new Mock<IConfiguration>();
            var twofasvc = Mock2FAManager();
            var mapper = MockUserMapper();
            var usermanager = MockUserManager();
            var signinmanager = MockSignInManager(usermanager);

            return new UserAuthenticationManager(config.Object,
                                                    new NullLogger<UserAuthenticationManager>(),
                                                    mapper.Object,
                                                    twofasvc.Object,
                                                    signinmanager.Object);
        }

        private Mock<SignInManager<HoundDogUser>>  MockSignInManager(Mock<UserManager<HoundDogUser>> manager)
        {
            var context = new Mock<HttpContext>();
            var sim = new Mock<SignInManager<HoundDogUser>>(manager.Object,
                                                    new HttpContextAccessor { HttpContext = context.Object },
                                                    new Mock<IUserClaimsPrincipalFactory<HoundDogUser>>().Object,
                                                    new Mock<IOptions<IdentityOptions>>().Object,
                                                    new NullLogger<SignInManager<HoundDogUser>>(),
                                                    new Mock<IAuthenticationSchemeProvider>().Object)
            { CallBase = true };

            sim.Setup(x => x.PasswordSignInAsync(_testUser, "TestPassword", true, false)).ReturnsAsync(SignInResult.Success); // allow success login
            sim.Setup(x => x.PasswordSignInAsync(_testUser, "TestPasswordWRONG", true, false)).ReturnsAsync(SignInResult.Failed); // allow fail login
            sim.Setup(x => x.PasswordSignInAsync(_testUser, "TestPassword2FA", true, false)).ReturnsAsync(SignInResult.TwoFactorRequired); // allow 2fa login
            sim.Setup(x => x.PasswordSignInAsync(_testUser, "TestPasswordLocked", true, false)).ReturnsAsync(SignInResult.LockedOut); // allow locked login

            return sim;
        }

        public Mock<UserManager<HoundDogUser>> MockUserManager()
        {
            IList<IUserValidator<HoundDogUser>> UserValidators = new List<IUserValidator<HoundDogUser>>();
            IList<IPasswordValidator<HoundDogUser>> PasswordValidators = new List<IPasswordValidator<HoundDogUser>>();

            var store = new Mock<IUserStore<HoundDogUser>>();
            UserValidators.Add(new UserValidator<HoundDogUser>());
            PasswordValidators.Add(new PasswordValidator<HoundDogUser>());
            var mgr = new Mock<UserManager<HoundDogUser>>(store.Object, null, null, UserValidators, PasswordValidators, null, null, null, null);

            mgr.Setup(x => x.FindByNameAsync(_testUser.UserName)).ReturnsAsync(_testUser); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUser)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            mgr.Setup(x => x.FindByNameAsync(_testUser2FA.UserName)).ReturnsAsync(_testUser2FA); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUser2FA)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            return mgr;
        }

        public Mock<IMapper> MockUserMapper()
        {
            var mapper = new Mock<IMapper>();
            mapper.Setup(x => x.Map<UserDTO>(It.IsAny<HoundDogUser>())).Returns(_testUserDto);
            return mapper;
        }

        public Mock<IUserTwoFactorAuthManager> Mock2FAManager()
        {
            var tfasvc = new Mock<IUserTwoFactorAuthManager>();
            tfasvc.Setup(x => x.ConfirmAuthenticatorCode(It.IsAny<string>(), "999999")).ReturnsAsync((true, null));
            return tfasvc;
        }
        #endregion
    }
}
