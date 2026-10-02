using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoSpeed;

public class QuanLyPhuongTien
{
    private readonly List<PhuongTien> _danhSach = new();

    public void AddPhuongTien(PhuongTien phuongTien)
    {
        if (phuongTien == null)
            throw new ArgumentNullException(nameof(phuongTien));
        _danhSach.Add(phuongTien);
    }

    public void DisplayAll()
    {
        foreach (PhuongTien pt in _danhSach)
            Console.WriteLine(pt.GetInfo());
    }

    public PhuongTien? FindMaxGiaLanBanh()
    {
        return _danhSach.Count == 0 ? null : _danhSach.MaxBy(x => x.TinhGiaLanBanh());
    }

    public List<PhuongTien> SearchByName(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<PhuongTien>(_danhSach);

        return _danhSach
            .Where(x => x.TenHang.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}
