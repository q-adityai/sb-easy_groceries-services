using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyGroceries.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum BasketStatus
{
    [EnumMember(Value = "Empty")]
    Empty = 1,
    
    [EnumMember(Value = "Active")]
    Active = 2,
    
    [EnumMember(Value = "Suspended")]
    Suspended = 3,
    
    [EnumMember(Value = "CheckedOut")]
    CheckedOut = 4,
    
    [EnumMember(Value = "Locked")]
    Locked = 5,
    
    [EnumMember(Value = "Closed")]
    Closed = 6,
    
    [EnumMember(Value = "Deleted")]
    Deleted = 7
}