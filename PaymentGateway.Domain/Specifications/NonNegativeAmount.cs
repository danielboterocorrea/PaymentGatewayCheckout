using PaymentGateway.Domain.Validators;

namespace PaymentGateway.Domain.Specifications
{
    public class NonNegativeAmount : IPaymentAmountRule
    {
        public static string NonNegativeAmountViolation = "NonNegativeAmountViolation";

        public bool IsValid(decimal amount, out string error)
        {
            error = null;
            if(amount < 0)
            {
                error = NonNegativeAmountViolation;
                return false;
            }

            return true;
        }
    }
}
