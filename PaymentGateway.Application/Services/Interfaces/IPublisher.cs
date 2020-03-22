namespace PaymentGateway.Application.Services.Interfaces
{
    public interface IPublisher<T>
    {
        void Publish(T request);
    }
}
