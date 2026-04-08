namespace LegacyRenewalApp
{
    public interface ITaxCalculator
    {
        decimal CalculateTaxRate(string country);
    }
}