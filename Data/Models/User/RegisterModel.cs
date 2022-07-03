using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models.User
{
    public class RegisterModel
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
    }

    public class RegisterModelValidator : AbstractValidator<RegisterModel>
    {
        public RegisterModelValidator()
        {
            RuleFor(x => x.UserName).NotEmpty().WithMessage("UserName is require");
            RuleFor(x => x.UserName).MinimumLength(3).WithMessage("UserName min length is 3");

            RuleFor(x => x.Password).NotEmpty().WithMessage("Password is require");
            RuleFor(x => x.Password).MinimumLength(3).WithMessage("Password min length is 3");

            RuleFor(x => x.Email).NotEmpty().WithMessage("Email is require");
            RuleFor(x => x.Email).EmailAddress().WithMessage("Email is invalid");
        }
    }
}
