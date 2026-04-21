namespace JustMeetinPoint.Maui.Features.Shared.Models
{
    public class CategoryModel
    {
        public CategoryModel(string name, string icon)
        {
            Name = name;
            Icon = icon;
        }
        public string Name { get; }
        public string Icon { get; }
    }
}