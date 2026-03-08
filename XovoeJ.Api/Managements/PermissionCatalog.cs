namespace XovoeJ.Api.Managements
{
    public static class PermissionCatalog
    {
        public static IReadOnlyList<PermissionCatalogItem> GetTree()
        {
            return PermissionTree;
        }

        public static IReadOnlyList<PermissionCatalogItem> GetFlatList()
        {
            return Flatten(PermissionTree);
        }

        private static List<PermissionCatalogItem> Flatten(IEnumerable<PermissionCatalogItem> nodes)
        {
            var result = new List<PermissionCatalogItem>();
            foreach (var node in nodes)
            {
                result.Add(node with { Children = [] });
                if (node.Children.Count > 0)
                {
                    result.AddRange(Flatten(node.Children));
                }
            }

            return result;
        }

        private static readonly IReadOnlyList<PermissionCatalogItem> PermissionTree =
        [
            Node(
                1,
                0,
                "权限管理",
                "admin.system",
                "menu",
                children:
                [
                    Node(
                        101,
                        1,
                        "用户管理",
                        "admin.user",
                        "page",
                        children:
                        [
                            Node(10101, 101, "用户查看", "admin.user.read", "button"),
                            Node(10102, 101, "用户创建", "admin.user.create", "button"),
                            Node(10103, 101, "用户编辑", "admin.user.update", "button"),
                            Node(10104, 101, "用户删除", "admin.user.delete", "button"),
                        ]),
                    Node(
                        102,
                        1,
                        "角色管理",
                        "admin.role",
                        "page",
                        children:
                        [
                            Node(10201, 102, "角色查看", "admin.role.read", "button"),
                            Node(10202, 102, "角色创建", "admin.role.create", "button"),
                            Node(10203, 102, "角色编辑", "admin.role.update", "button"),
                            Node(10204, 102, "角色删除", "admin.role.delete", "button"),
                            Node(10205, 102, "角色授权", "admin.role.permission.assign", "button"),
                        ]),
                ]),
            Node(
                2,
                0,
                "商城管理",
                "admin.mall",
                "menu",
                children:
                [
                    Node(
                        201,
                        2,
                        "商品管理",
                        "admin.product",
                        "page",
                        children:
                        [
                            Node(20101, 201, "商品查看", "admin.product.read", "button"),
                            Node(20102, 201, "商品创建", "admin.product.create", "button"),
                            Node(20103, 201, "商品编辑", "admin.product.update", "button"),
                            Node(20104, 201, "商品删除", "admin.product.delete", "button"),
                        ]),
                    Node(
                        202,
                        2,
                        "商品分类",
                        "admin.category",
                        "page",
                        children:
                        [
                            Node(20201, 202, "分类查看", "admin.category.read", "button"),
                            Node(20202, 202, "分类创建", "admin.category.create", "button"),
                            Node(20203, 202, "分类编辑", "admin.category.update", "button"),
                            Node(20204, 202, "分类删除", "admin.category.delete", "button"),
                        ]),
                    Node(
                        203,
                        2,
                        "订单管理",
                        "admin.order",
                        "page",
                        children:
                        [
                            Node(20301, 203, "订单查看", "admin.order.read", "button"),
                            Node(20302, 203, "订单创建", "admin.order.create", "button"),
                            Node(20303, 203, "订单编辑", "admin.order.update", "button"),
                            Node(20304, 203, "订单删除", "admin.order.delete", "button"),
                        ]),
                    Node(
                        204,
                        2,
                        "售后管理",
                        "admin.aftersale",
                        "page",
                        children:
                        [
                            Node(20401, 204, "售后查看", "admin.aftersale.read", "button"),
                            Node(20402, 204, "售后处理", "admin.aftersale.manage", "button"),
                        ]),
                ]),
            Node(
                3,
                0,
                "内容管理",
                "admin.content",
                "menu",
                children:
                [
                    Node(
                        301,
                        3,
                        "轮播图管理",
                        "admin.banner",
                        "page",
                        children:
                        [
                            Node(30101, 301, "轮播图查看", "admin.banner.read", "button"),
                            Node(30102, 301, "轮播图创建", "admin.banner.create", "button"),
                            Node(30103, 301, "轮播图编辑", "admin.banner.update", "button"),
                            Node(30104, 301, "轮播图删除", "admin.banner.delete", "button"),
                        ]),
                    Node(
                        302,
                        3,
                        "字典管理",
                        "admin.dictionary",
                        "page",
                        children:
                        [
                            Node(30201, 302, "字典查看", "admin.dictionary.read", "button"),
                            Node(30202, 302, "字典创建", "admin.dictionary.create", "button"),
                            Node(30203, 302, "字典编辑", "admin.dictionary.update", "button"),
                            Node(30204, 302, "字典删除", "admin.dictionary.delete", "button"),
                        ]),
                    Node(
                        303,
                        3,
                        "工作流配置",
                        "admin.workflow",
                        "page",
                        children:
                        [
                            Node(30301, 303, "工作流查看", "admin.workflow.read", "button"),
                            Node(30302, 303, "工作流创建", "admin.workflow.create", "button"),
                            Node(30303, 303, "工作流编辑", "admin.workflow.update", "button"),
                            Node(30304, 303, "工作流删除", "admin.workflow.delete", "button"),
                        ]),
                ]),
            Node(
                4,
                0,
                "营销中心",
                "admin.marketing",
                "menu",
                children:
                [
                    Node(
                        401,
                        4,
                        "优惠券中心",
                        "admin.coupon",
                        "page",
                        children:
                        [
                            Node(40101, 401, "优惠券查看", "admin.coupon.read", "button"),
                            Node(40102, 401, "优惠券发放", "admin.coupon.issue", "button"),
                            Node(40103, 401, "优惠券管理", "admin.coupon.manage", "button"),
                        ]),
                    Node(
                        402,
                        4,
                        "营销活动",
                        "admin.promotion",
                        "page",
                        children:
                        [
                            Node(40201, 402, "活动查看", "admin.promotion.read", "button"),
                            Node(40202, 402, "活动管理", "admin.promotion.manage", "button"),
                        ]),
                    Node(
                        403,
                        4,
                        "秒杀活动",
                        "admin.marketing.seckill",
                        "page",
                        children:
                        [
                            Node(40301, 403, "秒杀管理", "admin.marketing.seckill.manage", "button"),
                        ]),
                    Node(
                        404,
                        4,
                        "拼团活动",
                        "admin.marketing.group-buy",
                        "page",
                        children:
                        [
                            Node(40401, 404, "拼团管理", "admin.marketing.group-buy.manage", "button"),
                        ]),
                    Node(
                        405,
                        4,
                        "砍价活动",
                        "admin.marketing.bargain",
                        "page",
                        children:
                        [
                            Node(40501, 405, "砍价管理", "admin.marketing.bargain.manage", "button"),
                        ]),
                ]),
            Node(
                5,
                0,
                "增长中心",
                "admin.growth",
                "menu",
                children:
                [
                    Node(
                        501,
                        5,
                        "分销与邀请",
                        "admin.distribution",
                        "page",
                        children:
                        [
                            Node(50101, 501, "分销查看", "admin.distribution.read", "button"),
                            Node(50102, 501, "分销管理", "admin.distribution.manage", "button"),
                        ]),
                    Node(
                        502,
                        5,
                        "推广链接",
                        "admin.referral-link",
                        "page",
                        children:
                        [
                            Node(50201, 502, "推广链接查看", "admin.referral-link.read", "button"),
                            Node(50202, 502, "推广链接管理", "admin.referral-link.manage", "button"),
                        ]),
                    Node(
                        503,
                        5,
                        "佣金结算",
                        "admin.commission",
                        "page",
                        children:
                        [
                            Node(50301, 503, "佣金查看", "admin.commission.read", "button"),
                            Node(50302, 503, "佣金结算", "admin.commission.settle", "button"),
                        ]),
                ]),
            Node(
                6,
                0,
                "消息中心",
                "admin.message",
                "menu",
                children:
                [
                    Node(
                        601,
                        6,
                        "消息模板",
                        "admin.message.template",
                        "page",
                        children:
                        [
                            Node(60101, 601, "消息模板查看", "admin.message.template.read", "button"),
                            Node(60102, 601, "消息模板管理", "admin.message.template.manage", "button"),
                        ]),
                    Node(
                        602,
                        6,
                        "消息任务",
                        "admin.message.task",
                        "page",
                        children:
                        [
                            Node(60201, 602, "消息任务查看", "admin.message.task.read", "button"),
                            Node(60202, 602, "消息任务发送", "admin.message.task.send", "button"),
                        ]),
                    Node(
                        603,
                        6,
                        "发送记录",
                        "admin.message.record",
                        "page",
                        children:
                        [
                            Node(60301, 603, "发送记录查看", "admin.message.record.read", "button"),
                        ]),
                ]),
        ];

        private static PermissionCatalogItem Node(
            int id,
            int parentId,
            string name,
            string code,
            string type,
            string? path = null,
            string? icon = null,
            int sort = 0,
            int status = 1,
            IReadOnlyList<PermissionCatalogItem>? children = null)
        {
            return new PermissionCatalogItem(
                id,
                parentId,
                name,
                code,
                type,
                path,
                icon,
                sort,
                status,
                children ?? []);
        }
    }

    public sealed record PermissionCatalogItem(
        int Id,
        int ParentId,
        string Name,
        string Code,
        string Type,
        string? Path,
        string? Icon,
        int Sort,
        int Status,
        IReadOnlyList<PermissionCatalogItem> Children);
}
