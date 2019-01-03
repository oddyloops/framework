using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface that specifies the requirements for a configuration class
    /// </summary>
    public interface IConfiguration
    {

        /// <summary>
        /// Sets the value corresponding to the specified key in the configuration settings
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value corresponding to key</param>
        void SetValue(string key, string value);


        /// <summary>
        /// Sets the value corresponding to the specified key in the configuration settings
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value corresponding to key</param>
        void SetValue(string key, object value);


        /// <summary>
        /// Gets the string referenced by the specified key in the configuration settings
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Corresponding string value</returns>
        string GetValue(string key);


        /// <summary>
        /// Gets the value referenced by the specified key in the configuration settings
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Corresponding object value</returns>
        object GetValueObject(string key);
    }
}
