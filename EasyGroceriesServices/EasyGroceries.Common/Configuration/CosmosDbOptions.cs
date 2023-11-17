namespace EasyGroceries.Common.Configuration;

public class CosmosDbOptions
{
    public static readonly string SectionName = "CosmosDb";
    public string Uri { get; set; } = null!;
    public string Key { get; set; } = null!;
    public string DatabaseName { get; set; } = null!;
}