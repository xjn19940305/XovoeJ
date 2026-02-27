using System.ComponentModel;

namespace XovoeJ.Enum
{
    /// <summary>
    /// 字典数据类型
    /// </summary>
    public enum DictionaryDataType
    {
        /// <summary>
        /// 字符串
        /// </summary>
        [Description("字符串")]
        String = 0,

        /// <summary>
        /// 数字
        /// </summary>
        [Description("数字")]
        Number = 1,

        /// <summary>
        /// 布尔值
        /// </summary>
        [Description("布尔值")]
        Boolean = 2,

        /// <summary>
        /// JSON对象
        /// </summary>
        [Description("JSON对象")]
        Json = 3,

        /// <summary>
        /// 日期时间
        /// </summary>
        [Description("日期时间")]
        DateTime = 4,

        /// <summary>
        /// 富文本/HTML
        /// </summary>
        [Description("富文本")]
        Html = 5
    }
}
