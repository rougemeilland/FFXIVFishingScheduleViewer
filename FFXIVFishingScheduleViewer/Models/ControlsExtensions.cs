using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace FFXIVFishingScheduleViewer.Models
{
    static class ControlsExtensions
    {
        public static Panel AddToChildren(this Panel panel, IEnumerable<UIElement> source)
        {
            foreach (var child in source)
                panel.Children.Add(child);
            return panel;
        }
    }
}
