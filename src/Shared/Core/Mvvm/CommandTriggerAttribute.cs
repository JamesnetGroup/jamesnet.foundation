using System;

namespace Jamesnet.Foundation;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class CommandTriggerAttribute : Attribute
{
    public string CommandName { get; }
    public CommandTriggerAttribute(string commandName)
    {
        CommandName = commandName;
    }
}
