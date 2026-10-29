using System;
using System.Windows.Input;

namespace Jamesnet.Foundation;

/// <summary>[CommandTrigger]가 CanExecute 재평가를 요청할 수 있는 명령.</summary>
public interface IRelayCommand : ICommand
{
    void RaiseCanExecuteChanged();
}

public class RelayCommand<T> : IRelayCommand
{
    private readonly Action<T> _execute;
    private readonly Func<T, bool> _canExecute;

    public RelayCommand(Action<T> execute, Func<T, bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute(Cast(parameter));
    }

    public void Execute(object parameter)
    {
        _execute(Cast(parameter));
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }

    // 값 타입 T에 null이 들어와도(바인딩 초기 CanExecute(null) 등) 예외 없이 default(T)로 처리한다.
    private static T Cast(object parameter) => parameter is T value ? value : default;
}

public class RelayCommand : IRelayCommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute ?? throw new ArgumentNullException(nameof(execute));
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return _canExecute == null || _canExecute();
    }

    public void Execute(object parameter)
    {
        _execute();
    }

    public void RaiseCanExecuteChanged()
    {
        CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}