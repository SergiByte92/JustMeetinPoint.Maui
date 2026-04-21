using Microcharts;
using SkiaSharp;

namespace JustMeetinPoint.Maui.Features.Home.Factories;

public static class UserStatsChartFactory
{
    public static Chart CreatePlaceholder()
    {
        var entries = new[]
        {
            new ChartEntry(12)
            {
                Label = "Ene",
                ValueLabel = "12",
                Color = SKColor.Parse("#1A73E8")
            },
            new ChartEntry(18)
            {
                Label = "Feb",
                ValueLabel = "18",
                Color = SKColor.Parse("#34A853")
            },
            new ChartEntry(10)
            {
                Label = "Mar",
                ValueLabel = "10",
                Color = SKColor.Parse("#FBBC04")
            },
            new ChartEntry(21)
            {
                Label = "Abr",
                ValueLabel = "21",
                Color = SKColor.Parse("#D94FA8")
            }
        };

        return new BarChart
        {
            Entries = entries,
            LabelTextSize = 28,
            ValueLabelOrientation = Orientation.Horizontal,
            LabelOrientation = Orientation.Horizontal,
            Margin = 16
        };
    }
}