using XovoeJ.Api.Managements;
using Xunit;

namespace XovoeJ.Api.Tests.Management
{
    public class PermissionCatalogTests
    {
        [Fact]
        public void ReturnsChineseNamesWhileKeepingPermissionCodes()
        {
            var items = PermissionCatalog.GetFlatList();

            var marketingMenu = Assert.Single(items, item => item.Code == "admin.marketing");
            var couponPage = Assert.Single(items, item => item.Code == "admin.coupon");
            var couponIssue = Assert.Single(items, item => item.Code == "admin.coupon.issue");

            Assert.Equal("营销中心", marketingMenu.Name);
            Assert.Equal("优惠券中心", couponPage.Name);
            Assert.Equal("优惠券发放", couponIssue.Name);
        }
    }
}
