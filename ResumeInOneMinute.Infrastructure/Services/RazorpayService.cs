using Microsoft.Extensions.Configuration;
using Razorpay.Api;
using ResumeInOneMinute.Domain.Interface;

namespace ResumeInOneMinute.Infrastructure.Services;

public class RazorpayService : IRazorpayService
{
    private readonly string _keyId;
    private readonly string _keySecret;

    public RazorpayService(IConfiguration configuration)
    {
        _keyId = configuration["Razorpay:KeyId"] ?? throw new ArgumentNullException("Razorpay:KeyId");
        _keySecret = configuration["Razorpay:KeySecret"] ?? throw new ArgumentNullException("Razorpay:KeySecret");
    }

    public string CreateOrder(decimal amount, string currency, string receipt)
    {
        var client = new RazorpayClient(_keyId, _keySecret);

        Dictionary<string, object> options = new Dictionary<string, object>();
        options.Add("amount", (int)(amount * 100)); // amount in the smallest currency unit
        options.Add("receipt", receipt);
        options.Add("currency", currency);
        
        Order order = client.Order.Create(options);
        return order["id"].ToString();
    }

    public bool VerifyPayment(string paymentId, string orderId, string signature)
    {
        try
        {
            var attributes = new Dictionary<string, string>
            {
                { "razorpay_payment_id", paymentId },
                { "razorpay_order_id", orderId },
                { "razorpay_signature", signature }
            };

            Utils.verifyPaymentSignature(attributes);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
