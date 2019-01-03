using System;


namespace Framework.Attributes
{
    /// <summary>
    /// An attribute that specifies the field name in the data source that current object field corresponds to
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Interface,
        AllowMultiple = false, Inherited = true)]
    public class MapAttribute : Attribute
    {
        /// <summary>
        /// Constructor for specifying the data source field name
        /// </summary>
        /// <param name="mapName">Corresponding data source field name</param>
        public MapAttribute(string mapName)
        {
            MapName = mapName;
        }

        /// <summary>
        /// Data source field name
        /// </summary>
        public string MapName { get; private set; }
    }
}
