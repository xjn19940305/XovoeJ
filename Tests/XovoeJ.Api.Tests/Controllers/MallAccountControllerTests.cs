using System.Reflection;
using XovoeJ.Api.Controllers;
using Xunit;

namespace XovoeJ.Api.Tests.Controllers
{
    public class MallAccountControllerTests
    {
        [Theory]
        [InlineData(0, "普通会员")]
        [InlineData(1000, "白银会员")]
        [InlineData(5000, "黄金会员")]
        [InlineData(10000, "铂金会员")]
        [InlineData(50000, "钻石会员")]
        public void ResolveMemberLevel_ReturnsExpectedChineseLabel(decimal totalSpent, string expected)
        {
            var method = typeof(MallAccountController).GetMethod("ResolveMemberLevel", BindingFlags.NonPublic | BindingFlags.Static);

            var actual = Assert.IsType<string>(method?.Invoke(null, [totalSpent]));

            Assert.Equal(expected, actual);
        }
    }
}
