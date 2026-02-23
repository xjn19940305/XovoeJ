using System.ComponentModel;

namespace XovoeJ.Enum
{
    /// <summary>
    /// 字典类型
    /// </summary>
    public enum DictionaryType
    {
        /// <summary>
        /// 系统配置 - 用于存储系统级别的配置项
        /// </summary>
        [Description("系统配置")]
        System = 0,

        /// <summary>
        /// 业务字典 - 用于存储业务相关的下拉选项、状态值等
        /// </summary>
        [Description("业务字典")]
        Business = 1
    }
}
