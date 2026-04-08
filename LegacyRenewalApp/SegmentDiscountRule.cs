namespace LegacyRenewalApp
{
    public class SegmentDiscountRule : IDiscountRule
    {
        public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            if (customer.Segment == "Silver") return (baseAmount * 0.05m, "silver discount; ");
            if (customer.Segment == "Gold") return (baseAmount * 0.10m, "gold discount; ");
            if (customer.Segment == "Platinum") return (baseAmount * 0.15m, "platinum discount; ");
            if (customer.Segment == "Education" && plan.IsEducationEligible) return (baseAmount * 0.20m, "education discount; ");
            
            return (0m, string.Empty);
        }
    }

    public class LoyaltyDiscountRule : IDiscountRule
    {
        public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            if (customer.YearsWithCompany >= 5) return (baseAmount * 0.07m, "long-term loyalty discount; ");
            if (customer.YearsWithCompany >= 2) return (baseAmount * 0.03m, "basic loyalty discount; ");
            
            return (0m, string.Empty);
        }
    }

    public class TeamSizeDiscountRule : IDiscountRule
    {
        public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            if (seatCount >= 50) return (baseAmount * 0.12m, "large team discount; ");
            if (seatCount >= 20) return (baseAmount * 0.08m, "medium team discount; ");
            if (seatCount >= 10) return (baseAmount * 0.04m, "small team discount; ");
            
            return (0m, string.Empty);
        }
    }

    public class LoyaltyPointsDiscountRule : IDiscountRule
    {
        public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints)
        {
            if (useLoyaltyPoints && customer.LoyaltyPoints > 0)
            {
                int pointsToUse = customer.LoyaltyPoints > 200 ? 200 : customer.LoyaltyPoints;
                return (pointsToUse, $"loyalty points used: {pointsToUse}; ");
            }
            
            return (0m, string.Empty);
        }
    }
}