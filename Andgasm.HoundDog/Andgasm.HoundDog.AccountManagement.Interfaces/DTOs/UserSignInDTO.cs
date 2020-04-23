using FluentValidation;
using System;
using System.Collections.Generic;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public class UserSignInDTO
    {
        public string SuppliedUserName { get; set; }
        public string SuppliedPassword { get; set; }
        public string VerificationCode { get; set; }
    }

    public class UserSignInDTOValidator : AbstractValidator<UserSignInDTO>
    {
        public UserSignInDTOValidator()
        {
            RuleFor(x => x.SuppliedUserName).NotEmpty();
            RuleFor(x => x.SuppliedPassword).NotEmpty();
            
            RuleFor(x => x.SuppliedPassword).Matches(".*[A-Z].*").WithMessage("Password must contain at least one uppercase letter!");
            RuleFor(x => x.SuppliedPassword).Matches(".*[0-9].*").WithMessage("Password must contain at least one numerical digit!");
            RuleFor(x => x.SuppliedPassword).MinimumLength(6).WithMessage("Password must bet at least six characters long!");
            RuleFor(x => x.SuppliedPassword).Matches(".*[a-z].*").WithMessage("Password must contain at least one lowercase letter!");
        }
    }
}
