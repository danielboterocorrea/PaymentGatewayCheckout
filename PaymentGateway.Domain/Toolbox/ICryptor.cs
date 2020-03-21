namespace PaymentGateway.Domain.Toolbox.Interfaces
{
    public interface ICryptor
    {
        string Encrypt(string value);
        string Decrypt(string value);
    }
}
