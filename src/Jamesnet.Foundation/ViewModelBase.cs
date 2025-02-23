using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Jamesnet.Foundation
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]  // AllowMultiple = true 추가
    public class CommandTriggerAttribute : Attribute
    {
        public string CommandName { get; }
        public CommandTriggerAttribute(string commandName)
        {
            CommandName = commandName;
        }
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            HandleCommandTrigger(propertyName);
        }

        protected bool SetProperty<T>(ref T storage, T value, Action? callback = null, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);
            callback?.Invoke();
            return true;
        }

        private void HandleCommandTrigger(string propertyName)
        {
            var property = GetType().GetProperty(propertyName);
            if (property == null)
                return;

            // GetCustomAttributes로 모든 CommandTriggerAttribute를 가져옴
            var attributes = property.GetCustomAttributes<CommandTriggerAttribute>();
            if (!attributes.Any())
                return;

            // 각 attribute에 대해 Command를 찾아서 RaiseCanExecuteChanged 호출
            foreach (var attribute in attributes)
            {
                var commandProperty = GetType().GetProperty(attribute.CommandName);
                if (commandProperty == null)
                    continue;

                var command = commandProperty.GetValue(this) as RelayCommand;
                command?.RaiseCanExecuteChanged();
            }
        }
    }
}