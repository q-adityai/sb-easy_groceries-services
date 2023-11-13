using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace EasyGroceries.User.Dto;

public class UserDto
{
    public string? Id { get; set; }

    [Required] [StringLength(50)] public string FirstName { get; set; } = null!;

    [Required] [StringLength(50)] public string LastName { get; set; } = null!;

    [Required] [EmailAddress] public string Email { get; set; } = null!;

    [Phone] public string? PhoneNumber { get; set; }

    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }

    public List<UserAddressDto> BillingAddresses { get; set; } = new();
    public List<UserAddressDto> DeliveryAddresses { get; set; } = new();
}