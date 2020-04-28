using Andgasm.HoundDog.AccountManagement.Core;
using Andgasm.HoundDog.AccountManagement.Core.AuthManagement;
using Andgasm.HoundDog.AccountManagement.Database;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Andgasm.HoundDog.Core.Email.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
            ProfileAvatar = new byte[666]
        };
        HoundDogUser _testUserFail = new HoundDogUser()
        {
            Id = "bb10153d-b51e-40de-a79c-1b933d78f420",
            UserName = "TestUser",
            Email = "TestUser@TestEmail.com",
            ProfileAvatar = new byte[666]
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
        #endregion

        // TODO: Disable2FA tests
        //       failed enable test
        //       failed disable test
        //       already 2fa enabled test
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

            mgr.Setup(x => x.ResetAuthenticatorKeyAsync(_testUser)).ReturnsAsync(IdentityResult.Success); 
            mgr.Setup(x => x.GetAuthenticatorKeyAsync(_testUser)).ReturnsAsync("ANewCode");

            mgr.Setup(x => x.VerifyTwoFactorTokenAsync(_testUser, "Authenticator", "999999")).ReturnsAsync(true);
            mgr.Setup(x => x.VerifyTwoFactorTokenAsync(_testUser, "Authenticator", "123456")).ReturnsAsync(false);

            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUser, true)).ReturnsAsync(IdentityResult.Success);
            mgr.Setup(x => x.SetTwoFactorEnabledAsync(_testUserFail, true)).ReturnsAsync(IdentityResult.Failed());
            return mgr;
        }
        #endregion
    }
}
