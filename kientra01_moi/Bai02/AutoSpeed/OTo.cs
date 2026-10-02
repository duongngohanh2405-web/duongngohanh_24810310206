using System;

namespace AutoSpeed;

public class OTo : PhuongTien
{
    private int _soChoNgoi;
    private double _dungTichDongCo;

    public int SoChoNgoi
    {
        get => _soChoNgoi;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Số chỗ ngồi phải lớn hơn 0!");
            _soChoNgoi = value;
        }
    }

    public double DungTichDongCo
    {
        get => _dungTichDongCo;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Dung tích động cơ phải lớn hơn 0!");
            _dungTichDongCo = value;
        }
    }

    public OTo(string maPT, string tenHang, int namSanXuat, decimal giaGoc, int soChoNgoi, double dungTichDongCo)
        : base(maPT, tenHang, namSanXuat, giaGoc)
    {
        SoChoNgoi = soChoNgoi;
        DungTichDongCo = dungTichDongCo;
    }

    public override decimal TinhGiaLanBanh()
    {
        if (SoChoNgoi <= 9)
            return GiaGoc * 1.42m;

        return GiaGoc * 1.10m;
    }

    public override string GetInfo()
    {
        return base.GetInfo() + $" | Số chỗ: {SoChoNgoi} | Động cơ: {DungTichDongCo:N1}L";
    }
}
