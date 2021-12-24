using Framework.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace Framework.Utils
{
    /// <summary>
    /// Uses attribute based validation to 
    /// ensure DTO fields obey all specified constraints
    /// </summary>
    public static class DataValidator
    {
        private const string PASSED = "Passed!";

        /// <summary>
        /// Runs validation on DTO
        /// </summary>
        /// <param name="dto">DTO</param>
        /// <returns>A pair of the success flag, and summary</returns>
        public static KeyValuePair<bool,string> Validate(object dto)
        {
            KeyValuePair<bool, string> result = new KeyValuePair<bool, string>(true,PASSED);
            foreach(var property in dto.GetType().GetProperties())
            {
                //check for required attribute
                RequiredAttribute required = property.GetCustomAttribute<RequiredAttribute>(true);
                if (required != null)
                {
                    //validate required
                    result = ValidateRequired(dto, property);
                    if(!result.Key)
                    {
                        break;
                    }
                }

                if(property.GetValue(dto) == null)
                {
                    //passed required test and is null 
                    //meaning object was not required
                    //and wasn't set
                    return result;
                }

                NumberAttribute number = property.GetCustomAttribute<NumberAttribute>(true);
                if(number != null)
                {
                    result = ValidateIsNumber(dto, property, number);
                    if(!result.Key)
                    {
                        break;
                    }
                }
                LengthAttribute length = property.GetCustomAttribute<LengthAttribute>(true);
                if(length != null)
                {
                    //validate length
                    result = ValidateLength(dto, property, length);
                    if (!result.Key)
                    {
                        break;
                    }
                }


                RangeAttribute range = property.GetCustomAttribute<RangeAttribute>(true);
                if(range != null)
                {
                    //validate range
                    result = ValidateRange(dto, property, range);
                    if (!result.Key)
                    {
                        break;
                    }
                }

                PatternAttribute pattern = property.GetCustomAttribute<PatternAttribute>(true);
                if(pattern != null)
                {
                    //validate pattern
                    result = ValidatePattern(dto, property, pattern);
                    if (!result.Key)
                    {
                        break;
                    }
                }

            }
            return result;
        }

        
        private static KeyValuePair<bool, string> ValidateRequired(object dto, PropertyInfo field)
        {
            object value = field.GetValue(dto);
            bool passed;
            string reason = PASSED;
            if(field.PropertyType == typeof(string))
            {
                passed = value != null && !string.IsNullOrEmpty(value.ToString());
            }
            else
            {
                passed = (value != null);
            }

            if(!passed)
            {
                reason = $"Field {field.Name} is required!";
            }
            return new KeyValuePair<bool, string>(passed, reason);
        }

        private static KeyValuePair<bool, string> ValidateLength(object dto, PropertyInfo field, LengthAttribute attrib)
        {
            string value = field.GetValue(dto).ToString();
            bool passed = true;
            string reason = PASSED;
            if(attrib.MaxLength > 0 && value.Length > attrib.MaxLength)
            {
                passed = false;
                reason = $"Field {field.Name} of length {value.Length} exceeds maximum allowed length of {attrib.MaxLength}";
            }
            if(value.Length < attrib.MinLength)
            {
                passed = false;
                reason = $"Field {field.Name} of length {value.Length} is shorter than the minimum allowed length of {attrib.MinLength}";
            }
            return new KeyValuePair<bool, string>(passed, reason);
        }

        private static KeyValuePair<bool,string> ValidateIsNumber(object dto,PropertyInfo field, NumberAttribute attrib)
        {
            string value = field.GetValue(dto).ToString();
            string reason = PASSED;
            bool passed = double.TryParse(value, out _);
            if (!passed)
            {
                reason = $"Field {field.Name} with value {value} is not a valid numeric format";
            }
            return new KeyValuePair<bool, string>(passed, reason);
        }

        private static KeyValuePair<bool,string> ValidateRange(object dto, PropertyInfo field, RangeAttribute attrib)
        {
            bool passed = true;
            string reason = PASSED;
            string value = field.GetValue(dto).ToString();
            if(value.Contains("."))
            {
                //double
                double dbl = Convert.ToDouble(value);
                if (dbl < attrib.Min || dbl > attrib.Max)
                {
                    passed = false;
                    reason = $"Field {field.Name} of value {dbl} falls outside specified range [{attrib.Min} - {attrib.Max}]";
                }
                
            }
            else
            {
                //long
                long lng = Convert.ToInt64(value);
                if(lng < attrib.Min || lng > attrib.Max)
                {
                    passed = false;
                    reason = $"Field {field.Name} of value {lng} falls outside specified range [{attrib.Min} - {attrib.Max}]";
                }
            }
            return new KeyValuePair<bool, string>(passed, reason);

        }

        private static KeyValuePair<bool,string> ValidatePattern(object dto, PropertyInfo field,PatternAttribute attrib)
        {
            bool passed = true;
            string reason = PASSED;
            string value = field.GetValue(dto).ToString();
            if(!Regex.IsMatch(value,attrib.Pattern))
            {
                passed = false;
                reason = $"Field {field.Name} does not match the specified regex pattern {attrib.Pattern}";
            }
            return new KeyValuePair<bool, string>(passed, reason);
        }

    }
}
