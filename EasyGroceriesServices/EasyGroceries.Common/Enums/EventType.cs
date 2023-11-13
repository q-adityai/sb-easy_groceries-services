using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyGroceries.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum EventType
{
    [EnumMember(Value = "UserCreated")] UserCreated,

    [EnumMember(Value = "UserInactive")] UserInactive,

    [EnumMember(Value = "UserActive")] UserActive,

    [EnumMember(Value = "UserDeleted")] UserDeleted,

    [EnumMember(Value = "ProductCreated")] ProductCreated
}