namespace Jamesnet.Foundation;

public interface IResponsiveElement
{
    double Breakpoint { get; }
    void Update(double width);
}
