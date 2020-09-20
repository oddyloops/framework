using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Attributes
{
    /// <summary>
    /// Used for required fields
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]

    public class RequiredAttribute : Attribute
    {
       
    }
}
