using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;

namespace Data.Models.Album
{
    public class CreateAlbumModel
    {
        public string Name { get; set; }
        public string BackgroundImageUrl { get; set; }
        public string Description { get; set; }
    }

    public class CreateAlbumModelValidator: AbstractValidator<CreateAlbumModel>
    {
        public CreateAlbumModelValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("Album name is require");
            RuleFor(x => x.Name).MaximumLength(255).WithMessage("Album name max lenght 255");

            RuleFor(x => x.BackgroundImageUrl).NotEmpty().WithMessage("Image is require");

            RuleFor(x => x.Description).MaximumLength(255).WithMessage("Description max lenght 255");
        }
    }
}
