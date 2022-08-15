namespace Converter
{
    /// <summary>
    /// </summary>
    [Flags]
    public enum IgnoreFlags
    {
        /// <summary>
        /// 序列化時不生成字串
        /// </summary>
        Serialize = 0x1,
        /// <summary>
        /// 反序列化時不進行解析
        /// </summary>
        Deserialize = 0x2
    }
}
