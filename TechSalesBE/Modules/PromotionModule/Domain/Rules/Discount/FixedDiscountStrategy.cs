using System;
using System.Collections.Generic;
using System.Text;

namespace PromotionModule.Domain.Rules.Discount
{
    internal class FixedDiscountStrategy : IDiscountStrategy
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
                


            float newOrderAmount = totalOrderAmount - voucherValue;
            if(newOrderAmount < 0)
            {
                return Task.FromResult(0f);
            }
            return Task.FromResult(newOrderAmount);
        }
    }
}
