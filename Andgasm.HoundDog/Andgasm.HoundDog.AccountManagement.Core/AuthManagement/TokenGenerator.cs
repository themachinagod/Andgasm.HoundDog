﻿using Andgasm.HoundDog.AccountManagement.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Andgasm.HoundDog.AccountManagement.Core
{
    public class TokenGenerator : ITokenGenerator
    {
        private readonly IConfiguration _config;

        public TokenGenerator(IConfiguration config)
        {
            _config = config;
        }

        public string GenerateToken(UserDTO authenticateduser)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config.GetSection(ITokenGenerator.TokenConfigName).Value);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.NameIdentifier, authenticateduser.Id.ToString()),
                    new Claim(ClaimTypes.Name, authenticateduser.UserName),
                    new Claim(ClaimTypes.Role, string.Join(",",  authenticateduser.Roles))
                }),
                Expires = DateTime.Now.AddHours(Convert.ToDouble(_config.GetSection(ITokenGenerator.TokenExpiryConfigName).Value)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
