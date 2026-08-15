namespace Jamesnet.Foundation;

public interface ILayer
{
    string LayerName { get; set; }    
    bool IsRegistered { get; set; }   
    object UIContent { get; set; }    
}