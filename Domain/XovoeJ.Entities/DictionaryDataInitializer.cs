using XovoeJ.Enum;

namespace XovoeJ.Entities
{
    /// <summary>
    /// 字典数据初始化器 - 提供预置的字典配置数据
    /// </summary>
    public static class DictionaryDataInitializer
    {
        /// <summary>
        /// 获取预置的系统配置分组
        /// </summary>
        public static List<(string Code, string Name, string? Description, string? Icon)> GetSystemGroups()
        {
            return new List<(string, string, string?, string?)>
            {
                ("system", "系统配置", "系统基础配置", "setting"),
                ("system.site", "站点配置", "网站基本信息设置", "global"),
                ("system.payment", "支付配置", "支付方式相关配置", "payment"),
                ("system.shipping", "物流配置", "配送方式相关配置", "shipping"),
                ("system.storage", "存储配置", "文件存储相关配置", "folder"),
                ("system.sms", "短信配置", "短信服务相关配置", "message"),
                ("system.email", "邮件配置", "邮件服务相关配置", "mail")
            };
        }

        /// <summary>
        /// 获取预置的业务字典分组
        /// </summary>
        public static List<(string Code, string Name, string? Description, string? Icon)> GetBusinessGroups()
        {
            return new List<(string, string, string?, string?)>
            {
                ("business", "业务字典", "业务相关数据字典", "book"),
                ("business.payment", "支付方式", "支付方式选项", "credit-card"),
                ("business.shipping", "配送方式", "配送方式选项", "truck"),
                ("business.order", "订单来源", "订单来源渠道", "source"),
                ("business.user", "用户等级", "用户会员等级", "user"),
                ("business.region", "地区设置", "国家/地区设置", "location")
            };
        }

        /// <summary>
        /// 获取站点配置的预置字典项
        /// </summary>
        public static List<(string Key, string Name, string? Value, string? DefaultValue, string? Description)> GetSiteConfigItems()
        {
            return new List<(string, string, string?, string?, string?)>
            {
                ("site_name", "网站名称", null, "XovoeJ商城", "网站显示的名称"),
                ("site_title", "网站标题", null, "XovoeJ - 品质电商平台", "网站SEO标题"),
                ("site_keywords", "网站关键词", null, "电商,购物,商城", "网站SEO关键词"),
                ("site_description", "网站描述", null, "XovoeJ品质电商平台，提供优质商品和服务", "网站SEO描述"),
                ("site_logo", "网站Logo", null, null, "网站Logo图片URL"),
                ("site_favicon", "网站图标", null, null, "网站Favicon图标URL"),
                ("site_copyright", "版权信息", null, "© 2025 XovoeJ. All rights reserved.", "网站底部版权信息"),
                ("site_icp", "ICP备案号", null, null, "网站ICP备案号"),
                ("site_enabled", "是否启用站点", "true", "true", "关闭后网站将无法访问"),
                ("site_close_reason", "站点关闭原因", null, "系统维护中，请稍后再来", "站点关闭时显示的提示信息"),
                ("currency", "默认货币", "CNY", "CNY", "网站默认货币类型"),
                ("timezone", "时区设置", "Asia/Shanghai", "Asia/Shanghai", "网站默认时区")
            };
        }

        /// <summary>
        /// 获取支付配置的预置字典项
        /// </summary>
        public static List<(string Key, string Name, string? Value, string? DefaultValue, string? Description)> GetPaymentConfigItems()
        {
            return new List<(string, string, string?, string?, string?)>
            {
                ("alipay_enabled", "支付宝是否启用", "false", "false", "是否启用支付宝支付"),
                ("alipay_appid", "支付宝AppID", null, null, "支付宝应用ID"),
                ("alipay_public_key", "支付宝公钥", null, null, "支付宝公钥"),
                ("alipay_private_key", "支付宝私钥", null, null, "支付宝应用私钥"),
                ("wechatpay_enabled", "微信支付是否启用", "false", "false", "是否启用微信支付"),
                ("wechatpay_appid", "微信AppID", null, null, "微信应用ID"),
                ("wechatpay_mchid", "微信商户号", null, null, "微信支付商户号"),
                ("wechatpay_key", "微信支付密钥", null, null, "微信支付API密钥"),
                ("order_timeout", "订单超时时间(分钟)", "30", "30", "订单未支付自动取消时间"),
                ("refund_auto", "是否自动退款", "false", "false", "订单取消后是否自动退款")
            };
        }

        /// <summary>
        /// 获取支付方式的预置字典项
        /// </summary>
        public static List<(string Key, string Name, string? Value, string? DefaultValue, string? Description)> GetPaymentMethodItems()
        {
            return new List<(string, string, string?, string?, string?)>
            {
                ("alipay", "支付宝", "alipay", "alipay", "支付宝在线支付"),
                ("wechatpay", "微信支付", "wechatpay", "wechatpay", "微信在线支付"),
                ("balance", "余额支付", "balance", "balance", "账户余额支付"),
                ("offline", "货到付款", "offline", "offline", "线下货到付款")
            };
        }

        /// <summary>
        /// 获取配送方式的预置字典项
        /// </summary>
        public static List<(string Key, string Name, string? Value, string? DefaultValue, string? Description)> GetShippingMethodItems()
        {
            return new List<(string, string, string?, string?, string?)>
            {
                ("express", "快递配送", "express", "express", "快递物流配送"),
                ("ems", "邮政EMS", "ems", "ems", "邮政特快专递"),
                ("self_pickup", "门店自提", "self_pickup", "self_pickup", "到店自取"),
                ("city_delivery", "同城配送", "city_delivery", "city_delivery", "同城速递配送")
            };
        }

        /// <summary>
        /// 获取订单来源的预置字典项
        /// </summary>
        public static List<(string Key, string Name, string? Value, string? DefaultValue, string? Description)> GetOrderSourceItems()
        {
            return new List<(string, string, string?, string?, string?)>
            {
                ("web", "PC端", "web", "web", "PC网页端下单"),
                ("h5", "移动端H5", "h5", "h5", "移动网页端下单"),
                ("app", "APP端", "app", "app", "手机APP下单"),
                ("miniprogram", "小程序", "miniprogram", "miniprogram", "微信小程序下单"),
                ("admin", "后台代下单", "admin", "admin", "管理员后台代客下单")
            };
        }

        /// <summary>
        /// 获取用户等级的预置字典项
        /// </summary>
        public static List<(string Key, string Name, string? Value, string? DefaultValue, string? Description)> GetUserLevelItems()
        {
            return new List<(string, string, string?, string?, string?)>
            {
                ("normal", "普通会员", "0", "0", "注册会员"),
                ("silver", "银卡会员", "1", "1", "消费满1000元升级"),
                ("gold", "金卡会员", "2", "2", "消费满5000元升级"),
                ("platinum", "铂金会员", "3", "3", "消费满10000元升级"),
                ("diamond", "钻石会员", "4", "4", "消费满50000元升级")
            };
        }
    }
}
