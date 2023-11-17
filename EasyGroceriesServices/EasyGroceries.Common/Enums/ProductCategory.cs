using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EasyGroceries.Common.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum ProductCategory
{
    [EnumMember(Value = "Dairy")] Dairy = 1,

    [EnumMember(Value = "Breads")] Breads = 2,

    [EnumMember(Value = "Cereals")] Cereals = 3,

    [EnumMember(Value = "Grains")] Grains = 4,

    [EnumMember(Value = "PromotionCoupon")]
    PromotionCoupon = 5
}