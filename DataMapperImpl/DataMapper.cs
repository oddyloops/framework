using Framework.Attributes;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DataMapperImpl
{
    /// <summary>
    /// A concrete implementation of the IDataMapper interface
    /// </summary>
   
    public class DataMapper : IDataMapper
    {

        #region HelperMethods
        /// <summary>
        /// A helper method for retrieving a reference to an attribute within object metadata
        /// </summary>
        /// <param name="objType">Object metadta</param>
        /// <param name="attrType">Attribute type</param>
        /// <param name="isClass">A boolean flag indicating if attribute belongs to object class or field</param>
        /// <returns>A reference to the attribute of type attrType</returns>
        private Attribute GetAttribute(Type objType, Type attrType, bool isClass = false)
        {
            Attribute result = null;
            if (isClass)
            {
                // a class attribute
                var attrs = objType.GetCustomAttributes(attrType, false);
                if (attrs != null && attrs.Count() > 0)
                {
                    result = attrs.First() as Attribute;
                }
                else
                {
                    foreach (var _interface in objType.GetInterfaces())
                    { //recursively search its implemented interfaces (.net bug not performing this ancestral search)

                        result = GetAttribute(_interface, attrType, isClass);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            else
            {
                //is a field attribute
                foreach (var field in objType.GetProperties())
                {
                    var attrs = field.GetCustomAttributes(attrType, false);

                    if (attrs != null && attrs.Count() > 0)
                    {
                        result = attrs.First() as Attribute;
                    }
                }

                if (result == null)
                {
                    foreach (var _interface in objType.GetInterfaces())
                    {
                        result = GetAttribute(_interface, attrType, isClass);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// A helper method for returning object field metadata marked with the specified attribute
        /// </summary>
        /// <param name="objType">Object metadata</param>
        /// <param name="attrType">Attribute type</param>
        /// <returns>Object field metadata that is annotated with the attribute</returns>
        private PropertyInfo GetPropertyWithAttr(Type objType, Type attrType)
        {

            foreach (var property in objType.GetProperties())
            {
                var attrs = property.GetCustomAttributes(attrType, false);
                if (attrs != null && attrs.Count() > 0)
                {
                    return property;
                }
            }
            foreach (var _interface in objType.GetInterfaces())
            { //recursively search its implemented interfaces (.net bug not performing this ancestral search)

                var field = GetPropertyWithAttr(_interface, attrType);
                if (field != null)
                {
                    return field;
                }
            }
            return null;
        }


        /// <summary>
        /// A helper method for getting an attribute reference for a field
        /// </summary>
        /// <param name="objType">Containing object metadata</param>
        /// <param name="property">Field metadata</param>
        /// <param name="attrType">Attribute type</param>
        /// <returns>A reference to the attribute instance</returns>
        private Attribute GetPropertyAttribute(Type objType,PropertyInfo property,Type attrType)
        {
            foreach (var prop in objType.GetProperties())
            {
                if(prop.Equals(property))
                {
                    var attrs = prop.GetCustomAttributes(attrType, true);
                    if(attrs != null && attrs.Count() > 0)
                    {
                        return attrs.First() as Attribute;
                    }
                    else
                    {
                        //recursively search its implemented interfaces (.net bug not performing this ancestral search)
                        foreach(var _interface in objType.GetInterfaces())
                        {
                            var result = GetPropertyAttribute(_interface, property, attrType);
                            if(result != null)
                            {
                                return result;
                            }
                        }
                    }
                    break;
                }


            }

            return null;
           
        }

        #endregion



        /// <summary>
        /// A method for retrieving a field metadata based on its name, or map name
        /// </summary>
        /// <param name="fieldName">Field name or map name</param>
        /// <param name="objType">Object metadata</param>
        /// <returns>Metadata for field with specified name or map name</returns>
        public PropertyInfo GetFieldByName(string fieldName, Type objType)
        {
            foreach (var field in objType.GetProperties())
            {

                var mapAttr = field.GetCustomAttributes(typeof(MapAttribute), false);
                if (fieldName.Equals(field.Name) || (mapAttr != null && mapAttr.Count() > 0 && (mapAttr.First() as MapAttribute).MapName == fieldName))
                {
                    return field;
                }
                else
                {
                    foreach (var _interface in objType.GetInterfaces())
                    {
                        var result = GetFieldByName(fieldName, _interface);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Returns value of field mapped to specified name
        /// </summary>
        /// <param name="fieldName">Mapped field name</param>
        /// <param name="obj">Object to be mapped</param>
        /// <returns>Field reference</returns>
        public object GetField(string fieldName, object obj)
        {
            PropertyInfo field = GetFieldByName(fieldName, obj.GetType());

            if (field != null)
            {
                return field.GetValue(obj);
            }
            return null;

        }


        /// <summary>
        /// Gets name of object's key field
        /// </summary>
        /// <param name="objType">Type of object</param>
        /// <returns>key field name or null if no key field was found</returns>
        public string GetKeyName(Type objType)
        {

            PropertyInfo keyField = GetPropertyWithAttr(objType, typeof(PrimaryKeyAttribute));
            if (keyField != null)
            {
                return keyField.Name;
            }
            return null;
        }


        /// <summary>
        /// Get map name of object's key field
        /// </summary>
        /// <param name="objType">Object type</param>
        /// <returns>Key field map name or null if no key was found</returns>
        public string GetKeyMapName(System.Type objType)
        {
            PropertyInfo keyField = GetPropertyWithAttr(objType, typeof(PrimaryKeyAttribute));
            if (keyField != null)
            {
                MapAttribute mapAttribute = GetPropertyAttribute(objType, keyField, typeof(MapAttribute)) as MapAttribute;
                return mapAttribute == null? keyField.Name : mapAttribute.MapName;
            }
            return null;
        }

        /// <summary>
        /// Get the value of object's key field
        /// </summary>
        /// <param name="obj">object in question</param>
        /// <returns>key field value or null if no key fields were found</returns>
        public object GetKeyValue(object obj)
        {
            string keyName = GetKeyName(obj.GetType());

            if (keyName != null)
            {
                return GetField(keyName, obj);
            }

            return null;
        }

        /// <summary>
        /// Assigns a value to object's field mapped to specified field name
        /// </summary>
        /// <param name="fieldName">Field name mapped to field of interest</param>
        /// <param name="value">Value to be assigned to field of interest</param>
        /// <param name="obj">Object containing field</param>
        public void SetFieldValue(string fieldName, object value, object obj)
        {
            PropertyInfo field = GetFieldByName(fieldName, obj.GetType());
            if (field != null)
            {
                if (value != null)
                {
                    if (value.GetType() == typeof(decimal))
                    {
                        field.SetValue(obj, Convert.ToDouble(value));
                    }
                    else
                    {
                        field.SetValue(obj, value);
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Field with name " + fieldName +
                    " cannot be found in object " + obj.GetType());
            }

        }

        /// <summary>
        /// Fills an object of type T based on field name-value pairs
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="fields">Key-pairs of each field</param>
        /// <returns>An instance of type T</returns>
        public T CreateInstanceFromFields<T>(T obj, IDictionary<string, object> fields)
        {
            foreach(var field in fields)
            {
                SetFieldValue(field.Key, field.Value, obj);
            }
            return obj;

        }

        /// <summary>
        /// Gets the map name of an object class
        /// </summary>
        /// <param name="objType">Object metadata</param>
        /// <returns>Map name (if unmapped, returns object type name) </returns>
        public string GetObjectMapName(System.Type objType)
        {
            var mapAttribute = GetAttribute(objType, typeof(MapAttribute), true);
            if (mapAttribute != null)
            {
                return (mapAttribute as MapAttribute).MapName;
            }
            return objType.Name;
        }



        /// <summary>
        /// Returns a collection of all the field map names in object metadata. 
        /// Uses actual property names if map attribute is missing
        /// </summary>
        /// <param name="objType">Object metadata</param>
        /// <returns>Collection of field map names</returns>
        public IList<string> GetFieldNames(System.Type objType)
        {
            IList<string> mapNames = new List<string>();
            foreach(var field in objType.GetProperties())
            {
                var mapAttr = GetPropertyAttribute(objType, field, typeof(MapAttribute)) as MapAttribute;
                string mapName = mapAttr != null ? mapAttr.MapName : field.Name;
                mapNames.Add(mapName);
            }

            return mapNames;
        }

    }

}
