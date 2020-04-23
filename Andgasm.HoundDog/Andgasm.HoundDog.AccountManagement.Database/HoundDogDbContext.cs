using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;

namespace Andgasm.HoundDog.AccountManagement.Database
{
    public class HoundDogDbContext : IdentityDbContext<HoundDogUser>
    {
        public HoundDogDbContext(DbContextOptions<HoundDogDbContext> options) : base(options)
        { 
        }
    }
}
