namespace PaymentGateway.Domain.Model
{
    public class Currency
    {
        public string Value { get; }

        public Currency(string value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"Currency:[{Value}]";
        }
    }
}
