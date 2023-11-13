using System;
using EasyGroceries.Common.Entities;

namespace EasyGroceries.User.Model.Entities;

public class UserAddress
{
    public Address Address { get; set; } = null!;
    public bool IsDefault { get; set; }

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? LastModifiedAt { get; set; }
}