using System.Collections.Generic;

namespace Jamesnet.Foundation;

public class ResponsiveManager
{
    private readonly IResponsiveHost _host;
    private readonly Dictionary<object, List<IResponsiveElement>> _elements = new();

    public ResponsiveManager(IResponsiveHost host)
    {
        _host = host;
    }

    public void Register(object element, IResponsiveElement responsive)
    {
        if (!_elements.ContainsKey(element))
        {
            _elements[element] = new List<IResponsiveElement>();
            _host.Subscribe(element, () => UpdateAll(element));
        }

        if (!_elements[element].Contains(responsive))
            _elements[element].Add(responsive);

        double width = _host.GetWidth(element);
        responsive.Update(width);
    }

    public void Unregister(object element)
    {
        if (_elements.Remove(element))
            _host.Unsubscribe(element);
    }

    private void UpdateAll(object element)
    {
        if (!_elements.TryGetValue(element, out var list))
            return;

        double width = _host.GetWidth(element);
        foreach (var responsive in list)
            responsive.Update(width);
    }
}
