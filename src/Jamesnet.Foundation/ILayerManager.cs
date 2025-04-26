namespace Jamesnet.Foundation;

public interface ILayerManager
{
    void Register(string layerName, ILayer layer);
    void Mapping(string layerName, IView view);   
    void Add(string layerName, IView view);       
    void Show(string layerName, IView view);      
    void Hide(string layerName);                  
    ILayer GetLayer(string layerName);            
}