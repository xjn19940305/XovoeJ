using Microsoft.EntityFrameworkCore;
using XovoeJ.Abstractions.Services;
using XovoeJ.Contracts.Payment;
using XovoeJ.Entities;
using XovoeJ.Enum;
using XovoeJ.Persistence.PostgreSql;

namespace XovoeJ.Application.Services
{
    /// <summary>
    /// 支付服务实现。
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly XovoeJDbContext _dbContext;
        private readonly IAssetAccountService _assetAccountService;
        private readonly IAssetLedgerService _assetLedgerService;
        private readonly IOrderService _orderService;

        public PaymentService(
            XovoeJDbContext dbContext,
            IAssetAccountService assetAccountService,
            IAssetLedgerService assetLedgerService,
            IOrderService orderService)
        {
            _dbContext = dbContext;
            _assetAccountService = assetAccountService;
            _assetLedgerService = assetLedgerService;
            _orderService = orderService;
        }

        public async Task<PaymentOrderDto> CreatePaymentOrderAsync(string userId, string orderId)
        {
            var order = await _dbContext.Orders
                .AsTracking()
                .FirstOrDefaultAsync(item => item.Id == orderId && item.UserId == userId);

            if (order == null)
            {
                throw new ArgumentException("订单不存在");
            }

            var paymentOrder = await EnsurePaymentOrderAsync(order);
            return await MapToDtoAsync(paymentOrder);
        }

        public async Task<PaymentOrderDto?> GetPaymentOrderAsync(string userId, string paymentOrderNo)
        {
            var paymentOrder = await _dbContext.PaymentOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.PaymentOrderNo == paymentOrderNo && item.UserId == userId);

