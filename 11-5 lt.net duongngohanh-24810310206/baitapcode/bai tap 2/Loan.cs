namespace LabEquipmentCheckout;

public class Loan
{
    public string Id { get; set; } = "";
    public string StudentId { get; set; } = "";
    public string StudentName { get; set; } = "";
    public string Equipment { get; set; } = "";
    public int Quantity { get; set; } = 1;
    public DateTime BorrowDate { get; set; } = DateTime.Today;
    public DateTime DueDate { get; set; } = DateTime.Today;
    public DateTime? ReturnDate { get; set; }

    /// Tình trạng được tính tự động, không lưu cứng:
    /// đã có ngày trả -> Đã trả; chưa trả và hạn trả < hôm nay -> Quá hạn; còn lại -> Đang mượn.
    public string Status => StatusOf(DueDate, ReturnDate);

    public static string StatusOf(DateTime due, DateTime? returned) =>
        returned.HasValue ? "Đã trả"
        : due.Date < DateTime.Today ? "Quá hạn"
        : "Đang mượn";
}
