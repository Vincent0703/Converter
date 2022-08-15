using System.Collections;
using System.Data;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Converter
{
    /// <summary>
    /// </summary>
    public class XmlConvert
    {
        /// <summary>
        /// 將物件序列化成Xml字串
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
            if (type.IsArray ||
                typeof(IList).IsAssignableFrom(type))
            {
                throw new InvalidCastException();
            }
            StringBuilder stringBuilder = new StringBuilder();
            ObjectToXmlString(null, mObject, stringBuilder, string.Empty, mComposing);
            return stringBuilder.ToString();
        }
        private static bool ObjectToXmlString(PropertyInfo mProperty, object mObject, StringBuilder mStringBuilder, string mTab, bool mComposing)
        {
            Type type = mObject.GetType();
            string tagName = mProperty == null
                ? type.IsDefined(typeof(TagRenameAttribute))
                        ? type.GetCustomAttribute<TagRenameAttribute>().TagName
                        : type.Name
                : mProperty.IsDefined(typeof(XmlArrayAttribute))
                    ? mProperty.GetCustomAttribute<XmlArrayAttribute>().ChildTagName
                    : mProperty.IsDefinedAttribute(typeof(TagRenameAttribute))
                        ? mProperty.GetAttribute<TagRenameAttribute>().TagName
                        : mProperty.Name;
            List<string> attributeString = new List<string>();
            List<PropertyInfo> xmlContentProperties = new List<PropertyInfo>();
            PropertyInfo contentProperty = null;
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties.OrderBy(x => x.IsDefinedAttribute(typeof(DisplayOrderAttribute)) ? x.GetAttribute<DisplayOrderAttribute>().Index : properties.Length).ThenBy(x => x.Name))
            {
                if (!property.CanRead || 
                    (property.IsDefinedAttribute(typeof(IgnoreItemAttribute)) && property.GetAttribute<IgnoreItemAttribute>().IgnoreFlags.HasFlag(IgnoreFlags.Serialize))) continue;
                if (property.IsDefined(typeof(XmlAttributeAttribute)))
                {
                    object value = property.GetValue(mObject);
                    string tag = property.IsDefinedAttribute(typeof(TagRenameAttribute))
                                   ? property.GetAttribute<TagRenameAttribute>().TagName
                                   : property.Name;
                    if (value == null) continue;
                    attributeString.Add(" " + tag + "=\"" + property.ToValueString(value) + "\"");
                }
                else if (property.IsDefined(typeof(XmlContentAttribute)))
                {
                    xmlContentProperties.Clear();
                    contentProperty = property;
                }
                else if (contentProperty == null)
                {
                    xmlContentProperties.Add(property);
                }
            }
            int lastIndex = mStringBuilder.Length;
            if (mComposing)
            {
                if (contentProperty != null)
                {
                    object value = contentProperty.GetValue(mObject);
                    string valueString = XmlEncode(contentProperty.ToValueString(contentProperty.GetValue(mObject)));
                    if (string.IsNullOrEmpty(valueString))
                    {
                        if (attributeString.Count > 0 ||
                            contentProperty.IsDefinedAttribute(typeof(DisplayEmptyItemAttribute)))
                        {
                            mStringBuilder.Append(mTab + "<" + tagName + string.Join(string.Empty, attributeString) + "/>");
                        }
                    }
                    else
                    {
                        mStringBuilder.Append(mTab + "<" + tagName + string.Join(string.Empty, attributeString) + ">" + valueString + "</" + tagName + ">");
                    }
                }
                else if (xmlContentProperties.Count > 0 &&
                         !PropertiesToXmlString(mObject, xmlContentProperties, mStringBuilder, mTab + "    ", mComposing))
                {
                    mStringBuilder.Insert(lastIndex, mTab + "<" + tagName + string.Join(string.Empty, attributeString) + ">\r\n");
                    mStringBuilder.Append("\r\n" + mTab + "</" + tagName + ">");
                }
                else
                {
                    mStringBuilder.Append(mTab + "<" + tagName + string.Join(string.Empty, attributeString) + "/>");
                }
            }
            else
            {
                if (contentProperty != null)
                {
                    object value = contentProperty.GetValue(mObject);
                    if (value == null)
                    {
                        if (attributeString.Count > 0 ||
                            contentProperty.IsDefinedAttribute(typeof(DisplayEmptyItemAttribute)))
                        {
                            mStringBuilder.Append("<" + tagName + string.Join(string.Empty, attributeString) + "/>");
                        }
                    }
                    else
                    {
                        mStringBuilder.Append("<" + tagName + string.Join(string.Empty, attributeString) + ">" + XmlEncode(contentProperty.ToValueString(value)) + "</" + tagName + ">");
                    }
                }
                else if (xmlContentProperties.Count == 0 ||
                         PropertiesToXmlString(mObject, xmlContentProperties, mStringBuilder, mTab + "    ", mComposing))
                {
                    mStringBuilder.Append("<" + tagName + string.Join(string.Empty, attributeString) + "/>");
                }
                else
                {
                    mStringBuilder.Insert(lastIndex, "<" + tagName + string.Join(string.Empty, attributeString) + ">");
                    mStringBuilder.Append("</" + tagName + ">");
                }
            }
            return false;
        }
        private static bool PropertiesToXmlString(object mObject, IEnumerable<PropertyInfo> mProperties, StringBuilder mStringBuilder, string mTab, bool mComposing)
        {
            bool isEmpty = true;
            Type propertyType = null;
            bool first = true;
            foreach (PropertyInfo property in mProperties)
            {
                string tagName = property.IsDefinedAttribute(typeof(TagRenameAttribute))
                    ? property.GetAttribute<TagRenameAttribute>().TagName
                    : property.Name;
                object value = property.GetValue(mObject);
                string valueString = string.Empty;
                if (!property.IsDefinedAttribute(typeof(DisplayEmptyItemAttribute)) && value == null) continue;
                if (first)
                {
                    first = false;
                }
                else
                {
                    mStringBuilder.AppendLine(string.Empty);
                }
                if (property.PropertyType.IsArray ||
                    typeof(IList).IsAssignableFrom(property.PropertyType))
                {
                    if (property.IsDefined(typeof(XmlArrayAttribute)))
                    {
                        int lastIndex = mStringBuilder.Length;
                        if (mComposing)
                        {
                            if (value == null ||
                                ListToXmlString(property, value as IEnumerable, mStringBuilder, mTab + "    ", mComposing))
                            {
                                mStringBuilder.Append(mTab + "<" + tagName + "/>");
                            }
                            else
                            {
                                mStringBuilder.Insert(lastIndex, mTab + "<" + tagName + ">\r\n");
                                mStringBuilder.Append("\r\n" + mTab + "</" + tagName + ">");
                            }
                        }
                        else
                        {
                            if (value == null ||
                                ListToXmlString(property, value as IEnumerable, mStringBuilder, mTab + "    ", mComposing))
                            {
                                mStringBuilder.Append("<" + tagName + "/>");
                            }
                            else
                            {
                                mStringBuilder.Insert(lastIndex, "<" + tagName + ">");
                                mStringBuilder.Append("</" + tagName + ">");
                            }
                        }
                    }
                    else
                    {
                        isEmpty = ListToXmlString(property, value as IList, mStringBuilder, mTab, mComposing);
                    }
                }
                else
                {
                    propertyType = property.PropertyType.GetUnderlyingType();
                    TypeCode typeCode = Type.GetTypeCode(propertyType);
                    if (propertyType.IsValueType ||
                        typeCode == TypeCode.String ||
                        typeCode == TypeCode.DateTime ||
                        propertyType == typeof(TimeSpan))
                    {
                        valueString = property.ToValueString(value);
                        string content = (string.IsNullOrEmpty(valueString)
                                ? "<" + tagName + "/>"
                                : "<" + tagName + ">" + XmlEncode(valueString) + "</" + tagName + ">");
                        if (mComposing)
                        {
                            mStringBuilder.Append(mTab + content);
                        }
                        else
                        {
                            mStringBuilder.Append(content);
                        }
                        isEmpty = false;
                    }
                    else
                    {
                        if (value == null)
                        {
                            PropertyInfo contentProperty = null;
                            List<string> attributes = new List<string>();
                            foreach (PropertyInfo property1 in propertyType.GetProperties())
                            {
                                if (!property1.CanRead ||
                                    (property1.IsDefinedAttribute(typeof(IgnoreItemAttribute)) && property1.GetAttribute<IgnoreItemAttribute>().IgnoreFlags.HasFlag(IgnoreFlags.Serialize))) continue;
                                if (property1.PropertyType.IsDefined(typeof(XmlAttributeAttribute)))
                                {
                                    contentProperty = property1;
                                    continue;
                                }
                                if (property1.PropertyType.IsDefined(typeof(XmlAttributeAttribute)))
                                {
                                    attributes.Add(" " + (property1.PropertyType.IsDefined(typeof(TagRenameAttribute))
                                                             ? property1.PropertyType.GetCustomAttribute<TagRenameAttribute>().TagName
                                                             : property1.Name) +
                                                   "=\"\"");
                                }
                            }
                            if (mComposing)
                            {
                                mStringBuilder.Append(mTab + "<" + tagName + string.Join(string.Empty, attributes) + "/>");
                            }
                            else
                            {
                                mStringBuilder.Append("<" + tagName + string.Join(string.Empty, attributes) + "/>");
                            }
                            isEmpty = false;
                        }
                        else
                        {
                            isEmpty = ObjectToXmlString(property, value, mStringBuilder, mTab, mComposing);
                        }
                    }
                }
            }
            return isEmpty;
        }
        private static bool ListToXmlString(PropertyInfo mProperty, IEnumerable mList, StringBuilder mStringBuilder, string mTab, bool mComposing)
        {
            bool isEmpty = true;
            Type genericType = mProperty.PropertyType.IsArray
                ? mProperty.PropertyType.GetElementType()
                : mProperty.PropertyType.GenericTypeArguments[0];
            TypeCode genericTypeCode = Type.GetTypeCode(genericType);
            string tagName = mProperty.IsDefined(typeof(XmlArrayAttribute))
                ? mProperty.GetCustomAttribute<XmlArrayAttribute>().ChildTagName
                : mProperty.IsDefinedAttribute(typeof(TagRenameAttribute))
                    ? mProperty.GetAttribute<TagRenameAttribute>().TagName
                    : mProperty.Name;
            bool first = true;
            if (genericType.IsValueType ||
                genericTypeCode == TypeCode.String ||
                genericTypeCode == TypeCode.DateTime ||
                genericType == typeof(TimeSpan))
            {
                foreach (object data in mList)
                {
                    if (data == null) continue;
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        mStringBuilder.AppendLine(string.Empty);
                    }
                    string valueString = string.Empty;
                    valueString = mProperty.ToValueString(data, genericType);
                    string content = string.IsNullOrEmpty(valueString)
                        ? "<" + tagName + "/>"
                        : "<" + tagName + ">" + XmlEncode(valueString) + "</" + tagName + ">";
                    if (mComposing)
                    {
                        mStringBuilder.Append(mTab + content);
                    }
                    else
                    {
                        mStringBuilder.Append(content);
                    }
                    isEmpty = false;
                }
            }
            else
            {
                foreach (object data in mList)
                {
                    if (data == null) continue;
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        mStringBuilder.AppendLine(string.Empty);
                    }
                    isEmpty = ObjectToXmlString(mProperty, data, mStringBuilder, mTab, mComposing);
                }
            }
            return isEmpty;
        }
        /// <summary>
        /// 將Xml字串反序列化成物件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="mXmlString">Xml字串</param>
        /// <returns></returns>
        /// <exception cref="FormatException">Xml字串格式錯誤</exception>
        public static T Deserialize<T>(string mXmlString)
        {
            XmlDocument xml = new XmlDocument();
            try
            {
                xml.LoadXml(mXmlString);
            }
            catch
            {
                throw new FormatException("Xml字串格式錯誤");
            }
            T obj = (T)Activator.CreateInstance(typeof(T));
            ParseXmlNode(obj, xml.DocumentElement);
            return obj;
        }
        private static void ParseXmlNode(object mObject, XmlNode mXmlNode)
        {
            if (mObject == null) return;
            List<string> tagLists = new List<string>();
            for (int i = 0; i < mXmlNode.ChildNodes.Count; i++)
            {
                tagLists.Add(mXmlNode.ChildNodes[i].Name);
            }
            foreach (PropertyInfo property in mObject.GetType().GetProperties())
            {
                if (property.IsDefinedAttribute(typeof(IgnoreItemAttribute)) && 
                    property.GetAttribute<IgnoreItemAttribute>().IgnoreFlags.HasFlag(IgnoreFlags.Deserialize)) continue;
                string tagName = property.Name;
                if (property.IsDefinedAttribute(typeof(TagRenameAttribute)))
                    tagName = property.GetAttribute<TagRenameAttribute>().TagName;
                object value = null;
                XmlNode node = null;
                if (property.PropertyType.IsArray)
                {
                    if (!property.CanWrite || !tagLists.Contains(tagName)) continue;
                    if (property.IsDefined(typeof(XmlArrayAttribute)))
                    {
                        node = mXmlNode[tagName];
                        if (node != null)
                        {
                            value = ParseXmlNodeList(property, property.PropertyType.GetElementType(), node.ChildNodes);
                        }
                    }
                    else
                    {
                        value = ParseXmlNodeList(property, property.PropertyType.GetElementType(), mXmlNode.ChildNodes);
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
                        if (!property.CanWrite) continue;
                        if (property.IsDefined(typeof(XmlAttributeAttribute)))
                        {
                            node = mXmlNode.Attributes[tagName];
                            value = node != null
                                ? property.Parse(XmlDecode(node.Value))
                                : property.PropertyType.Default();
                        }
                        else if (property.IsDefined(typeof(XmlContentAttribute)))
                        {
                            value = property.Parse(XmlDecode(mXmlNode.InnerXml));
                        }
                        else if (tagLists.Contains(tagName))
                        {
                            node = mXmlNode[tagName];
                            value = node != null
                                ? property.Parse(XmlDecode(node.InnerXml))
                                : property.PropertyType.Default();
                        }
                    }
                    else
                    {
                        if (!tagLists.Contains(tagName)) continue;
                        value = property.GetValue(mObject, null);
                        if (typeof(IList).IsAssignableFrom(property.PropertyType))
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
                            IList list = (IList)value;
                            if (property.IsDefined(typeof(XmlArrayAttribute)))
                            {
                                node = mXmlNode[tagName];
                                if (node != null)
                                {
                                    ParseXmlNodeList(property, list, node.ChildNodes);
                                }
                            }
                            else
                            {
                                ParseXmlNodeList(property, list, mXmlNode.ChildNodes);
                            }
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
                            node = mXmlNode[tagName];
                            if (node != null)
                            {
                                ParseXmlNode(value, node);
                            }
                        }
                    }
                }
                if (value == null) continue;
                property.SetValue(mObject, value);
            }
        }
        private static Array ParseXmlNodeList(PropertyInfo mProperty, Type mElementType, XmlNodeList mChildNodes)
        {
            IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(mElementType));
            ParseXmlNodeList(mProperty, list, mChildNodes);
            Array array = Array.CreateInstance(mElementType, list.Count);
            for (int index = 0; index < list.Count; index++)
            {
                object obj = list[index];
                array.SetValue(obj, index);
            }
            return array;
        }
        private static void ParseXmlNodeList(PropertyInfo mProperty, IList mList, XmlNodeList mChildNodes)
        {
            string tagName = mProperty.IsDefined(typeof(XmlArrayAttribute)) 
                ? mProperty.GetCustomAttribute<XmlArrayAttribute>().ChildTagName
                : mProperty.Name;
            Type genericType = mList.GetType().GenericTypeArguments[0];
            TypeCode genericTypeCode = Type.GetTypeCode(genericType);
            if (genericType.IsValueType ||
                genericTypeCode == TypeCode.String ||
                genericTypeCode == TypeCode.DateTime ||
                genericType == typeof(TimeSpan))
            {
                for (int i = 0; i < mChildNodes.Count; i++)
                {
                    XmlNode node = mChildNodes[i];
                    if (node.Name != tagName) continue;
                    object value = mProperty.Parse(XmlDecode(node.InnerXml), genericType);
                    if (value == null) continue;
                    mList.Add(value);
                }
            }
            else
            {
                for (int i = 0; i < mChildNodes.Count; i++)
                {
                    XmlNode node = mChildNodes[i];
                    if (node.Name != tagName) continue;
                    object value = null;
                    try
                    {
                        value = Activator.CreateInstance(genericType, true);
                    }
                    catch { continue; }
                    ParseXmlNode(value, node);
                    mList.Add(value);
                }
            }
        }
        private static string XmlEncode(string m_Value)
        {
            string retString = string.Empty;
            int start = 0;
            for (int i = 0; i < m_Value.Length; i++)
            {
                string encode = null;
                switch (m_Value[i])
                {
                    case '<':
                        encode = "&lt;";
                        break;
                    case '>':
                        encode = "&gt;";
                        break;
                    case '&':
                        encode = "&amp;";
                        break;
                    case '\'':
                        encode = "&apos;";
                        break;
                    case '"':
                        encode = "&quot;";
                        break;
                }
                if (string.IsNullOrWhiteSpace(encode)) continue;
                retString += m_Value.Substring(start, i - start) + encode;
                start = i + 1;
            }
            if (start < m_Value.Length)
            {
                retString += m_Value.Substring(start, m_Value.Length - start);
            }
            return retString;
        }
        private static string XmlDecode(string m_Value)
        {
            string retString = string.Empty;
            int start = 0;
            for (int i = 0; i < m_Value.Length; i++)
            {
                string decode = null;
                string s = null;
                if (i + 3 < m_Value.Length)
                {
                    s = m_Value.Substring(i, 4);
                    switch (s)
                    {
                        case "&lt;":
                            decode = "<";
                            break;
                        case "&gt;":
                            decode = ">";
                            break;
                        default:
                            if (i + 4 < m_Value.Length)
                            {
                                if ((s = m_Value.Substring(i, 5)) == "&amp;")
                                {
                                    decode = "&";
                                }
                                else if (i + 5 < m_Value.Length)
                                {
                                    s = m_Value.Substring(i, 6);
                                    switch (s)
                                    {
                                        case "&apos;":
                                            decode = "'";
                                            break;
                                        case "&quot;":
                                            decode = "\"";
                                            break;
                                    }
                                }
                            }
                            break;
                    }
                }
                if (string.IsNullOrWhiteSpace(decode)) continue;
                retString += m_Value.Substring(start, i - start) + decode;
                start = i + s.Length;
            }
            if (start < m_Value.Length)
            {
                retString += m_Value.Substring(start, m_Value.Length - start);
            }
            return retString;
        }
    }
}
