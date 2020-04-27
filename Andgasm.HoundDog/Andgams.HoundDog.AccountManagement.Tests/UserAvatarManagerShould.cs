using Andgasm.HoundDog.AccountManagement.Core;
using Andgasm.HoundDog.AccountManagement.Core.AuthManagement;
using Andgasm.HoundDog.AccountManagement.Database;
using Andgasm.HoundDog.AccountManagement.Interfaces;
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
    public class UserAvatarManagerShould
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

        #region Get Avatar Tests
        [Fact]
        public async Task GetAvatarData_WhenValidUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAvatarManager(new NullLogger<UserAvatarManager>(), usrmgr.Object);
            var avatar = await am.GetAvatar(_testUser.Id);
            Assert.NotNull(avatar.ImageData);
            Assert.Equal(666, avatar.ImageData.Length); 
        }

        [Fact]
        public async Task UploadAvatarData_WhenValidUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAvatarManager(new NullLogger<UserAvatarManager>(), usrmgr.Object);
            var avatar = await am.UploadAvatar(_testUser.Id, new byte[999]);
            Assert.True(avatar.Succeeded);
            usrmgr.Verify(x => x.UpdateAsync(It.Is<HoundDogUser>(y => y.ProfileAvatar.Length == 999)));
        }

        [Fact]
        public async Task ReportGetError_WhenNullUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAvatarManager(new NullLogger<UserAvatarManager>(), usrmgr.Object);
            var avatar = await am.GetAvatar(null);
            Assert.Null(avatar.ImageData);
            Assert.Equal("You must provide a user id to get an avatar!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportGetError_WhenNonExistingUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAvatarManager(new NullLogger<UserAvatarManager>(), usrmgr.Object);
            var avatar = await am.GetAvatar(Guid.NewGuid().ToString());
            Assert.Null(avatar.ImageData);
            Assert.Equal("Specified user does not exist!", avatar.Errors.First().Description);
        }
        #endregion

        #region Upload Avatar Tests
        [Fact]
        public async Task ReportUploadError_WhenNullUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAvatarManager(new NullLogger<UserAvatarManager>(), usrmgr.Object);
            var avatar = await am.UploadAvatar(null, new byte[999]);
            Assert.False(avatar.Succeeded);
            Assert.Equal("You must provide a user and image to upload an avatar!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportUploadError_WhenNonExistingUserIdRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAvatarManager(new NullLogger<UserAvatarManager>(), usrmgr.Object);
            var avatar = await am.UploadAvatar(Guid.NewGuid().ToString(), new byte[999]);
            Assert.False(avatar.Succeeded);
            Assert.Equal("Specified user does not exist!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportUploadError_WhenNullPayloadRecieved()
        {
            var usrmgr = MockUserManager();
            var am = new UserAvatarManager(new NullLogger<UserAvatarManager>(), usrmgr.Object);
            var avatar = await am.UploadAvatar(_testUser.Id, null);
            Assert.False(avatar.Succeeded);
            Assert.Equal("You must provide a user and image to upload an avatar!", avatar.Errors.First().Description);
        }

        [Fact]
        public async Task ReportUploadError_WhenUpdateFails()
        {
            var usrmgr = MockUserManager();
            var am = new UserAvatarManager(new NullLogger<UserAvatarManager>(), usrmgr.Object);
            var avatar = await am.UploadAvatar(_testUserFail.Id, new byte[999]);
            Assert.False(avatar.Succeeded);
            Assert.Equal("Fail", avatar.Errors.First().Description);
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

            mgr.Setup(x => x.UpdateAsync(_testUser)).ReturnsAsync(IdentityResult.Success); // allow search by username
            mgr.Setup(x => x.UpdateAsync(_testUserFail)).ReturnsAsync(IdentityResult.Failed(new IdentityError() { Code = "Fail", Description = "Fail" })); // allow search by username

            return mgr;
        }
        #endregion
    }
}