            return paymentOrder == null ? null : await MapToDtoAsync(paymentOrder);
        }

        public async Task<PaymentOrderDto?> GetPaymentOrderByOrderIdAsync(string userId, string orderId)
        {
            var paymentOrder = await _dbContext.PaymentOrders
                .AsNoTracking()
                .FirstOrDefaultAsync(item => item.OrderId == orderId && item.UserId == userId);

            return paymentOrder == null ? null : await MapToDtoAsync(paymentOrder);
        }

        public async Task<PaymentOrderDto> PayByWalletAsync(string userId, string paymentOrderNo)
        {
            await _assetAccountService.EnsureUserAssetsAsync(userId);

            var paymentOrder = await _dbContext.PaymentOrders
                .AsTracking()
                .FirstOrDefaultAsync(item => item.PaymentOrderNo == paymentOrderNo && item.UserId == userId);

            if (paymentOrder == null)
            {
                throw new ArgumentException("支付单不存在");
            }

            if (paymentOrder.Status == PaymentOrderStatus.Paid
                || paymentOrder.Status == PaymentOrderStatus.PartiallyRefunded
                || paymentOrder.Status == PaymentOrderStatus.Refunded)
            {
                return await MapToDtoAsync(paymentOrder);
            }

            if (paymentOrder.Status != PaymentOrderStatus.Pending)
            {
                throw new ArgumentException("当前支付单不可支付");
            }

            if (paymentOrder.ExpireAt.HasValue && paymentOrder.ExpireAt.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("支付单已过期");
            }

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                await _assetLedgerService.DeductWalletAsync(
                    userId,
                    paymentOrder.PayableAmount,
                    "order_pay",
                    paymentOrder.OrderNo,
                    $"wallet-pay-{paymentOrder.PaymentOrderNo}",
                    "钱包支付订单");

                paymentOrder.PaymentMethod = "wallet";
                paymentOrder.PaidAmount = paymentOrder.PayableAmount;
                paymentOrder.Status = PaymentOrderStatus.Paid;
                paymentOrder.PaidAt = DateTime.UtcNow;
                paymentOrder.UpdatedAt = DateTime.UtcNow;
                paymentOrder.FailureReason = null;

                await _dbContext.SaveChangesAsync();
                await _orderService.PayOrderAsync(
                    paymentOrder.OrderId,
                    "wallet",
                    paymentOrder.Id,
                    paymentOrder.PaymentOrderNo,
                    paymentOrder.PaidAmount);

                await transaction.CommitAsync();
                return await MapToDtoAsync(paymentOrder);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> ClosePaymentOrderAsync(string paymentOrderNo)
        {
            var paymentOrder = await _dbContext.PaymentOrders
                .AsTracking()
                .FirstOrDefaultAsync(item => item.PaymentOrderNo == paymentOrderNo);

            if (paymentOrder == null)
            {
                return false;
            }

            if (paymentOrder.Status != PaymentOrderStatus.Pending)
            {
                throw new ArgumentException("只有待支付支付单可以关闭");
            }

            paymentOrder.Status = PaymentOrderStatus.Closed;
            paymentOrder.ClosedAt = DateTime.UtcNow;
            paymentOrder.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task RefundOrderAsync(Order order, decimal refundAmount, string businessNo, string? remark = null)
        {
            if (refundAmount <= 0)
            {
                return;
            }

            var paymentOrder = await _dbContext.PaymentOrders
                .AsTracking()
                .FirstOrDefaultAsync(item => item.OrderId == order.Id);

            if (paymentOrder == null)
            {
                return;
            }

            var remainingRefundable = paymentOrder.PaidAmount - paymentOrder.RefundedAmount;
            if (remainingRefundable <= 0)
            {
                return;
            }

            if (refundAmount > remainingRefundable)
            {
                throw new ArgumentException("退款金额超过可退金额");
            }

            if (string.Equals(paymentOrder.PaymentMethod, "wallet", StringComparison.OrdinalIgnoreCase))
            {
                await _assetLedgerService.RefundWalletAsync(
                    order.UserId,
                    refundAmount,
                    "order_refund",
                    businessNo,
                    $"wallet-refund-{businessNo}",
                    remark ?? "售后退款回退钱包");
            }

            if (order.PointsUsed > 0)
            {
                await _assetLedgerService.RefundPointsAsync(
                    order.UserId,
                    order.PointsUsed,
                    "order_points_refund",
                    businessNo,
                    $"points-refund-{businessNo}",
                    "订单退款返还抵扣积分");
            }

            if (order.RewardPoints > 0 && order.RefundedAmount + refundAmount >= order.PayAmount)
            {
                await _assetLedgerService.DeductPointsAsync(
                    order.UserId,
                    order.RewardPoints,
                    "order_reward_revoke",
                    businessNo,
                    $"points-revoke-{order.OrderNo}",
                    "订单全额退款冲销奖励积分");
            }

            paymentOrder.RefundedAmount += refundAmount;
            paymentOrder.Status = paymentOrder.RefundedAmount >= paymentOrder.PaidAmount
                ? PaymentOrderStatus.Refunded
                : PaymentOrderStatus.PartiallyRefunded;
            paymentOrder.UpdatedAt = DateTime.UtcNow;
            order.RefundedAmount += refundAmount;
            order.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        private async Task<PaymentOrder> EnsurePaymentOrderAsync(Order order)
        {
            var existing = await _dbContext.PaymentOrders
                .AsTracking()
                .FirstOrDefaultAsync(item => item.OrderId == order.Id);

            if (existing != null)
            {
                if (string.IsNullOrWhiteSpace(order.PaymentOrderId) || string.IsNullOrWhiteSpace(order.PaymentOrderNo))
                {
                    order.PaymentOrderId = existing.Id;
                    order.PaymentOrderNo = existing.PaymentOrderNo;
                    order.UpdatedAt = DateTime.UtcNow;
                    await _dbContext.SaveChangesAsync();
                }

                return existing;
            }

            var paymentOrder = new PaymentOrder
            {
                OrderId = order.Id,
                OrderNo = order.OrderNo,
                UserId = order.UserId,
                PaymentOrderNo = GeneratePaymentOrderNo(),
                PayableAmount = order.PayAmount,
                PaidAmount = 0m,
                RefundedAmount = 0m,
                Status = PaymentOrderStatus.Pending,
                ExpireAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            _dbContext.PaymentOrders.Add(paymentOrder);
            await _dbContext.SaveChangesAsync();

            order.PaymentOrderId = paymentOrder.Id;
            order.PaymentOrderNo = paymentOrder.PaymentOrderNo;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return paymentOrder;
        }

        private async Task<PaymentOrderDto> MapToDtoAsync(PaymentOrder paymentOrder)
        {
            await _assetAccountService.EnsureUserAssetsAsync(paymentOrder.UserId);

            var wallet = await _dbContext.WalletAccounts
                .AsNoTracking()
                .FirstAsync(item => item.UserId == paymentOrder.UserId);

            var points = await _dbContext.PointsAccounts
                .AsNoTracking()
                .FirstAsync(item => item.UserId == paymentOrder.UserId);

            return new PaymentOrderDto
            {
                Id = paymentOrder.Id,
                PaymentOrderNo = paymentOrder.PaymentOrderNo,
                OrderId = paymentOrder.OrderId,
                OrderNo = paymentOrder.OrderNo,
                UserId = paymentOrder.UserId,
                PaymentMethod = paymentOrder.PaymentMethod,
                PayableAmount = paymentOrder.PayableAmount,
                PaidAmount = paymentOrder.PaidAmount,
                RefundedAmount = paymentOrder.RefundedAmount,
                Status = paymentOrder.Status,
                StatusDescription = paymentOrder.Status.ToString(),
                ExpireAt = paymentOrder.ExpireAt,
                PaidAt = paymentOrder.PaidAt,
                ClosedAt = paymentOrder.ClosedAt,
                FailureReason = paymentOrder.FailureReason,
                WalletAvailableBalance = wallet.AvailableBalance,
                AvailablePoints = points.AvailablePoints,
                CreatedAt = paymentOrder.CreatedAt,
            };
        }

        private static string GeneratePaymentOrderNo()
        {
            return $"PAY{DateTime.UtcNow:yyyyMMddHHmmssfff}{Guid.NewGuid():N}"[..27];
        }
    }
}
