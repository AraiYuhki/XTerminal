using System;

namespace Company.Terminal.Runtime
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class CommandAttribute : Attribute
    {
        public CommandAttribute(string commandName, string description)
        {
            CommandName = commandName;
            Description = description;
        }

        public string CommandName { get; }
        public string Description { get; }
    }
}
