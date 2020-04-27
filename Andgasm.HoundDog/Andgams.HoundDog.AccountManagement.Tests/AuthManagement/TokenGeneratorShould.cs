using Andgasm.HoundDog.AccountManagement.Core.AuthManagement;
using Andgasm.HoundDog.AccountManagement.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using Xunit;

namespace Andgams.HoundDog.AccountManagement.Tests
{
    public class TokenGeneratorShould
    {
        [Fact]
        public void GenerateValidToken_WhenValidUserRecieved()
        {
            var _config =  new Mock<IConfiguration>();
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenConfigName).Value).Returns("Fu5pvu7yW3uMRVRwXRo40l30mVsWC4tj");
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenExpiryConfigName).Value).Returns("1");
            
            TokenGenerator tg = new TokenGenerator(_config.Object);
            var usrid = Guid.NewGuid();
            var user = new UserDTO()
            {
                Id = usrid,
                UserName = "TestUser",
                Roles = "user"
            };
            var tokenresult = tg.GenerateToken(user);
            Assert.Equal("eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9", tokenresult.Split('.')[0]); 
        }

        [Fact]
        
        public void ThrowException_WhenNoUserRecieved()
        {
            var _config = new Mock<IConfiguration>();
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenConfigName).Value).Returns("Fu5pvu7yW3uMRVRwXRo40l30mVsWC4tj");
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenExpiryConfigName).Value).Returns("1");

            TokenGenerator tg = new TokenGenerator(_config.Object);
            Exception ex = Assert.Throws<Exception>(() => tg.GenerateToken(null));
            Assert.Equal("User data must be supplied to generate token!", ex.Message);
        }

        [Fact]

        public void ThrowException_WhenNoUserNameRecieved()
        {
            var _config = new Mock<IConfiguration>();
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenConfigName).Value).Returns("Fu5pvu7yW3uMRVRwXRo40l30mVsWC4tj");
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenExpiryConfigName).Value).Returns("1");

            TokenGenerator tg = new TokenGenerator(_config.Object);
            var usrid = Guid.NewGuid();
            var user = new UserDTO()
            {
                Id = usrid,
                UserName = null,
                Roles = "user"
            };
            Exception ex = Assert.Throws<Exception>(() => tg.GenerateToken(null));
            Assert.Equal("User data must be supplied to generate token!", ex.Message);
        }

        [Fact]

        public void ThrowException_WhenNoUserIdRecieved()
        {
            var _config = new Mock<IConfiguration>();
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenConfigName).Value).Returns("Fu5pvu7yW3uMRVRwXRo40l30mVsWC4tj");
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenExpiryConfigName).Value).Returns("1");

            TokenGenerator tg = new TokenGenerator(_config.Object);
            var usrid = Guid.NewGuid();
            var user = new UserDTO()
            {
                Id = Guid.Empty,
                UserName = "TestUser",
                Roles = "user"
            };
            Exception ex = Assert.Throws<Exception>(() => tg.GenerateToken(null));
            Assert.Equal("User data must be supplied to generate token!", ex.Message);
        }

        [Fact]

        public void ThrowException_WhenNoUserRolesRecieved()
        {
            var _config = new Mock<IConfiguration>();
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenConfigName).Value).Returns("Fu5pvu7yW3uMRVRwXRo40l30mVsWC4tj");
            _config.Setup(x => x.GetSection(ITokenGenerator.TokenExpiryConfigName).Value).Returns("1");

            TokenGenerator tg = new TokenGenerator(_config.Object);
            var usrid = Guid.NewGuid();
            var user = new UserDTO()
            {
                Id = Guid.Empty,
                UserName = "TestUser",
                Roles = null
            };
            Exception ex = Assert.Throws<Exception>(() => tg.GenerateToken(null));
            Assert.Equal("User data must be supplied to generate token!", ex.Message);
        }
    }
}
