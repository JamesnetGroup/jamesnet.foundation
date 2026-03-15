using System;

namespace Jamesnet.Foundation;

public interface IResponsiveHost
{
    double GetWidth(object element);
    void Subscribe(object element, Action callback);
    void Unsubscribe(object element);
}
