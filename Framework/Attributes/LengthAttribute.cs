using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Attributes
{
    /// <summary>
    /// Used for length validation
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple =false, Inherited = true)]
    public class LengthAttribute : Attribute
    {
        public int MaxLength { get; private set; }

        public int MinLength { get; private set; }
        public LengthAttribute(int maxLength)
        {
            MaxLength = maxLength;
        }

        /// <summary>
        /// Use 0 for max length if unlimited
        /// </summary>
        /// <param name="minLength"></param>
        /// <param name="maxLength"></param>
        public LengthAttribute(int minLength, int maxLength)
        {
            MinLength = minLength;
            MaxLength = maxLength;
        }
    }
}
