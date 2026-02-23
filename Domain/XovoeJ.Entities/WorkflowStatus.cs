namespace XovoeJ.Entities
{
    /// <summary>
    /// 工作流实例状态
    /// </summary>
    public enum WorkflowStatus
    {
        /// <summary>
        /// 待提交
        /// </summary>
        Pending = 0,

        /// <summary>
        /// 进行中
        /// </summary>
        InProgress = 1,

        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 2,

        /// <summary>
        /// 已拒绝
        /// </summary>
        Rejected = 3,

        /// <summary>
        /// 已撤回
        /// </summary>
        Withdrawn = 4,

        /// <summary>
        /// 已终止
        /// </summary>
        Terminated = 5
    }
}
