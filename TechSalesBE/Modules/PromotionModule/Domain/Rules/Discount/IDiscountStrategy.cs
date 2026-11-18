using System;
using System.Collections.Generic;
using System.Text;

namespace PromotionModule.Domain.Rules.Discount
{
    public record OrderInfo(float TotalAmount);
    public record VoucherInfo(float VoucherValue, float MinOrderAmount);
    internal interface IDiscountStrategy
    {
        Task<float> CalculateAsync(OrderInfo orderInfo, VoucherInfo voucherInfo);
    }
}
