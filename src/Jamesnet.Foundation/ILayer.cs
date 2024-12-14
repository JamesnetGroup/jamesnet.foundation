using System.Windows;

namespace Jamesnet.Foundation;

public interface ILayer
{
    UIElement Content { get; set; }
    string LayerName { get; set; }
    bool IsRegistered { get; set; }
}
