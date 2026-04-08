namespace LegacyRenewalApp;

public interface IInvoiceFactory
{
    RenewalInvoice Create(
        Customer customer, 
        string planCode, 
        string paymentMethod, 
        int seatCount, 
        decimal baseAmount, 
        decimal discountAmount, 
        decimal supportFee, 
        decimal paymentFee, 
        decimal taxAmount, 
        decimal finalAmount, 
        string notes);
}