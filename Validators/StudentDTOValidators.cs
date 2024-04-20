using System.Text.RegularExpressions;
using FluentValidation;
using SqlClientExample_Tasks.DTOs;

namespace SqlClientExample_Tasks.Validators;

public class StudentWithGroupNameDTOValidator : AbstractValidator<StudentWithGroupsIdsDTO>
{
    public StudentWithGroupNameDTOValidator()
    {
        RuleFor(e => e.FirstName).MaximumLength(50).NotNull().NotEmpty();
        RuleFor(e => e.LastName).MaximumLength(50).NotNull().NotEmpty();
        RuleFor(e => e.Phone).Must(e => Regex.IsMatch(e, "[0-9]{9}")).Length(9).NotNull();
        RuleFor(e => e.Birthdate).NotNull();
        RuleFor(e => e.GroupsIds).NotNull();
    }
}