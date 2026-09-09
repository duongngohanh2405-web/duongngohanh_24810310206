using System;
using System.Text;

namespace PaymentGatewaySystem
{
    // 1. Interfaces
    public interface IPayable
    {
        bool ProcessPayment(decimal amount);
    }

    public interface IRefundable
    {
        bool ProcessRefund(decimal amount, string reason);
    }

    // 2. Abstract Class
    public abstract class PaymentGateway
    {
        public string TransactionId { get; init; }
        public DateTime CreationDate { get; init; }
        public string Status { get; protected set; }

        protected PaymentGateway(string transactionId)
        {
            TransactionId = transactionId;
            CreationDate = DateTime.Now;
            Status = "Pending";
        }

        public abstract void ValidateConnection();

        public virtual void LogTransaction(string message)
        {
            Console.WriteLine($"[{CreationDate:yyyy-MM-dd HH:mm:ss}] [TxID: {TransactionId}] [{Status}]: {message}");
        }
    }

    // 3. Concrete Class
    public class MomoPayment : PaymentGateway, IPayable, IRefundable
    {
        public string PhoneNumber { get; set; }

        public MomoPayment(string transactionId, string phoneNumber) : base(transactionId)
        {
            PhoneNumber = phoneNumber;
        }

        public override void ValidateConnection()
        {
            Console.WriteLine($"--> Kiểm tra kết nối API MoMo thành công cho SĐT: {PhoneNumber}");
        }

        public bool ProcessPayment(decimal amount)
        {
            ValidateConnection();
            Status = "Success";
            LogTransaction($"Thanh toán MoMo thành công số tiền {amount:N0} VNĐ");
            return true;
        }

        public bool ProcessRefund(decimal amount, string reason)
        {
            Status = "Refunded";
            LogTransaction($"Hoàn tiền MoMo thành công {amount:N0} VNĐ. Lý do: {reason}");
            return true;
        }
    }

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.WriteLine("=== KỊCH BẢN KIỂM THỬ BÀI 4 ===");

            // Khởi tạo cổng MoMo
            MomoPayment momo = new MomoPayment("TXN_MOMO_9981", "0987654321");

            Console.WriteLine("--- Ép kiểu về IPayable để xử lý thanh toán ---");
            IPayable payableService = momo; // Explicit interface usage via Polymorphism
            payableService.ProcessPayment(250_000m);

            Console.WriteLine("\n--- Ép kiểu về IRefundable để xử lý hoàn tiền ---");
            IRefundable refundableService = momo;
            refundableService.ProcessRefund(250_000m, "Khách hàng đổi ý hủy đơn");
        }
    }
}