using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models.Song
{
    public class UpdateSongModel
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public string Lyric { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateSongModelValidator: AbstractValidator<UpdateSongModel>
    {
        public UpdateSongModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Music name is require");
            RuleFor(x => x.Name).MaximumLength(255).WithMessage("Music name max length is 255");

            RuleFor(x => x.Author).NotEmpty().WithMessage("Author is require");
            RuleFor(x => x.Author).MaximumLength(255).WithMessage("Author max length is 255");

            RuleFor(x => x.Url).NotEmpty().WithMessage("Url is require");

            RuleFor(x => x.Image).NotEmpty().WithMessage("Image max length is 255");
        }
    }
}
