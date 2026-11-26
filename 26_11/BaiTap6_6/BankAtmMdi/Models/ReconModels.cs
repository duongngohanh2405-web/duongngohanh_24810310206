namespace BankAtmMdi.Models
{
    /// <summary>Thông tin tiến độ gửi về UI Thread qua IProgress.</summary>
    public record ReconProgress(int Percent, string Message, long Processed);

    /// <summary>Kết quả đối soát cuối cùng.</summary>
    public class ReconResult
    {
        public long TotalTransactions { get; set; }
        public long Matched { get; set; }
        public long Mismatched { get; set; }
        public long Pending { get; set; }
        public decimal TotalWithdrawn { get; set; }
        public TimeSpan Elapsed { get; set; }
    }
}
