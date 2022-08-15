using Newtonsoft.Json.Linq;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Converter
{
    internal static class Extenstions
    {
        internal static string ToValueString(this PropertyInfo mProperty, object mValue, Type mGenericType = null)
        {
            Type type = (mGenericType != null ? mGenericType : mProperty.PropertyType).GetUnderlyingType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    bool boolean = mValue != null && (bool)mValue;
                    return mProperty.IsDefinedAttribute(typeof(BooleanToIntAttribute))
                                ? boolean ? "1" : "0"
                                : boolean.ToString();
                case TypeCode.Int32:
                    if (type.IsEnum)
                    {
                        if (mValue == null)
                        {
                            mValue = type.Default();
                        }
                        if (mProperty != null &&
                            mProperty.IsDefinedAttribute(typeof(EnumToIntAttribute)))
                        {
                            return ((int)mValue).ToString();
                        }
                        else
                        {
                            return mValue.ToString();
                        }
                    }
                    break;
                case TypeCode.String:
                    return mValue != null ? mValue.ToString() : string.Empty;
                case TypeCode.DateTime:
                    if (mValue == null) return string.Empty;
                    string dateTimeFormat = mProperty != null && mProperty.IsDefinedAttribute(typeof(DateTimeFormatAttribute))
                        ? mProperty.GetAttribute<DateTimeFormatAttribute>().Format
                        : "yyyy/MM/dd HH:mm:ss";
                    try
                    {
                        DateTime.Now.ToString(dateTimeFormat);
                    }
                    catch
                    {
                        dateTimeFormat = "yyyy/MM/dd HH:mm:ss";
                    }
                    return ((DateTime)mValue).ToString(dateTimeFormat);
                case TypeCode.Object:
                    if (mValue == null) return string.Empty;
                    string timeSpanFormat = mProperty != null && mProperty.IsDefinedAttribute(typeof(DateTimeFormatAttribute))
                        ? mProperty.GetAttribute<DateTimeFormatAttribute>().Format
                        : "hh\\:mm\\:ss";
                    try
                    {
                        TimeSpan.MinValue.ToString(timeSpanFormat);
                    }
                    catch
                    {
                        timeSpanFormat = "hh\\:mm\\:ss";
                    }
                    return ((TimeSpan)mValue).ToString(timeSpanFormat);
            }
            return mValue != null ? mValue.ToString() : type.IsValueType ? "0" : string.Empty;
        }
        internal static dynamic Parse(this PropertyInfo mProperty, string mValueString, Type mGenericType = null)
        {
            Type type = (mGenericType != null ? mGenericType : mProperty.PropertyType).GetUnderlyingType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    bool boolString = false;
                    int boolInt = 0;
                    if (bool.TryParse(mValueString, out boolString))
                    {
                        return boolString;
                    }
                    else if (int.TryParse(mValueString, out boolInt))
                    {
                        return boolInt != 0;
                    }
                    break;
                case TypeCode.Int32:
                    if (type.IsEnum)
                    {
                        int enumValue = 0;
                        if (Enum.IsDefined(type, mValueString))
                        {
                            return Enum.Parse(type, mValueString);
                        }
                        else if (int.TryParse(mValueString, out enumValue))
                        {
                            if (type.IsDefined(typeof(FlagsAttribute)))
                            {
                                int check = enumValue;
                                foreach (int flag in Enum.GetValues(type))
                                {
                                    check = (int.MaxValue ^ flag) & check;
                                }
                                if (check == 0)
                                {
                                    return Enum.ToObject(type, enumValue);
                                }
                            }
                            else if (Enum.IsDefined(type, enumValue))
                            {
                                return Enum.ToObject(type, enumValue);
                            }
                        }
                    }
                    break;
                case TypeCode.DateTime:
                    DateTime dateTime = default(DateTime);
                    if ((mProperty != null &&
                         mProperty.IsDefinedAttribute(typeof(DateTimeFormatAttribute)) &&
                         DateTime.TryParseExact(mValueString, mProperty.GetAttribute<DateTimeFormatAttribute>().Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime)) ||
                        DateTime.TryParse(mValueString, out dateTime))
                    {
                        return dateTime;
                    }
                    break;
                case TypeCode.Object:
                    if (type.Equals(typeof(TimeSpan)))
                    {
                        TimeSpan timeSpan = default(TimeSpan);
                        if ((mProperty != null &&
                             mProperty.IsDefinedAttribute(typeof(DateTimeFormatAttribute)) &&
                             TimeSpan.TryParseExact(mValueString, mProperty.GetAttribute<DateTimeFormatAttribute>().Format, CultureInfo.InvariantCulture, out timeSpan)) ||
                            TimeSpan.TryParse(mValueString, out timeSpan))
                        {
                            return timeSpan;
                        }
                    }
                    break;
                default:
                    try
                    {
                        return Convert.ChangeType(mValueString, type);
                    }
                    catch { }
                    break;
            }
            return (mGenericType != null ? mGenericType : mProperty.PropertyType).Default();
        }
        internal static dynamic Parse(this Type mType, string mValueString)
        {
            Type type = mType.GetUnderlyingType();
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                    bool boolString = false;
                    int boolInt = 0;
                    if (bool.TryParse(mValueString, out boolString))
                    {
                        return boolString;
                    }
                    else if (int.TryParse(mValueString, out boolInt))
                    {
                        return boolInt != 0;
                    }
                    break;
                case TypeCode.Int32:
                    if (type.IsEnum)
                    {
                        int enumValue = 0;
                        if (Enum.IsDefined(type, mValueString))
                        {
                            return Enum.Parse(type, mValueString);
                        }
                        else if (int.TryParse(mValueString, out enumValue))
                        {
                            if (type.IsDefined(typeof(FlagsAttribute)))
                            {
                                int check = enumValue;
                                foreach (int flag in Enum.GetValues(type))
                                {
                                    check = (int.MaxValue ^ flag) & check;
                                }
                                if (check == 0)
                                {
                                    return Enum.ToObject(type, enumValue);
                                }
                            }
                            else if (Enum.IsDefined(type, enumValue))
                            {
                                return Enum.ToObject(type, enumValue);
                            }
                        }
                    }
                    break;
                case TypeCode.DateTime:
                    DateTime dateTime = default(DateTime);
                    if (DateTime.TryParse(mValueString, out dateTime))
                    {
                        return dateTime;
                    }
                    break;
                case TypeCode.Object:
                    if (mType.Equals(typeof(TimeSpan)))
                    {
                        TimeSpan timeSpan = default(TimeSpan);
                        if (TimeSpan.TryParse(mValueString, out timeSpan))
                        {
                            return timeSpan;
                        }
                    }
                    break;
                default:
                    try
                    {
                        return Convert.ChangeType(mValueString, mType);
                    }
                    catch { }
                    break;
            }
            return mType.Default();
        }
        internal static Type GetUnderlyingType(this Type mType)
        {
            Type nullableType = Nullable.GetUnderlyingType(mType);
            return nullableType == null ? mType : nullableType;
        }
        internal static object Default(this Type mType)
        {
            return mType.IsEnum && mType.IsDefined(typeof(DefaultValueAttribute)) 
                ? mType.GetCustomAttribute<DefaultValueAttribute>().Value
                : mType.IsValueType 
                    ? Activator.CreateInstance(mType) 
                    : null;
        }
        internal static bool IsDefinedAttribute(this PropertyInfo mProperty, Type mCustomerAttribute)
        {
            if (!typeof(Attribute).IsAssignableFrom(mCustomerAttribute))
                throw new ArgumentException();
            if (mProperty.IsDefined(mCustomerAttribute)) return true;
            foreach (Type interfaceType in mProperty.DeclaringType.GetInterfaces())
            {
                PropertyInfo property = interfaceType.GetProperty(mProperty.Name);
                if (property == null) continue;
                if (property.IsDefined(mCustomerAttribute)) return true;
            }
            return false;
        }
        internal static T GetAttribute<T>(this PropertyInfo mProperty) where T : Attribute
        {
            Type attributeType = typeof(T);
            if (mProperty.IsDefined(attributeType))
                return mProperty.GetCustomAttribute<T>();
            foreach (Type interfaceType in mProperty.DeclaringType.GetInterfaces())
            {
                PropertyInfo property = interfaceType.GetProperty(mProperty.Name);
                if (property == null) continue;
                if (property.IsDefinedAttribute(attributeType))
                    return property.GetCustomAttribute<T>();
            }
            return default(T);
        }
    }
}
