using Newtonsoft.Json.Linq;
using System.Collections;
using System.Reflection;
using System.Text;

namespace Converter
{
    /// <summary>
    /// </summary>
    public class JsonConvert
    {
        /// <summary>
        /// 將物件序列化成Json字串
        /// </summary>
        /// <param name="mObject">序列化物件</param>
        /// <param name="mComposing">排版</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidCastException"></exception>
        public static string Serialize(object mObject, bool mComposing = true)
        {
            if (mObject == null) throw new ArgumentNullException();
            Type type = mObject.GetType();
            TypeCode typeCode = Type.GetTypeCode(type);
            StringBuilder stringBuilder = new StringBuilder();
            if (type.IsArray ||
                typeof(IList).IsAssignableFrom(type))
            {
                ListToJsonString(null, mObject as IEnumerable, stringBuilder, string.Empty, mComposing);
            }
            else if (type.IsValueType ||
                     typeCode == TypeCode.String ||
                     typeCode == TypeCode.DateTime ||
                     type == typeof(TimeSpan))
            {
                throw new InvalidCastException();
            }
            else
            {
                ObjectToJsonString(mObject, stringBuilder, string.Empty, mComposing);
            }
            return stringBuilder.ToString();
        }
        private static void ObjectToJsonString(object mObject, StringBuilder mStringBuilder, string mTab, bool mComposing)
        {
            Type type = mObject.GetType();
            PropertyInfo[] properties = type.GetProperties();
            mStringBuilder.Append("{");
            bool first = true;
            bool isEmpty = true;
            foreach (PropertyInfo property in properties.OrderBy(x => x.IsDefinedAttribute(typeof(DisplayOrderAttribute)) ? x.GetAttribute<DisplayOrderAttribute>().Index : properties.Length).ThenBy(x => x.Name))
            {
                if (!property.CanRead || 
                    (property.IsDefinedAttribute(typeof(IgnoreItemAttribute)) && property.GetAttribute<IgnoreItemAttribute>().IgnoreFlags.HasFlag(IgnoreFlags.Serialize))) continue;
                object value = property.GetValue(mObject);
                string valueString = string.Empty;
                if (!property.IsDefinedAttribute(typeof(DisplayEmptyItemAttribute)) && value == null) continue;
                string tagName = property.IsDefinedAttribute(typeof(TagRenameAttribute))
                    ? property.GetAttribute<TagRenameAttribute>().TagName
                    : property.Name;
                if (first)
                {
                    if (mComposing)
                    {
                        mStringBuilder.AppendLine(string.Empty);
                    }
                    first = false;
                }
                else
                {
                    if (mComposing)
                    {
                        mStringBuilder.AppendLine(",");
                    }
                    else
                    {
                        mStringBuilder.Append(",");
                    }
                }
                if (property.PropertyType.IsArray ||
                    typeof(IList).IsAssignableFrom(property.PropertyType))
                {
                    if (mComposing)
                    {
                        mStringBuilder.Append(mTab + "    \"" + tagName + "\" : ");
                    }
                    else
                    {
                        mStringBuilder.Append("\"" + tagName + "\":");
                    }
                    if (value == null)
                    {
                        if (mComposing)
                        {
                            mStringBuilder.AppendLine("[ ]");
                        }
                        else
                        {
                            mStringBuilder.Append("[]");
                        }
                    }
                    else
                    {
                        ListToJsonString(property, value as IEnumerable, mStringBuilder, mTab + "    ", mComposing);
                    }
                }
                else
                {
                    Type propertyType = property.PropertyType.GetUnderlyingType();
                    TypeCode typeCode = Type.GetTypeCode(propertyType);
                    if (propertyType.IsValueType ||
                        typeCode == TypeCode.String ||
                        typeCode == TypeCode.DateTime ||
                        propertyType == typeof(TimeSpan))
                    {
                        if (typeCode == TypeCode.Boolean)
                        {
                            valueString = value != null && (bool)value ? "1" : "0";
                        }
                        else
                        {
                            valueString = property.ToValueString(value);
                            if (!propertyType.IsValueType ||
                                typeCode == TypeCode.DateTime ||
                                propertyType == typeof(TimeSpan) ||
                                (propertyType.IsEnum && !property.IsDefinedAttribute(typeof(EnumToIntAttribute))))
                            {
                                valueString = "\"" + valueString + "\"";
                            }
                        }
                        if (mComposing)
                        {
                            mStringBuilder.Append(mTab + "    \"" + tagName + "\" : " + valueString);
                        }
                        else
                        {
                            mStringBuilder.Append("\"" + tagName + "\":" + valueString);
                        }
                    }
                    else
                    {
                        if (mComposing)
                        {
                            mStringBuilder.Append(mTab + "    \"" + tagName + "\" : ");
                        }
                        else
                        {
                            mStringBuilder.Append("\"" + tagName + "\":");
                        }
                        if (value == null)
                        {
                            if (mComposing)
                            {
                                mStringBuilder.AppendLine("{ }");
                            }
                            else
                            {
                                mStringBuilder.Append("{}");
                            }
                        }
                        else
                        {
                            ObjectToJsonString(value, mStringBuilder, mTab + "    ", mComposing);
                        }
                    }
                }
                isEmpty = false;
            }
            if (mComposing)
            {
                if (isEmpty)
                {
                    mStringBuilder.Append(" }");
                }
                else
                {
                    mStringBuilder.Append("\r\n" + mTab + "}");
                }
            }
            else
            {
                mStringBuilder.Append("}");
            }
        }
        private static void ListToJsonString(PropertyInfo mProperty, IEnumerable mEnumerable, StringBuilder mStringBuilder, string mTab, bool mComposing)
        {
            mStringBuilder.Append("[");
            bool first = true;
            bool isEmpty = true;
            Type type = mEnumerable.GetType();
            Type genericType = type.IsArray 
                ? type.GetElementType()
                : type.GenericTypeArguments[0];
            TypeCode genericTypeCode = Type.GetTypeCode(genericType);
            if (genericType.IsValueType ||
                genericTypeCode == TypeCode.String ||
                genericTypeCode == TypeCode.DateTime ||
                genericType == typeof(TimeSpan))
            {
                List<string> jsonArray = new List<string>();
                if (genericTypeCode == TypeCode.Boolean)
                {
                    foreach (bool value in mEnumerable)
                    {
                        jsonArray.Add(value ? "1" : "0");
                    }
                }
                else if (genericType.IsEnum)
                {
                    if (mProperty != null &&
                        mProperty.IsDefinedAttribute(typeof(EnumToIntAttribute)))
                    {
                        foreach (int value in mEnumerable)
                        {
                            jsonArray.Add(value.ToString());
                        }
                    }
                    else
                    {
                        foreach (object value in mEnumerable)
                        {
                            jsonArray.Add("\"" + value.ToString() + "\"");
                        }
                    }
                }
                else if (genericType.IsValueType)
                {
                    foreach (object value in mEnumerable)
                    {
                        jsonArray.Add(value.ToString());
                    }
                }
                else if (genericTypeCode == TypeCode.DateTime)
                {
                    string format = mProperty != null && mProperty.IsDefinedAttribute(typeof(DateTimeFormatAttribute))
                        ? mProperty.GetAttribute<DateTimeFormatAttribute>().Format
                        : "yyyy/MM/dd HH:mm:ss";
                    try
                    {
                        DateTime.Now.ToString(format);
                    }
                    catch
                    {
                        format = "yyyy/MM/dd HH:mm:ss";
                    }
                    foreach (DateTime dateTime in mEnumerable)
                    {
                        jsonArray.Add("\"" + dateTime.ToString(format) + "\"");
                    }
                }
                else if (genericType == typeof(TimeSpan))
                {
                    string format = mProperty != null && mProperty.IsDefinedAttribute(typeof(DateTimeFormatAttribute))
                        ? mProperty.GetAttribute<DateTimeFormatAttribute>().Format
                        : "hh\\:mm\\:ss";
                    try
                    {
                        TimeSpan.MinValue.ToString(format);
                    }
                    catch
                    {
                        format = "hh\\:mm\\:ss";
                    }
                    foreach (TimeSpan dateTime in mEnumerable)
                    {
                        jsonArray.Add("\"" + dateTime.ToString(format) + "\"");
                    }
                }
                else
                {
                    foreach (object value in mEnumerable)
                    {
                        if (value == null) continue;
                        jsonArray.Add("\"" + value.ToString() + "\"");
                    }
                }
                if (jsonArray.Count > 0)
                {
                    if (mComposing)
                    {
                        mStringBuilder.Append(string.Join(", ", jsonArray));
                    }
                    else
                    {
                        mStringBuilder.Append(string.Join(",", jsonArray));
                    }
                }
                if (mComposing)
                {
                    mStringBuilder.Append(" ]");
                }
                else
                {
                    mStringBuilder.Append("]");
                }
            }
            else
            {
                foreach (object obj in mEnumerable)
                {
                    if (obj == null) continue;
                    if (first)
                    {
                        if (mComposing)
                        {
                            mStringBuilder.Append("\r\n" + mTab + "    ");
                        }
                        first = false;
                    }
                    else
                    {
                        if (mComposing)
                        {
                            mStringBuilder.Append(",\r\n" + mTab + "    ");
                        }
                        else
                        {
                            mStringBuilder.Append(",");
                        }
                    }
                    ObjectToJsonString(obj, mStringBuilder, mTab + "    ", mComposing);
                    isEmpty = false;
                }
                if (mComposing)
                {
                    if (isEmpty)
                    {
                        mStringBuilder.Append(" ]");
                    }
                    else
                    {
                        mStringBuilder.Append("\r\n" + mTab + "]");
                    }
                }
                else
                {
                    mStringBuilder.Append("]");
                }
            }
        }
        /// <summary>
        /// 將Json字串反序列化成物件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mJsonString">Json字串</param>
        /// <returns></returns>
        /// <exception cref="InvalidCastException"></exception>
        /// <exception cref="FormatException">Json字串格式錯誤</exception>
        public static T Deserialize<T>(string mJsonString)
        {
            if (string.IsNullOrWhiteSpace(mJsonString)) return default(T);
            T obj = default(T);
            switch (mJsonString.TrimStart(' ')[0])
            {
                case '[':
                    if (!typeof(T).IsArray &&
                        !typeof(IList).IsAssignableFrom(typeof(T))) throw new InvalidCastException();
                    JArray jArray = null;
                    try
                    {
                        jArray = JArray.Parse(mJsonString);
                    }
                    catch { throw new FormatException(); }
                    Type genericType = typeof(T).IsArray
                        ? typeof(T).GetElementType()
                        : typeof(T).GenericTypeArguments[0];
                    TypeCode genericTypeCode = Type.GetTypeCode(genericType);
                    IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(genericType));
                    foreach (JToken token in jArray)
                    {
                        object value = null;
                        if (genericType.IsValueType ||
                            genericTypeCode == TypeCode.String ||
                            genericTypeCode == TypeCode.DateTime ||
                            genericType == typeof(TimeSpan))
                        {
                            JValue jValue = null;
                            try
                            {
                                jValue = (JValue)token;
                            }
                            catch { continue; }
                            if (jValue.Value == null) continue;
                            value = genericType.Parse(jValue.Value.ToString());
                        }
                        else
                        {
                            JObject jObj = null;
                            try
                            {
                                jObj = (JObject)token;
                                value = Activator.CreateInstance(genericType);
                            }
                            catch { continue; }
                            ParseObject(value, jObj);
                        }
                        if (value == null) continue;
                        list.Add(value);
                    }
                    if (!typeof(T).IsArray)
                    {
                        Array array = Array.CreateInstance(genericType, jArray.Count);
                        for (int index = 0; index < list.Count; index++)
                        {
                            object data = list[index];
                            array.SetValue(data, index);
                        }
                        obj = (T)(object)array;
                    }
                    else
                    {
                        obj = (T)list;
                    }
                    break;
                case '{':
                    obj = (T)Activator.CreateInstance(typeof(T));
                    JObject jObject = null;
                    try
                    {
                        jObject = JObject.Parse(mJsonString);
                    }
                    catch { throw new FormatException(); }
                    ParseObject(obj, jObject);
                    break;
                default: throw new FormatException();
            }
            return obj;
        }
        private static void ParseObject(object mObject, JObject mJObject)
        {
            if (mObject == null) return;
            foreach (PropertyInfo property in mObject.GetType().GetProperties())
            {
                if (property.IsDefinedAttribute(typeof(IgnoreItemAttribute)) &&
                    property.GetAttribute<IgnoreItemAttribute>().IgnoreFlags.HasFlag(IgnoreFlags.Deserialize)) continue;
                string tagName = property.IsDefinedAttribute(typeof(TagRenameAttribute))
                    ? property.GetAttribute<TagRenameAttribute>().TagName
                    : property.Name;
                if (!mJObject.ContainsKey(tagName)) continue;
                object value = null;
                if (property.PropertyType.IsArray)
                {
                    if (!property.CanWrite) continue;
                    JArray jArray = null;
                    try
                    {
                        jArray = (JArray)mJObject[tagName];
                    }
                    catch { continue; }
                    Type elementType = property.PropertyType.GetElementType();
                    IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
                    ParseJArray(property, list, jArray);
                    Array array = Array.CreateInstance(elementType, list.Count);
                    for (int index = 0; index < list.Count; index++)
                    {
                        object obj = list[index];
                        array.SetValue(obj, index);
                    }
                    value = array;
                }
                else
                {
                    Type propertyType = property.PropertyType.GetUnderlyingType();
                    TypeCode typeCode = Type.GetTypeCode(propertyType);
                    if (propertyType.IsValueType ||
                        typeCode == TypeCode.String ||
                        typeCode == TypeCode.DateTime ||
                        propertyType == typeof(TimeSpan))
                    {
                        if (!property.CanWrite) continue;
                        JValue jValue = null;
                        try
                        {
                            jValue = (JValue)mJObject[tagName];
                        }
                        catch { continue; }
                        if (jValue.Value == null)
                        {
                            value = property.PropertyType.Default();
                        }
                        else
                        {
                            value = property.Parse(jValue.Value.ToString());
                        }
                    }
                    else
                    {
                        value = property.GetValue(mObject, null);
                        if (typeof(IList).IsAssignableFrom(propertyType))
                        {
                            bool readOnlyCollection = false;
                            if ((readOnlyCollection = typeof(IReadOnlyCollection<>).MakeGenericType(property.PropertyType.GenericTypeArguments).IsAssignableFrom(property.PropertyType)) ||
                                value == null)
                            {
                                if (!property.CanWrite) continue;
                                try
                                {
                                    value = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(property.PropertyType.GenericTypeArguments));
                                }
                                catch { continue; }
                            }
                            JArray jArray = null;
                            try
                            {
                                jArray = (JArray)mJObject[tagName];
                            }
                            catch { continue; }
                            IList list = (IList)value;
                            ParseJArray(property, list, jArray);
                            if (readOnlyCollection)
                            {
                                try
                                {
                                    value = Activator.CreateInstance(property.PropertyType, list);
                                }
                                catch { continue; }
                            }
                        }
                        else
                        {
                            if (value == null)
                            {
                                if (!property.CanWrite) continue;
                                try
                                {
                                    value = Activator.CreateInstance(property.PropertyType, true);
                                }
                                catch { continue; }
                            }
                            JObject jObject = null;
                            try
                            {
                                jObject = (JObject)mJObject[tagName];
                            }
                            catch { continue; }
                            ParseObject(value, jObject);
                        }
                    }
                }
                if (value == null) continue;
                property.SetValue(mObject, value);
            }
        }
        private static void ParseJArray(PropertyInfo mProperty, IList mList, JArray mJArray)
        {
            Type genericType = mList.GetType().GenericTypeArguments[0];
            TypeCode genericTypeCode = Type.GetTypeCode(genericType);
            if (genericType.IsValueType ||
                genericTypeCode == TypeCode.String ||
                genericTypeCode == TypeCode.DateTime ||
                genericType == typeof(TimeSpan))
            {
                foreach (JToken token in mJArray)
                {
                    JValue jValue = null;
                    try
                    {
                        jValue = (JValue)token;
                    }
                    catch { continue; }
                    if (jValue.Value == null) continue;
                    object value = mProperty.Parse(jValue.Value.ToString(), genericType);
                    if (value == null) continue;
                    mList.Add(value);
                }
            }
            else
            {
                foreach (JToken token in mJArray)
                {
                    object value = null;
                    JObject jObject = null;
                    try
                    {
                        jObject = (JObject)token;
                        value = Activator.CreateInstance(genericType, true);
                    }
                    catch { continue; }
                    ParseObject(value, jObject);
                    mList.Add(value);
                }
            }
        }
    }
}
