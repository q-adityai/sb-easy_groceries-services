namespace EasyGroceries.Common.Configuration;

public class MessagingOptions
{
    public static readonly string SectionName = "Messaging";
    public string ConnectionString { get; set; } = null!;
}