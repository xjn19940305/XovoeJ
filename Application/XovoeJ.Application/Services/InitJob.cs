using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XovoeJ.Entities;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    public class InitJob
    {
        UserManager<User> userManage;
        XovoeJDbContext dbContext;
        public async Task Init(IServiceScope scope)
        {
            userManage = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            dbContext = scope.ServiceProvider.GetRequiredService<XovoeJDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

            var pendingMigrations = await dbContext.Database.GetPendingMigrationsAsync();

            if (!await dbContext.Database.CanConnectAsync())
            {
                await dbContext.Database.MigrateAsync();
                await CreateRole(roleManager);
                await CreateUser();
                await ImportMasterData();
            }
            else if (pendingMigrations.Any())
            {
                await dbContext.Database.MigrateAsync();
            }


        }

        async Task ImportMasterData()
        {
            dbContext.Database.SetCommandTimeout(1800);
            var sqlScriptsDir = Path.Combine(AppContext.BaseDirectory, "sqlscripts");

            if (!Directory.Exists(sqlScriptsDir))
            {
                // 尝试相对路径
                sqlScriptsDir = "sqlscripts";
                if (!Directory.Exists(sqlScriptsDir)) return;
            }

            foreach (var file in Directory.GetFiles(sqlScriptsDir, "*.sql"))
            {
                try
                {
                    string sql = await File.ReadAllTextAsync(file);
                    // 移除 SQL 注释和空行，然后按分号分割
                    var statements = sql.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(s => s.Trim())
                        .Where(s => !string.IsNullOrWhiteSpace(s) &&
                                    !s.StartsWith("--") &&
                                    !s.StartsWith("/*") &&
                                    !s.StartsWith("DROP TABLE", StringComparison.OrdinalIgnoreCase)) // 跳过 DROP 语句，避免重复执行报错
                        .ToList();

                    foreach (var statement in statements)
                    {
                        if (!string.IsNullOrWhiteSpace(statement))
                        {
                            await dbContext.Database.ExecuteSqlRawAsync(statement);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // 记录错误但继续执行其他脚本
                    Console.WriteLine($"执行 SQL 脚本失败: {Path.GetFileName(file)}, 错误: {ex.Message}");
                }
            }
        }

        async Task CreateUser()
        {
            await AddSysAccount("milo", ["超级管理员"], "管理员账号");
        }

        private async Task AddSysAccount(string userName, string[] roleNames, string? realName = null)
        {
            var user = await userManage.FindByNameAsync(userName);
            if (user == null)
            {
                user = new User
                {
                    NickName = realName ?? userName,
                    RealName = realName ?? userName,
                };
                if (!string.IsNullOrEmpty(userName))
                {
                    await userManage.SetPhoneNumberAsync(user, userName);
                    await userManage.SetUserNameAsync(user, userName);
                }

                await userManage.AddPasswordAsync(user, "123456");

                var result = await userManage.CreateAsync(user);
                if (!result.Succeeded)
                    throw new Exception(JsonConvert.SerializeObject(result.Errors));
                await userManage.SetLockoutEnabledAsync(user, false);
                await dbContext.SaveChangesAsync();
            }
            foreach (var roleName in roleNames)
            {
                if (!await userManage.IsInRoleAsync(user, roleName))
                {
                    await userManage.AddToRoleAsync(user, roleName);
                }
            }
        }

        async Task CreateRole(RoleManager<Role> roleManager)
        {
            // 创建超级管理员角色
            var role = await roleManager.FindByNameAsync("超级管理员");
            if (role == null)
            {
                role = new Role { Name = "超级管理员", Sort = 10, Description = "系统内置管理员，拥有所有权限" };
                await roleManager.CreateAsync(role);
            }

            // 检查并添加管理员权限声明
            var adminPermissions = new[]
            {
                "*", // 通配符，表示所有权限
            };

            var existingClaims = await roleManager.GetClaimsAsync(role);
            foreach (var permission in adminPermissions)
            {
                if (!existingClaims.Any(c => c.Type == "permission" && c.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new System.Security.Claims.Claim("permission", permission));
                }
            }

            // 创建前端用户角色
            role = await roleManager.FindByNameAsync("前端用户");
            if (role == null)
            {
                role = new Role { Name = "前端用户", Sort = 30, Description = "前端用户" };
                await roleManager.CreateAsync(role);
            }

            // 创建后端用户角色
            role = await roleManager.FindByNameAsync("后端用户");
            if (role == null)
            {
                role = new Role { Name = "后端用户", Sort = 40, Description = "后端用户" };
                await roleManager.CreateAsync(role);
            }
        }
    }
}
