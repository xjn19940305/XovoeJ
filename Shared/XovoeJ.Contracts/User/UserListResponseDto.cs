namespace XovoeJ.Contracts.User
{
    /// <summary>
    /// 用户列表响应DTO
    /// </summary>
    public class UserListResponseDto
    {
        /// <summary>
        /// 用户列表
        /// </summary>
        public List<UserDto> Items { get; set; } = new();

        /// <summary>
        /// 总记录数
        /// </summary>
        public int Total { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// 每页数量
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    }
}
