using System;

namespace Framework.Attributes
{
    /// <summary>
    /// An attribute used to identify the primary field of a record type
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class PrimaryKeyAttribute : Attribute
    {
    }
}
