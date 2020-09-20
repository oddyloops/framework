using System;
using System.Collections.Generic;
using System.Text;

namespace Framework.Interfaces
{
    /// <summary>
    /// An interface providing specifications for component containers for dependency injection an IoC
    /// </summary>
    public interface IContainer
    {

        /// <summary>
        /// Builds underlying container from library files in the working directory
        /// </summary>
        void BuildContainer();


        /// <summary>
        /// Builds underlying container from library files in each specified directories
        /// </summary>
        /// <param name="directory">Directories containing each required component libraries</param>
        void BuildContainer(params string[] directory);


        /// <summary>
        /// Creates or returns a single instance of the type T from the container
        /// </summary>
        /// <typeparam name="T">Type of instance to be created</typeparam>
        /// <returns>A shared or new instance of type T from the container</returns>
        T CreateInstance<T>();


        /// <summary>
        /// Creates or returns a single instance of the type T from the container referenced by the specified contract
        /// </summary>
        /// <typeparam name="T">Type of instance to be created</typeparam>
        /// <param name="contract">Contract name referencing the exact instance of T to be returned</param>
        /// <returns>A shared or new instance of type T from the container</returns>
        T CreateInstance<T>(string contract);


        /// <summary>
        /// Creates an instance for each implementation of interface T available in the container
        /// </summary>
        /// <typeparam name="T">Type of instance to be created</typeparam>
        /// <returns>A list of created instances</returns>
        IList<T> CreateInstances<T>();

        /// <summary>
        /// Creates an instance for each implementation of interface T available in the container referenced by the 
        /// specified contract
        /// </summary>
        /// <typeparam name="T">Type of instance to be created</typeparam>
        /// <param name="contract">Contract name referencing the exact instance of T to be returned</param>
        /// <returns>A list of created instances</returns>
        IList<T> CreateInstances<T>(string contract);

    }
}
