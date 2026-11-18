using System;
using System.Collections.Generic;
using System.Text;

namespace PromotionModule.Domain.Rules.Discount
{
    internal class PercentDiscountStrategy : IDiscountStrategy
    {
        public Task<float> CalculateAsync(OrderInfo orderInfo, VoucherInfo voucherInfo)
        {
            
            float totalOrderAmount = orderInfo.TotalAmount;
            float voucherValue = voucherInfo.VoucherValue;
            float minOrderAmount = voucherInfo.MinOrderAmount;

            if(totalOrderAmount < minOrderAmount)
            {
                return Task.FromResult(totalOrderAmount);
            } 
                


            float newOrderAmount = totalOrderAmount * (1 - voucherValue / 100f);
            return Task.FromResult(newOrderAmount);
        }
    }
}
