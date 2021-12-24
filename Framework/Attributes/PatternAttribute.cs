using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Attributes
{
    /// <summary>
    /// Used for regex validation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]

    public class PatternAttribute : Attribute
    {
        public const string GUID_PATTERN = @"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}";
        public string Pattern { get; private set; }

        public PatternAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }
}
