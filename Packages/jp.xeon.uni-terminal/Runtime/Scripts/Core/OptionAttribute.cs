using System;

namespace Company.Terminal.Runtime
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class OptionAttribute : Attribute
    {
        public OptionAttribute(string longName, string shortName = "", bool isRequire = false)
        {
            LongName = longName;
            ShortName = shortName;
            IsRequire = isRequire;
        }

        public string LongName { get; }
        public string ShortName { get; }
        public bool IsRequire { get; }
    }
}
