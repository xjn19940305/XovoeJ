namespace XovoeJ.Contracts.User
{
    /// <summary>
    /// 用户列表查询参数
    /// </summary>
    public class UserListQueryDto
    {
        /// <summary>
        /// 页码（从1开始）
        /// </summary>
        public int Page { get; set; } = 1;

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; } = 20;

        /// <summary>
        /// 搜索关键字（用户名、昵称、手机号、邮箱）
        /// </summary>
        public string? Keyword { get; set; }

        /// <summary>
        /// 角色名称筛选
        /// </summary>
        public string? RoleName { get; set; }
    }
}
