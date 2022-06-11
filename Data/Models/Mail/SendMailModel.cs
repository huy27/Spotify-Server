using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models.Mail
{
    public class SendMailModel
    {
        public string ToEmail { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
    }

    public class SendMailModelValidator: AbstractValidator<SendMailModel>
    {
        public SendMailModelValidator()
        {
            RuleFor(x => x.ToEmail).EmailAddress().WithMessage("Email is invalid");

            RuleFor(x => x.Message).NotEmpty().WithMessage("Message is not null");

            RuleFor(x => x.Subject).NotEmpty().WithMessage("Subject is not null");
            RuleFor(x => x.Subject).MaximumLength(100).WithMessage("Subject max lenght is 100");
        }
    }
}
