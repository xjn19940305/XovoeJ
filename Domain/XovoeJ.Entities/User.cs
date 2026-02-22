using Microsoft.AspNetCore.Identity;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XovoeJ.Enum;

namespace XovoeJ.Entities
{
    public class User : IdentityUser
    {
        public string? NickName { get; set; }

        /// <summary>
        /// 用户的性别
        /// </summary>
        public GenderEnum? Gender { get; set; }

        public string? RealName { get; set; }
        public string? FirstName { get; set; }

        public string? LastName { get; set; }
        public string? Picture { get; set; }
        public string? Province { get; set; }
        public string? City { get; set; }
        public string? Area { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public DateTime? UpdateAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// 备注
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// 上次登录时间
        /// </summary>
        public DateTime? LastSignDate { get; set; }

        /// <summary>
        /// 是否已完善信息
        /// </summary>
        public bool IsProfileCompleted { get; set; } = false;


    }
}
