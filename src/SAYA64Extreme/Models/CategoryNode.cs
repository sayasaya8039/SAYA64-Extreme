using System.Collections.ObjectModel;

namespace SAYA64Extreme.Models;

public class CategoryNode
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = "📁";
    public ObservableCollection<CategoryNode> Children { get; set; } = new();
}
