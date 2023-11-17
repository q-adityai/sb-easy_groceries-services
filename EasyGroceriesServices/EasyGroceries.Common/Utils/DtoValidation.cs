using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EasyGroceries.Common.Dto;
using Microsoft.AspNetCore.Mvc;

namespace EasyGroceries.Common.Utils;

public static class DtoValidation
{
    public static bool IsValid<T>(T? dto, out IActionResult result)
    {
        result = null!;

        if (dto == null)
        {
            result = new BadRequestObjectResult(StandardResponse.Failure("Request cannot be null"));
            return false;
        }

        var validationContext = new ValidationContext(dto);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(dto, validationContext, validationResults, true);
        if (isValid) return true;
        result = new BadRequestObjectResult(StandardResponse.Failure(validationResults
            .Select(v => $"{string.Join(", ", v.MemberNames)}: {v.ErrorMessage}").ToArray()));
        return false;
    }
}