using FluentValidation;
using System;
using System.Collections.Generic;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public class UserPasswordChangeDTO
    {
        public Guid UserId { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm { get; set; }
        public string OldPassword { get; set; }
        public string ResetToken { get; set; }
        public string VerificationCode { get; set; }
    }

    public class UserPasswordChangeDTOValidator : AbstractValidator<UserPasswordChangeDTO>
    {
        public UserPasswordChangeDTOValidator()
        {
            RuleFor(x => x.Password).Equal(x => x.PasswordConfirm).WithMessage("Password must match with specified password confirmation!");
            RuleFor(x => x.PasswordConfirm).Equal(x => x.Password).WithMessage("Password must match with specified password confirmation!");

            RuleFor(x => x.Password).Matches(".*[A-Z].*").WithMessage("Password must contain at least one uppercase letter!");
            RuleFor(x => x.Password).Matches(".*[a-z].*").WithMessage("Password must contain at least one lowercase letter!");
            RuleFor(x => x.Password).Matches(".*[0-9].*").WithMessage("Password must contain at least one numerical digit!");
            RuleFor(x => x.Password).MinimumLength(6).WithMessage("Password must bet at least six characters long!");
        }
    }
}
