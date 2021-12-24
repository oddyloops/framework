using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Attributes
{
    /// <summary>
    /// Used for range validation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple =false, Inherited = true)]
    public class RangeAttribute : Attribute
    {
        public long Max { get; private set; }

        public long Min { get; private set; }
        public RangeAttribute(long min,long max)
        {
            Min = min;
            Max = max;
        }
    }
}
