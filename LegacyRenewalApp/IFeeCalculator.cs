namespace LegacyRenewalApp;

public interface IFeeCalculator
{
    (decimal Amount, string Note) CalculateSupportFee(string planCode, bool includePremiumSupport);
    (decimal Amount, string Note) CalculatePaymentFee(string paymentMethod, decimal baseAmountForFee);
}