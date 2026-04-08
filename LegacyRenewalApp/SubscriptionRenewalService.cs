using System;
using System.Collections.Generic;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IBillingGateway _billingGateway;
        private readonly IEnumerable<IDiscountRule> _discountRules;
        private readonly ITaxCalculator _taxCalculator;
        private readonly IFeeCalculator _feeCalculator;
        private readonly IInvoiceFactory _invoiceFactory;
        
        public SubscriptionRenewalService()
            : this(new CustomerRepository(), 
                new SubscriptionPlanRepository(), 
                new BillingGatewayAdapter(),
                new List<IDiscountRule>
                {
                    new SegmentDiscountRule(),
                    new LoyaltyDiscountRule(),
                    new TeamSizeDiscountRule(),
                    new LoyaltyPointsDiscountRule()
                },
                new TaxCalculator(),
                new FeeCalculator(),
                new InvoiceFactory())
        {
        }
        
        public SubscriptionRenewalService(
            ICustomerRepository customerRepository, 
            ISubscriptionPlanRepository planRepository,
            IBillingGateway billingGateway,
            IEnumerable<IDiscountRule> discountRules,
            ITaxCalculator taxCalculator,
            IFeeCalculator feeCalculator,
            IInvoiceFactory invoiceFactory)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _billingGateway = billingGateway;
            _discountRules = discountRules;
            _taxCalculator = taxCalculator;
            _feeCalculator = feeCalculator;
            _invoiceFactory = invoiceFactory;
        }
        
        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            if (customerId <= 0)
            {
                throw new ArgumentException("Customer id must be positive");
            }

            if (string.IsNullOrWhiteSpace(planCode))
            {
                throw new ArgumentException("Plan code is required");
            }

            if (seatCount <= 0)
            {
                throw new ArgumentException("Seat count must be positive");
            }

            if (string.IsNullOrWhiteSpace(paymentMethod))
            {
                throw new ArgumentException("Payment method is required");
            }

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();

            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }

            decimal baseAmount = plan.CalculateBaseAmount(seatCount);
            decimal discountAmount = 0m;
            string notes = string.Empty;

            foreach (var rule in _discountRules)
            {
                var result = rule.Calculate(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
                discountAmount += result.Amount;
                notes += result.Note;
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                notes += "minimum discounted subtotal applied; ";
            }

            //OPŁATY DODATKOWE
            var supportFeeResult = _feeCalculator.CalculateSupportFee(normalizedPlanCode, includePremiumSupport);
            decimal supportFee = supportFeeResult.Amount;
            notes += supportFeeResult.Note;

            var paymentFeeResult = _feeCalculator.CalculatePaymentFee(normalizedPaymentMethod, subtotalAfterDiscount + supportFee);
            decimal paymentFee = paymentFeeResult.Amount;
            notes += paymentFeeResult.Note;

            //PODATKI
            decimal taxRate = _taxCalculator.CalculateTaxRate(customer.Country);
            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = taxBase * taxRate;
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                notes += "minimum invoice amount applied; ";
            }

            var invoice = _invoiceFactory.Create(
                customer, normalizedPlanCode, normalizedPaymentMethod, seatCount, 
                baseAmount, discountAmount, supportFee, paymentFee, taxAmount, finalAmount, notes);

            LegacyBillingGateway.SaveInvoice(invoice);

            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body =
                    $"Hello {customer.FullName}, your renewal for plan {normalizedPlanCode} " +
                    $"has been prepared. Final amount: {invoice.FinalAmount:F2}.";

                _billingGateway.SendEmail(customer.Email, subject, body);
            }

            return invoice;
        }
    }
}
