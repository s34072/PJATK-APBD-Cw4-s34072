using System;

namespace LegacyRenewalApp;

public class FeeCalculator : IFeeCalculator
{
    public (decimal Amount, string Note) CalculateSupportFee(string planCode, bool includePremiumSupport)
    {
        if (!includePremiumSupport) return (0m, string.Empty);

        if (planCode == "START") return (250m, "premium support included; ");
        if (planCode == "PRO") return (400m, "premium support included; ");
        if (planCode == "ENTERPRISE") return (700m, "premium support included; ");

        return (0m, string.Empty);
    }

    public (decimal Amount, string Note) CalculatePaymentFee(string paymentMethod, decimal baseAmountForFee)
    {
        if (paymentMethod == "CARD") return (baseAmountForFee * 0.02m, "card payment fee; ");
        if (paymentMethod == "BANK_TRANSFER") return (baseAmountForFee * 0.01m, "bank transfer fee; ");
        if (paymentMethod == "PAYPAL") return (baseAmountForFee * 0.035m, "paypal fee; ");
        if (paymentMethod == "INVOICE") return (0m, "invoice payment; ");

        throw new ArgumentException("Unsupported payment method");
    }
}
