using System;


namespace Framework.Attributes
{
    /// <summary>
    /// An attribute for specifying a foreign key field
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple =false, Inherited = true)]
    public class ForeignKeyAttribute : Attribute
    {
        /// <summary>
        /// Constructor for specifying the parent record map name
        /// </summary>
        /// <param name="parentName">Parent record map name</param>
        public ForeignKeyAttribute(string parentMapName)
        {
            ParentMapName = parentMapName;
        }

        /// <summary>
        /// Parent record map name
        /// </summary>
        public string ParentMapName { get; private set; }
    }
}
