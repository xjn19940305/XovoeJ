export const permissionCodes = {
  admin: {
    system: 'admin.system',
    mall: 'admin.mall',
    content: 'admin.content',
    marketing: 'admin.marketing',
    growth: 'admin.growth',
    message: 'admin.message',
    user: {
      page: 'admin.user',
      read: 'admin.user.read',
      create: 'admin.user.create',
      update: 'admin.user.update',
      delete: 'admin.user.delete',
    },
    role: {
      page: 'admin.role',
      read: 'admin.role.read',
      create: 'admin.role.create',
      update: 'admin.role.update',
      delete: 'admin.role.delete',
      assignPermissions: 'admin.role.permission.assign',
    },
    product: {
      page: 'admin.product',
      read: 'admin.product.read',
      create: 'admin.product.create',
      update: 'admin.product.update',
      delete: 'admin.product.delete',
    },
    category: {
      page: 'admin.category',
      read: 'admin.category.read',
      create: 'admin.category.create',
      update: 'admin.category.update',
      delete: 'admin.category.delete',
    },
    order: {
      page: 'admin.order',
      read: 'admin.order.read',
      create: 'admin.order.create',
      update: 'admin.order.update',
      delete: 'admin.order.delete',
    },
    payment: {
      page: 'admin.payment',
      read: 'admin.payment.read',
      close: 'admin.payment.close',
    },
    aftersale: {
      page: 'admin.aftersale',
      read: 'admin.aftersale.read',
      manage: 'admin.aftersale.manage',
    },
    assetWallet: {
      page: 'admin.asset.wallet',
      read: 'admin.asset.wallet.read',
    },
    assetWalletLog: {
      page: 'admin.asset.wallet-log',
      read: 'admin.asset.wallet-log.read',
    },
    assetPoints: {
      page: 'admin.asset.points',
      read: 'admin.asset.points.read',
    },
    assetPointsLog: {
      page: 'admin.asset.points-log',
      read: 'admin.asset.points-log.read',
    },
    banner: {
      page: 'admin.banner',
      read: 'admin.banner.read',
      create: 'admin.banner.create',
      update: 'admin.banner.update',
      delete: 'admin.banner.delete',
    },
    dictionary: {
      page: 'admin.dictionary',
      read: 'admin.dictionary.read',
      create: 'admin.dictionary.create',
      update: 'admin.dictionary.update',
      delete: 'admin.dictionary.delete',
    },
    workflow: {
      page: 'admin.workflow',
      read: 'admin.workflow.read',
      create: 'admin.workflow.create',
      update: 'admin.workflow.update',
      delete: 'admin.workflow.delete',
    },
    coupon: {
      page: 'admin.coupon',
      read: 'admin.coupon.read',
      issue: 'admin.coupon.issue',
      manage: 'admin.coupon.manage',
    },
    promotion: {
      page: 'admin.promotion',
      read: 'admin.promotion.read',
      manage: 'admin.promotion.manage',
    },
    seckill: {
      page: 'admin.marketing.seckill',
      manage: 'admin.marketing.seckill.manage',
    },
    groupBuy: {
      page: 'admin.marketing.group-buy',
      manage: 'admin.marketing.group-buy.manage',
    },
    bargain: {
      page: 'admin.marketing.bargain',
      manage: 'admin.marketing.bargain.manage',
    },
    distribution: {
      page: 'admin.distribution',
      read: 'admin.distribution.read',
      manage: 'admin.distribution.manage',
    },
    referralLink: {
      page: 'admin.referral-link',
      read: 'admin.referral-link.read',
      manage: 'admin.referral-link.manage',
    },
    commission: {
      page: 'admin.commission',
      read: 'admin.commission.read',
      settle: 'admin.commission.settle',
    },
    messageTemplate: {
      page: 'admin.message.template',
      read: 'admin.message.template.read',
      manage: 'admin.message.template.manage',
    },
    messageTask: {
      page: 'admin.message.task',
      read: 'admin.message.task.read',
      send: 'admin.message.task.send',
    },
    messageRecord: {
      page: 'admin.message.record',
      read: 'admin.message.record.read',
    },
  },
} as const

export function hasPermission(userPermissions: string[], permission: string) {
  return userPermissions.includes('*') || userPermissions.includes(permission)
}

export function hasAnyPermission(userPermissions: string[], permissions: string[]) {
  return permissions.some(permission => hasPermission(userPermissions, permission))
}
