using Framework.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Utils
{
    /// <summary>
    /// Utility class
    /// </summary>
    public static class Util
    {
        /// <summary>
        /// Replaces placeholders in a file with desired value
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="templatePairs">Map containing placeholders to be replaced</param>
        /// <returns>New templated string with placeholders replaced</returns>

        public static string BuildStringTemplateFromFile(string filePath, IDictionary<string, string> templatePairs)
        {
            string str = System.IO.File.ReadAllText(filePath);
            return BuildStringTemplate(str, templatePairs);
        }

        /// <summary>
        /// Replaces placeholders in a string with desired value
        /// </summary>
        /// <param name="str">Original string</param>
        /// <param name="templatePairs">Map containing placeholders to be replaced</param>
        /// <returns>New templated string with placeholders replaced</returns>
        public static string BuildStringTemplate(string str, IDictionary<string, string> templatePairs)
        {
            foreach (var pair in templatePairs)
            {
                str = str.Replace(pair.Key, pair.Value);
            }
            return str;
        }

        /// <summary>
        /// Performs a deep copy of one object's fields to another (both objects need to matching field names)
        /// </summary>
        /// <param name="from">Source object</param>
        /// <param name="to">Destination object</param>
        /// <param name="excludeNulls">Exclude nulls in the copy process</param>
        /// <param name="excludeFieldNames">Field names to exclude</param>
        public static void DeepCopy(object from, object to, bool excludeNulls, params string[] excludeFieldNames)
        {
            foreach(var property in from.GetType().GetProperties())
            {
                object value = property.GetValue(from);
                
                if(!excludeFieldNames.Contains(property.Name) && (value != null || !excludeNulls))
                {
                    to.GetType().GetProperty(property.Name).SetValue(to, value);
                }
            }
        }

        /// <summary>
        /// A reference to the object container component used for dependency injection
        /// </summary>
        public static IContainer Container { get; set; }
    }
}
