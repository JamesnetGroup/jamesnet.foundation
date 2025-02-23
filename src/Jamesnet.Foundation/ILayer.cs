namespace Jamesnet.Foundation;

public interface ILayer
{
    object UIContent { get; set; }
    string LayerName { get; set; }
    bool IsRegistered { get; set; }
}
