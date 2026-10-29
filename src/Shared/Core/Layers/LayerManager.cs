using System;
using System.Collections.Generic;

namespace Jamesnet.Foundation;

public class LayerManager : ILayerManager
{
    private readonly Dictionary<string, ILayer> _layers = new();
    private readonly Dictionary<string, IView> _layerViewMappings = new();

    /// <summary>
    /// 레이어를 이름으로 등록한다. 같은 이름으로 다시 등록하면(창을 새로 열어 레이어가 다시 만들어진 경우 등)
    /// 새 레이어로 교체하고 매핑된 뷰를 다시 보여준다. 옛 레이어는 더 이상 붙잡지 않는다.
    /// </summary>
    public void Register(string layerName, ILayer layer)
    {
        if (_layers.TryGetValue(layerName, out var existing) && ReferenceEquals(existing, layer))
        {
            return;
        }

        _layers[layerName] = layer;
        if (_layerViewMappings.TryGetValue(layerName, out var view))
        {
            Show(layerName, view);
        }
    }

    public void Show(string layerName, IView view)
    {
        var layer = GetRequiredLayer(layerName);

        if (view == null)
        {
            layer.UIContent = null;
            return;
        }

        if (Equals(layer.UIContent, view))
        {
            (view.DataContext as IViewLoadable)?.OnLoaded(view);
            return;
        }

        if (layer.UIContent is IView closing && closing.DataContext is IViewClosed viewClosed)
        {
            viewClosed.OnClosed(closing);
        }

        layer.UIContent = view;
        (view.DataContext as IViewActivated)?.ViewActivated(view);
    }

    /// <summary>레이어가 등록되어 있는지만 확인한다. (뷰 목록을 따로 보관하지 않는다)</summary>
    public void Add(string layerName, IView view) => GetRequiredLayer(layerName);

    public void Hide(string layerName)
    {
        if (_layers.TryGetValue(layerName, out var layer))
        {
            layer.UIContent = null;
        }
    }

    public void Mapping(string layerName, IView view) => _layerViewMappings[layerName] = view;

    public ILayer GetLayer(string layerName) => _layers.TryGetValue(layerName, out var layer) ? layer : null;

    private ILayer GetRequiredLayer(string layerName) =>
        GetLayer(layerName) ?? throw new InvalidOperationException($"Layer not registered: {layerName}");

    /// <summary>레이어가 Loaded될 때 LayerManager에 등록되도록 한다. Loaded 이벤트가 없는 타입은 즉시 등록한다.</summary>
    public static void InitializeLayer(ILayer layer)
    {
        if (layer == null)
        {
            throw new ArgumentNullException(nameof(layer));
        }

        var loadedEvent = layer.GetType().GetEvent("Loaded");
        if (loadedEvent == null)
        {
            RegisterToLayerManager(layer);
            return;
        }

        Delegate handler = null;
        Action<object, object> onLoaded = (_, _) =>
        {
            loadedEvent.RemoveEventHandler(layer, handler);
            RegisterToLayerManager(layer);
        };
        handler = Delegate.CreateDelegate(loadedEvent.EventHandlerType, onLoaded.Target, onLoaded.Method);
        loadedEvent.AddEventHandler(layer, handler);
    }

    public static void RegisterToLayerManager(ILayer layer)
    {
        if (string.IsNullOrEmpty(layer.LayerName) || layer.IsRegistered)
        {
            return;
        }

        var layerManager = ContainerProvider.GetContainer().Resolve<ILayerManager>();
        if (layerManager != null)
        {
            layerManager.Register(layer.LayerName, layer);
            layer.IsRegistered = true;
        }
    }
}
