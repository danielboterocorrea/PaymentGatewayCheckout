using System;

namespace PaymentGateway.Domain.Model
{
    public class Merchant
    {
        public Guid Id { get; }
        public string Name { get; }

        public Merchant(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString()
        {
            return $"Merchant:[{Id} - {Name}]";
        }
    }
}
