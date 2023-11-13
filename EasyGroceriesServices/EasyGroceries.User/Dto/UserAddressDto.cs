using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Dto;

namespace EasyGroceries.User.Dto;

public class UserAddressDto
{
    [Required] public AddressDto Address { get; set; } = null!;

    public bool IsDefault { get; set; }
}