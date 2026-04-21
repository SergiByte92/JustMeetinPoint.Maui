namespace JustMeetinPoint.Maui.Features.Home.Models
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