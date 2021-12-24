using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Attributes
{
    /// <summary>
    /// Attribute for specifying collection name for NoSql document db data classes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple =false,Inherited = true)]
    public class NoSqlCollectionAttribute : Attribute
    {

        /// <summary>
        /// Collection Name
        /// </summary>
        public string CollectionName { get; set; }

        public NoSqlCollectionAttribute(string collectionName)
        {
            CollectionName = collectionName;
        }
    }
}
