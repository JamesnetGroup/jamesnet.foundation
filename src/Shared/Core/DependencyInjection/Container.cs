using System;
using System.Collections.Generic;
using System.Linq;

namespace Jamesnet.Foundation;

public class Container : IContainer
{
    private readonly Dictionary<(Type, string), Func<object>> _registrations = new Dictionary<(Type, string), Func<object>>();

    public void Register<TInterface, TImplementation>() where TImplementation : TInterface
    {
        Register<TInterface, TImplementation>(null);
    }

    public void Register<TInterface, TImplementation>(string name) where TImplementation : TInterface
    {
        _registrations[(typeof(TInterface), name)] = () => CreateInstance(typeof(TImplementation));
    }

    public void RegisterSingleton<TInterface, TImplementation>() where TImplementation : TInterface
    {
        RegisterSingleton<TInterface, TImplementation>(null);
    }

    public void RegisterSingleton<TInterface, TImplementation>(string name) where TImplementation : TInterface
    {
        var lazy = new Lazy<object>(() => CreateInstance(typeof(TImplementation)));
        _registrations[(typeof(TInterface), name)] = () => lazy.Value;
    }

    public void RegisterSingleton<TImplementation>(string name)
    {
        var lazy = new Lazy<object>(() => CreateInstance(typeof(TImplementation)));
        _registrations[(typeof(TImplementation), name)] = () => lazy.Value;
    }

    public void RegisterInstance<TInterface>(TInterface instance)
    {
        RegisterInstance(instance, null);
    }

    public void RegisterInstance<TInterface>(TInterface instance, string name)
    {
        _registrations[(typeof(TInterface), name)] = () => instance;
    }

    public T Resolve<T>()
    {
        return Resolve<T>(null);
    }

    public T Resolve<T>(string name)
    {
        return (T)Resolve(typeof(T), name);
    }

    public object Resolve(Type type)
    {
        return Resolve(type, null);
    }

    public object Resolve(Type type, string name)
    {
        if (_registrations.TryGetValue((type, name), out var creator))
        {
            return creator();
        }
        if (!type.IsAbstract && !type.IsInterface)
        {
            return CreateInstance(type);
        }
        throw new InvalidOperationException($"Type {type} has not been registered.");
    }

    public bool TryResolve<T>(out T result)
    {
        return TryResolve<T>(null, out result);
    }

    public bool TryResolve<T>(string name, out T result)
    {
        if (_registrations.TryGetValue((typeof(T), name), out var creator))
        {
            result = (T)creator();
            return true;
        }
        if (!typeof(T).IsAbstract && !typeof(T).IsInterface)
        {
            result = (T)CreateInstance(typeof(T));
            return true;
        }
        result = default;
        return false;
    }

    // 지금 이 스레드에서 생성 중인 타입. 순환 의존성(A→B→A)을 스택 오버플로 대신 명확한 예외로 알린다.
    [ThreadStatic]
    private static HashSet<Type> _creating;

    private object CreateInstance(Type type)
    {
        // 의존성을 가장 많이 받는 public 생성자를 쓴다. (DefaultViewModelInitializer와 같은 규칙)
        var constructor = type.GetConstructors().OrderByDescending(c => c.GetParameters().Length).FirstOrDefault()
            ?? throw new InvalidOperationException($"Type {type} has no public constructor.");

        _creating ??= new HashSet<Type>();
        if (!_creating.Add(type))
        {
            throw new InvalidOperationException($"Circular dependency detected while creating {type}.");
        }

        object instance;
        try
        {
            instance = constructor.Invoke(constructor.GetParameters().Select(p => Resolve(p.ParameterType)).ToArray());
        }
        finally
        {
            _creating.Remove(type);
        }

        if (instance is IView view)
        {
            InitializeView(view, type);
        }

        return instance;
    }

    private void InitializeView(IView view, Type type)
    {
        Resolve<IViewModelInitializer>().InitializeViewModel(view);

        var loadedEvent = type.GetEvent("Loaded");
        if (loadedEvent == null || view.DataContext == null)
        {
            return;
        }

        bool isFirst = true;
        Action<object, object> onLoaded = (_, _) =>
        {
            if (isFirst && view.DataContext is IViewFirstLoadable firstLoadable)
            {
                firstLoadable.OnFirstLoad(view);
            }
            isFirst = false;

            (view.DataContext as IViewLoadable)?.OnLoaded(view);
        };
        loadedEvent.AddEventHandler(view, Delegate.CreateDelegate(loadedEvent.EventHandlerType, onLoaded.Target, onLoaded.Method));
    }
}
