using System;

namespace AutoSpeed;

public abstract class PhuongTien
{
    private string _maPT;
    private string _tenHang;
    private int _namSanXuat;
    private decimal _giaGoc;

    public string MaPT
    {
        get => _maPT;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Mã phương tiện không được để trống!");
            _maPT = value;
        }
    }

    public string TenHang
    {
        get => _tenHang;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Tên hãng không được để trống!");
            _tenHang = value;
        }
    }

    public int NamSanXuat
    {
        get => _namSanXuat;
        set
        {
            if (value < 1900 || value > DateTime.Now.Year)
                throw new ArgumentException("Năm sản xuất không hợp lệ!");
            _namSanXuat = value;
        }
    }

    public decimal GiaGoc
    {
        get => _giaGoc;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Giá gốc phải lớn hơn 0!");
            _giaGoc = value;
        }
    }

    protected PhuongTien(string maPT, string tenHang, int namSanXuat, decimal giaGoc)
    {
        _maPT = "PT000";
        _tenHang = "";
        _namSanXuat = 1900;
        _giaGoc = 1;
        MaPT = string.IsNullOrWhiteSpace(maPT) ? "PT000" : maPT;
        TenHang = tenHang;
        NamSanXuat = namSanXuat;
        GiaGoc = giaGoc;
    }

    public abstract decimal TinhGiaLanBanh();

    public virtual string GetInfo()
    {
        return $"Mã: {MaPT} | Hãng: {TenHang} | Năm: {NamSanXuat} | Giá gốc: {GiaGoc:N0} | Giá lăn bánh: {TinhGiaLanBanh():N0}";
    }
}
