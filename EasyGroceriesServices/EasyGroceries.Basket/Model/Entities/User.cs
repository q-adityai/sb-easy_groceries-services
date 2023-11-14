using System.ComponentModel.DataAnnotations;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.Basket.Model.Entities;

public class User
{
    [Required] public string Id { get; set; } = null!;

    [Required] public string FirstName { get; set; } = null!;

    [Required] public string LastName { get; set; } = null!;

    [Required] public string Email { get; set; } = null!;

    public string? PhoneNumber { get; set; }
    public DefaultAddress? DefaultBillingAddress { get; set; }
    public DefaultAddress? DefaultDeliveryAddress { get; set; }
}