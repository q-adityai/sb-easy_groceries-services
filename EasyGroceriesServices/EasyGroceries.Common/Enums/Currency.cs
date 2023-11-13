using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyGroceries.Common.Enums;
[JsonConverter(typeof(StringEnumConverter))]
public enum Currency
{
    [EnumMember(Value = "UNKNOWN")]
    Unknown,
    
    [EnumMember(Value = "GBP")]
    Gbp,
    
    [EnumMember(Value = "USD")]
    Usd
}