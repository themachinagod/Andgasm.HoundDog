using FluentValidation;
using System;

namespace Andgasm.HoundDog.AccountManagement.Interfaces
{
    public class UserDTO
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Location { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string AccountType { get; set; }

        public string PasswordClear { get; set; }
        public string PasswordClearConfirm { get; set; }
        public string OldPasswordClear { get; set; }
        public string Token { get; set; }

        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }

        public bool HasChangePassword { get; set; }
        public bool HasChangeEmail { get; set; }
        public bool HasChangePhone { get; set; }

        public bool TwoFactorEnabled { get; set; }

        public string Roles { get; set; }
        public SimpleDate DoB { get; set; }
    }

    public class UserDTOValidator : AbstractValidator<UserDTO>
    {
        public UserDTOValidator()
        {
            RuleFor(x => x.FirstName).NotEmpty();
            RuleFor(x => x.LastName).NotEmpty();
            RuleFor(x => x.Email).NotEmpty();
            RuleFor(x => x.UserName).NotEmpty();
            RuleFor(x => x.DoB).NotEmpty();

            RuleFor(x => x.Email).EmailAddress();

            RuleFor(x => x.PhoneNumber).Matches(@"^\(?(?:(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?\(?(?:0\)?[\s-]?\(?)?|0)(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}|\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4}|\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3})|\d{5}\)?[\s-]?\d{4,5}|8(?:00[\s-]?11[\s-]?11|45[\s-]?46[\s-]?4\d))(?:(?:[\s-]?(?:x|ext\.?\s?|\#)\d+)?)$")
                .WithMessage("Phone number must be a valid E.164 formatted phone number... e.g. +447939948389");

            RuleFor(x => x.PasswordClear).Equal(x => x.PasswordClearConfirm).WithMessage("Password must match with specified password confirmation!");
            RuleFor(x => x.PasswordClearConfirm).Equal(x => x.PasswordClear).WithMessage("Password must match with specified password confirmation!");

            RuleFor(x => x.PasswordClear).Matches(".*[A-Z].*").WithMessage("Password must contain at least one uppercase letter!");
            RuleFor(x => x.PasswordClear).Matches(".*[a-z].*").WithMessage("Password must contain at least one lowercase letter!");
            RuleFor(x => x.PasswordClear).Matches(".*[0-9].*").WithMessage("Password must contain at least one numerical digit!");
            RuleFor(x => x.PasswordClear).MinimumLength(6).WithMessage("Password must bet at least six characters long!");

            RuleFor(x => x.UserName).MinimumLength(6).WithMessage("Username must bet at least six characters long!");
        }
    }
}
