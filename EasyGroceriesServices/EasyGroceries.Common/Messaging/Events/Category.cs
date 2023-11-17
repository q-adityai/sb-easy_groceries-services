namespace EasyGroceries.Common.Messaging.Events;

public class Category
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsActive { get; set; }
    public bool IncludeInDelivery { get; set; }
}