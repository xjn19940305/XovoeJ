using System.ComponentModel.DataAnnotations;
using XovoeJ.Enum;

namespace XovoeJ.Contracts.Auth
{
    /// <summary>
    /// 修改个人信息请求DTO
    /// </summary>
    public class UpdateProfileRequestDto
    {
        /// <summary>
        /// 昵称
        /// </summary>
        [MaxLength(128, ErrorMessage = "昵称长度不能超过128个字符")]
        public string? NickName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public GenderEnum? Gender { get; set; }

        /// <summary>
        /// 真实姓名
        /// </summary>
        [MaxLength(128, ErrorMessage = "真实姓名长度不能超过128个字符")]
        public string? RealName { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        [MaxLength(512, ErrorMessage = "头像URL长度不能超过512个字符")]
        public string? Picture { get; set; }

        /// <summary>
        /// 省份
        /// </summary>
        [MaxLength(64, ErrorMessage = "省份长度不能超过64个字符")]
        public string? Province { get; set; }

        /// <summary>
        /// 城市
        /// </summary>
        [MaxLength(64, ErrorMessage = "城市长度不能超过64个字符")]
        public string? City { get; set; }

        /// <summary>
        /// 区域
        /// </summary>
        [MaxLength(64, ErrorMessage = "区域长度不能超过64个字符")]
        public string? Area { get; set; }

        /// <summary>
        /// 详细地址
        /// </summary>
        [MaxLength(256, ErrorMessage = "详细地址长度不能超过256个字符")]
        public string? Address { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? BirthDate { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        [MaxLength(512, ErrorMessage = "备注长度不能超过512个字符")]
        public string? Notes { get; set; }
    }
}
