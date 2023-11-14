using System;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace EasyGroceries.Common.Attributes;

public class RequiredEnumAttribute : RequiredAttribute
{
    public override bool IsValid(object? value)
    {
        if (value == null) return false;
        var type = value.GetType();
        return type.IsEnum && Enum.IsDefined(type, value);
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null)
            return new ValidationResult(
                $"The {validationContext.MemberName ?? validationContext.DisplayName} field is required",
                new[] { validationContext.MemberName ?? validationContext.DisplayName });
        var type = value.GetType();
        if (!type.IsEnum || !Enum.IsDefined(type, value))
        {
            var validValues = new StringBuilder();
            var enumerator = Enum.GetValues(type).GetEnumerator();
            while (enumerator.MoveNext()) validValues = validValues.Append($"{enumerator.Current} ");


            var errorMessage = $"Invalid enum value supplied. Valid values are: {validValues.ToString().Trim()}";
            return new ValidationResult(errorMessage,
                new[] { validationContext.MemberName ?? validationContext.DisplayName });
        }

        return ValidationResult.Success;
    }
}