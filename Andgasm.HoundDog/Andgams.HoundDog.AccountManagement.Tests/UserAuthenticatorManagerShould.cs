using Andgasm.HoundDog.AccountManagement.Core;
using Andgasm.HoundDog.AccountManagement.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Andgams.HoundDog.AccountManagement.Tests
{
    public class UserAuthenticatorManagerShould
    {
        #region Test Fields
        HoundDogUser _testUser = new HoundDogUser()
        {
            Id = "fd50153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUser",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666],
            TwoFactorEnabled = false
        };
        HoundDogUser _testUser2FA = new HoundDogUser()
        {
            Id = "bd50153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUser2FA",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666],
            TwoFactorEnabled = true
            
        };
        HoundDogUser _testUserFail = new HoundDogUser()
        {
            Id = "cb10153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUserFail",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666],
            TwoFactorEnabled = false
        };
        HoundDogUser _testUserFail2FA = new HoundDogUser()
        {
            Id = "db10153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUserFail2FA",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666],
            TwoFactorEnabled = true
        };
        HoundDogUser _testUserFail2FAK = new HoundDogUser()
        {
            Id = "eb10153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUserFail2FA",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666],
            TwoFactorEnabled = true
        };
        #endregion

        #region Generate Authenticator Shared Key Tests
        [Fact]
        public async Task GenerateAuthenticatorSharedKey_WhenValidUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.GenerateAuthenticatorSharedKey(_testUser.Id);
            Assert.NotNull(avatar.GeneratedCode);
            Assert.NotNull(avatar.GeneratedCode.SharedKey);
            Assert.NotNull(avatar.GeneratedCode.QrCodeUri);
            Assert.Equal("anew code", avatar.GeneratedCode.SharedKey);
            Assert.Equal("otpauth://totp/HoundDog:TestUser@TestEmail.com?secret=ANewCode&issuer=HoundDog&digits=6", avatar.GeneratedCode.QrCodeUri);
        }

        [Fact]
        public async Task ReportError_WhenNullUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.GenerateAuthenticatorSharedKey(null);
            Assert.Null(avatar.GeneratedCode);
            Assert.Equal("You must provide a user id to generate the shared key!", avatar.Error.Description);
        }

        [Fact]
        public async Task ReportError_WhenNotExistsUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.GenerateAuthenticatorSharedKey(Guid.NewGuid().ToString());
            Assert.Null(avatar.GeneratedCode);
            Assert.Equal("Specified user does not exist!", avatar.Error.Description);
        }
        #endregion

        #region Confirm Authenticator Code Tests
        [Fact]
        public async Task ConfirmAuthenticatorCode_WhenValidUserIdAndCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.ConfirmAuthenticatorCode(_testUser.Id, "999999");
            Assert.True(avatar.Succeeded);
        }

        [Fact]
        public async Task ReportAuthenticatorError_WhenValidUserIdAndInvalidCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.ConfirmAuthenticatorCode(_testUser.Id, "123456");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Authenticator code could not be validated!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportAuthenticatorError_WhenNullUserIdAndValidCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.ConfirmAuthenticatorCode(null, "999999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("You must provide a user id  and authenticator token to confirm authenticator code!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportAuthenticatorError_WhenNonExistingUserIdAndValidCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.ConfirmAuthenticatorCode(Guid.NewGuid().ToString(), "999999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Specified user does not exist!", avatar.Errors.First().Description);
        }
        #endregion

        #region Enable 2FA Tests
        [Fact]
        public async Task Enable2FA_WhenValidUserIdAndCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Enable2FA(_testUser.Id, "999999");
            Assert.True(avatar.Succeeded);
        }

        [Fact]
        public async Task Report2FAEnableError_WhenValidUserIdAndInvalidCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Enable2FA(_testUser.Id, "123456");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Authenticator code could not be validated!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task Report2FAEnableError_WhenNullUserIdAndValidCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Enable2FA(null, "999999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("You must provide a user id & authenticator token to enable 2FA!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task Report2FAEnableError_WhenNonExistingUserIdAndValidCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Enable2FA(Guid.NewGuid().ToString(), "999999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Specified user does not exist!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportEnable2FAError_WhenUserEnabled2FA()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Enable2FA(_testUser2FA.Id, "999999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Specified user is already enrolled for 2FA!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportEnable2FAError_WhenEnableFails()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Enable2FA(_testUserFail.Id, "999999");
            Assert.False(avatar.Succeeded);
            Assert.Equal("Fail2FA", avatar.Errors.First().Description);
        }
        #endregion

        #region Disable 2FA Tests
        [Fact]
        public async Task Disable2FA_WhenValidUserIdAndCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Disable2FA(_testUser2FA.Id);
            Assert.True(avatar.Success);
        }

        [Fact]
        public async Task ReportDisable2FAError_WhenNullUserIdAndValidCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Disable2FA(null);
            Assert.False(avatar.Success);
            Assert.Equal("You must provide a user id to enable 2FA!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportDisable2FAError_WhenNonExistingUserIdAndValidCodeRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Disable2FA(Guid.NewGuid().ToString());
            Assert.False(avatar.Success);
            Assert.Equal("Specified user does not exist!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportDisable2FAError_WhenUserNotEnabled2FA()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Disable2FA(_testUser.Id);
            Assert.False(avatar.Success);
            Assert.Equal("Specified user is not enrolled for 2FA!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportDisable2FAError_WhenDisableFails()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Disable2FA(_testUserFail2FA.Id);
            Assert.False(avatar.Success);
            Assert.Equal("Fail2FA", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportDisable2FAError_WhenResetKeysFails()
        {
            var usrmgr = MockUserManager();
            var am = new UserAuthenticatorManager(new NullLogger<UserAuthenticatorManager>(), usrmgr.Object);
            var avatar = await am.Disable2FA(_testUserFail2FAK.Id);
            Assert.False(avatar.Success);
            Assert.Equal("FailReset", avatar.Errors.First().Description);
        }

        #endregion

        #region Mock Helpers
        public Mock<UserManager<HoundDogUser>> MockUserManager()
        {
            IList<IUserValidator<HoundDogUser>> UserValidators = new List<IUserValidator<HoundDogUser>>();
            IList<IPasswordValidator<HoundDogUser>> PasswordValidators = new List<IPasswordValidator<HoundDogUser>>();

            var store = new Mock<IUserStore<HoundDogUser>>();
            UserValidators.Add(new UserValidator<HoundDogUser>());
            PasswordValidators.Add(new PasswordValidator<HoundDogUser>());
            var mgr = new Mock<UserManager<HoundDogUser>>(store.Object, null, null, UserValidators, PasswordValidators, null, null, null, null);

            mgr.Setup(x => x.FindByIdAsync(_testUser.Id)).ReturnsAsync(_testUser); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUser)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            mgr.Setup(x => x.FindByIdAsync(_testUserFail.Id)).ReturnsAsync(_testUserFail); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUserFail)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            mgr.Setup(x => x.FindByIdAsync(_testUser2FA.Id)).ReturnsAsync(_testUser2FA); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUser2FA)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            mgr.Setup(x => x.FindByIdAsync(_testUserFail2FA.Id)).ReturnsAsync(_testUserFail2FA); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUserFail2FA)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            mgr.Setup(x => x.FindByIdAsync(_testUserFail2FAK.Id)).ReturnsAsync(_testUserFail2FAK); // allow search by username
            mgr.Setup(x => x.GetRolesAsync(_testUserFail2FAK)).ReturnsAsync(new List<string>() { "User" }); // allow search of roles

            mgr.Setup(x => x.ResetAuthenticatorKeyAsync(_testUser)).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.ResetAuthenticatorKeyAsync(_testUser2FA)).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.ResetAuthenticatorKeyAsync(_testUserFail2FAK)).ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code = "FailReset", Description = "FailReset" }));

            mgr.Setup(x => x.GetAuthenticatorKeyAsync(_testUser)).ReturnsAsync("ANewCode");

            mgr.Setup(x => x.VerifyTwoFactorTokenAsync(_testUser, "Authenticator", "999999")).ReturnsAsync(true);
            mgr.Setup(x => x.VerifyTwoFactorTokenAsync(_testUser, "Authenticator", "123456")).ReturnsAsync(false);
            mgr.Setup(x => x.VerifyTwoFactorTokenAsync(_testUserFail, "Authenticator", "999999")).ReturnsAsync(true);

            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUser, true)).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUser, false)).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUser2FA, false)).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUser2FA, true)).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUserFail, true)).ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code = "Fail2FA", Description = "Fail2FA" } ));
            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUserFail2FA, false)).ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code = "Fail2FA", Description = "Fail2FA" }));
            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUserFail2FAK, false)).ReturnsAsync(IdentityResult.Success);
            return mgr;
        }
        #endregion
    }
}
