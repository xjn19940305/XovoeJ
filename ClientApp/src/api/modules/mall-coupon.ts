import api, { buildMallApiPath } from '../index'

export default {
  getCouponCenter: (params: Api.Mall.CouponCenterParams) => api.get<Api.Mall.PageResponse<Api.Mall.CouponCenterItem>>(buildMallApiPath('coupons/center'), { params }),
  getCouponDetail: (id: string) => api.get<Api.Mall.CouponCenterItem>(buildMallApiPath(`coupons/available/${id}`)),
  claimCoupon: (id: string) => api.post<Api.Mall.UserCoupon>(buildMallApiPath(`coupons/${id}/claim`)),
  getWallet: (params: Api.Mall.UserCouponParams) => api.get<Api.Mall.PageResponse<Api.Mall.UserCoupon>>(buildMallApiPath('coupons/wallet'), { params }),
}
