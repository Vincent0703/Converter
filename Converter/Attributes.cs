namespace Converter
{
    /// <summary>
    /// Tag順序
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DisplayOrderAttribute : Attribute
    {
        /// <summary>
        /// 顯示次序
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// </summary>
        public DisplayOrderAttribute() { }
        /// <summary>
        /// </summary>
        /// <param name="mIndex">顯示次序</param>
        public DisplayOrderAttribute(int mIndex)
        {
            Index = mIndex;
        }
    }
    /// <summary>
    /// 轉換成字串時，布林值以1/0表示(未繼承 布林值以True/False表示)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class BooleanToIntAttribute : Attribute { }
    /// <summary>
    /// 轉換成字串時，Enum型態資料以Int型態表示(未繼承 Enum以字串表示)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EnumToIntAttribute : Attribute { }
    /// <summary>
    /// 轉換成字串時，字串項目為Null或空值仍會建立該項目(未繼承 若字串為Null或空值則不會建立該項目)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DisplayEmptyItemAttribute : Attribute { }
    /// <summary>
    /// 標記特定 Property 不進行序列化或反序列化處理
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class IgnoreItemAttribute : Attribute 
    {
        private IgnoreFlags cvIgnoreFlags = IgnoreFlags.Serialize | IgnoreFlags.Deserialize;
        /// <summary>
        /// 忽略處理旗標
        /// </summary>
        public IgnoreFlags IgnoreFlags
        {
            get { return cvIgnoreFlags; }
            set 
            {
                if (value == 0) return;
                cvIgnoreFlags = value;
            }
        }
        /// <summary>
        /// </summary>
        /// <param name="mIgnoreFlags">忽略處理旗標</param>
        public IgnoreItemAttribute(IgnoreFlags mIgnoreFlags = IgnoreFlags.Serialize | IgnoreFlags.Deserialize)
        {
            IgnoreFlags = mIgnoreFlags;
        }
    }

    /// <summary>
    /// Tag / PropertyName 間的名稱映射
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
    public sealed class TagRenameAttribute : Attribute
    {
        /// <summary>
        /// 標籤名稱
        /// </summary>
        public string TagName { get; set; }
        /// <summary>
        /// </summary>
        public TagRenameAttribute() { }
        /// <summary>
        /// </summary>
        /// <param name="mTagName">標籤名稱</param>
        public TagRenameAttribute(string mTagName)
        {
            TagName = mTagName;
        }
    }
    /// <summary>
    /// DateTime / Timespan 字串格式
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DateTimeFormatAttribute : Attribute
    {
        /// <summary>
        /// 日期時間格式
        /// </summary>
        public string Format { get; set; }
        /// <summary>
        /// </summary>
        public DateTimeFormatAttribute() { }
        /// <summary>
        /// </summary>
        /// <param name="mFormat">日期時間格式</param>
        public DateTimeFormatAttribute(string mFormat)
        {
            Format = mFormat;
        }
    }
    #region Xml Attributes
    /// <summary>
    /// 標記特定 Property 為 Xml 中的屬性項目
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class XmlAttributeAttribute : System.Xml.Serialization.XmlAttributeAttribute { }
    /// <summary>
    /// 標記特定 Property 為 Xml 中的內容
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class XmlContentAttribute : Attribute { }
    /// <summary>
    /// Xml陣列各物件標籤
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class XmlArrayAttribute : Attribute
    {
        /// <summary>
        /// 子項目標籤名稱
        /// </summary>
        public string ChildTagName { get; set; }
        /// <summary>
        /// </summary>
        public XmlArrayAttribute() { }
        /// <summary>
        /// </summary>
        /// <param name="mChildTagName">子項目標籤名稱</param>
        public XmlArrayAttribute(string mChildTagName)
        {
            ChildTagName = mChildTagName;
        }
    }
    #endregion
}