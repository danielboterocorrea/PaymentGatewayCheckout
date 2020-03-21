namespace PaymentGateway.Domain.Validators
{
    public interface IPaymentAmountRule
    {
        bool IsValid(decimal amount, out string error);
    }
}
