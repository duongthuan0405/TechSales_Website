using PromotionModule.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace PromotionModule.Domain.Rules.Discount
{
    internal static class DiscountStrategyResolver
    {
        public static IDiscountStrategy Resolve(VoucherType type)
        {
            switch(type)
            {
                case VoucherType.FIXED:
                    return new FixedDiscountStrategy();
                case VoucherType.PERCENT:
                    return new PercentDiscountStrategy();
                default:
                    throw new ArgumentException($"Voucher Type {type.ToString()} is not provided strategy!");
            }    
        }
    }
}
