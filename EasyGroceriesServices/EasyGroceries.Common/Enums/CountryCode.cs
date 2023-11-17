using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyGroceries.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum CountryCode
{
    [EnumMember(Value = "United Kingdom")] Gb
}