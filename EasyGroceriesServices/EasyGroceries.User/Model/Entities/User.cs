using System;
using System.Collections.Generic;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.User.Model.Entities;

public class User
{
    public string Id { get; set; } = $"{Constants.UserPrefix}{Guid.NewGuid()}";
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public List<UserAddress> BillingAddresses { get; set; } = null!;
    public List<UserAddress> DeliveryAddresses { get; set; } = null!;
    public bool IsAdmin { get; set; }
    public bool IsActive { get; set; }
}