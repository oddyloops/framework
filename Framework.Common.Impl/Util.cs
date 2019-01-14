using Framework.Interfaces;
using System.Collections.Generic;

namespace Framework.Common.Impl
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
        /// A reference to the object container component used for dependency injection
        /// </summary>
        public static IContainer Container { get; set; }
    }
}
