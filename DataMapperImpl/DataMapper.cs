﻿using Framework.Attributes;
using Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Reflection;

namespace DataMapperImpl
{
    /// <summary>
    /// A concrete implementation of the IDataMapper interface
    /// </summary>
    [Export(typeof(IDataMapper))]
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
        /// A helper method for retrieving a field metadata based on its name, or map name
        /// </summary>
        /// <param name="fieldName">Field name or map name</param>
        /// <param name="objType">Object metadata</param>
        /// <returns>Metadata for field with specified name or map name</returns>
        private PropertyInfo GetFieldByName(string fieldName, Type objType)
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
        #endregion

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
                if (value.GetType().Equals(field.PropertyType))
                {
                    field.SetValue(obj, value);
                }
            }
            else
            {
                throw new InvalidOperationException("Field with name " + fieldName +
                    " cannot be found in object " + obj.GetType());
            }

        }

        /// <summary>
        /// Creates an instance of type T based on field name-value pairs
        /// </summary>
        /// <param name="fields">Key-pairs of each field</param>
        /// <returns>An instance of type T</returns>
        public T CreateInstanceFromFields<T>(IDictionary<string, object> fields)
        {
            T obj = Activator.CreateInstance<T>();

            foreach(var field in fields)
            {
                SetFieldValue(field.Key, field.Value, obj);
            }

            return obj;

        }
    }

}