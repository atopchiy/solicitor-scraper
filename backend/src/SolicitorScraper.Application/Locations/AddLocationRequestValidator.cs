using System.Text.RegularExpressions;
using FluentValidation;

namespace SolicitorScraper.Application.Locations;

public partial class AddLocationRequestValidator : AbstractValidator<AddLocationRequest>
{
    public AddLocationRequestValidator()
    {
        RuleFor(r => r.Name)
            .NotEmpty().WithMessage("Location name is required.")
            .MaximumLength(100).WithMessage("Location name must be 100 characters or fewer.")
            .Must(BeAValidName).WithMessage(
                "Location name can only contain letters, spaces, hyphens and apostrophes.");
    }

    private static bool BeAValidName(string? name) =>
        string.IsNullOrWhiteSpace(name) || NamePattern().IsMatch(name.Trim());

    [GeneratedRegex(@"^[\p{L}][\p{L} \-']*$")]
    private static partial Regex NamePattern();
}
