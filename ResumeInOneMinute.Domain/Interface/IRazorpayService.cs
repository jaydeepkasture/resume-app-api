namespace ResumeInOneMinute.Domain.Interface;

public interface IRazorpayService
{
    string CreateOrder(decimal amount, string currency, string receipt);
    bool VerifyPayment(string paymentId, string orderId, string signature);
}
