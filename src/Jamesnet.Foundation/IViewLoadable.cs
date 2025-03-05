namespace Jamesnet.Foundation
{
    public interface IViewFirstLoadable
    {
        void OnFirstLoad(object view);
    }

    public interface IViewLoadable
    {
        void OnLoaded(object view);
    }

    public interface IViewActivated
    {
        void ViewActivated(object view);
    }

    public interface IViewClosed
    {
        void OnClosed(object view);
    }
}
