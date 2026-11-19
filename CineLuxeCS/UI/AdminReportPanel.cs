using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>
    /// Báo cáo &amp; Doanh thu = use case "Thống kê doanh thu vé":
    /// theo khoảng thời gian (ngày / tháng), theo phim, trạng thái vé (đã bán / đã hủy), hình thức thanh toán, xuất báo cáo.
    /// </summary>
    public class AdminReportPanel : Panel
    {
        private readonly Input txtFrom = Theme.Field(), txtTo = Theme.Field();
        private readonly ComboBox cbMode = new ComboBox();
        private readonly Label lblSummary = Theme.Lbl("", FontStyle.Bold, 15, Theme.RUBY);
        private readonly DataGridView gTime = Grid("MỐC THỜI GIAN"), gMovie = Grid("PHIM"), gStatus = Grid("TRẠNG THÁI VÉ"), gPay = Grid("HÌNH THỨC THANH TOÁN");
        private DateTime from = DateTime.Today.AddDays(-6), to = DateTime.Today;

        private static DataGridView Grid(string first)
        {
            DataGridView g = Theme.Table(first, "SỐ VÉ / ĐƠN", "DOANH THU", "TỶ TRỌNG");
            g.Dock = DockStyle.Fill;
            return g;
        }

        public AdminReportPanel()
        {
            Dock = DockStyle.Fill; BackColor = Theme.BG;
            Padding = new Padding(28, 22, 28, 24);

            TabControl tabs = new TabControl();
            tabs.Dock = DockStyle.Fill; tabs.Font = Theme.Fnt(FontStyle.Bold, 13);
            AddTab(tabs, "Theo thời gian", gTime);
            AddTab(tabs, "Doanh thu theo phim", gMovie);
            AddTab(tabs, "Trạng thái vé (đã bán / đã hủy)", gStatus);
            AddTab(tabs, "Hình thức thanh toán phổ biến", gPay);
            Controls.Add(tabs);

            Stack north = new Stack();
            Stack t = new Stack();
            t.Dock = DockStyle.None;
            t.Add(Theme.Lbl("Báo Cáo & Thống Kê Doanh Thu Vé", FontStyle.Bold, 26, Theme.TEXT));
            t.Add(Theme.Lbl("Doanh thu chỉ tính vé đã thanh toán (không gồm vé đã hủy), theo ngày đặt vé", FontStyle.Regular, 12, Theme.MUTED));
            RoundButton exp = Theme.Primary("Xuất báo cáo (CSV/Excel)");
            exp.Click += (s, e) => ExportCsv();
            north.Add(Lay.Border(t, null, exp));

            txtFrom.MinimumSize = new Size(140, 40); txtFrom.MaximumSize = new Size(140, 40);
            txtTo.MinimumSize = new Size(140, 40); txtTo.MaximumSize = new Size(140, 40);
            cbMode.DropDownStyle = ComboBoxStyle.DropDownList; cbMode.Font = Theme.Fnt(FontStyle.Regular, 13);
            cbMode.Items.AddRange(new object[] { "Theo ngày", "Theo tháng" }); cbMode.SelectedIndex = 0; cbMode.Width = 130;
            RoundButton view = Theme.Primary("Xem thống kê");
            view.Click += (s, e) => Reload();
            txtFrom.Submit += (s, e) => Reload();
            txtTo.Submit += (s, e) => Reload();
            north.Add(Lay.Row(8, Theme.Lbl("Từ ngày", FontStyle.Bold, 12, Theme.MUTED), txtFrom,
                    Theme.Lbl("Đến ngày", FontStyle.Bold, 12, Theme.MUTED), txtTo, cbMode, view), 10, false);
            north.Add(lblSummary, 10);
            Panel np = Lay.Pad(north, 0, 0, 0, 12);
            np.Dock = DockStyle.Top;
            Controls.Add(np);
        }

        private static void AddTab(TabControl tabs, string title, Control content)
        {
            TabPage p = new TabPage(title);
            p.BackColor = Color.White;
            p.Controls.Add(content);
            tabs.TabPages.Add(p);
        }

        public void RefreshView()
        {
            txtFrom.Text = Fmt.Date(from); txtTo.Text = Fmt.Date(to);
            Reload();
        }

        private bool ReadRange()
        {
            try
            {
                DateTime f = Fmt.ParseDate(txtFrom.Text), t = Fmt.ParseDate(txtTo.Text);
                if (f > t) throw new ArgumentException("\"Từ ngày\" phải trước hoặc bằng \"Đến ngày\".");
                if ((t - f).TotalDays > 800) throw new ArgumentException("Khoảng thời gian tối đa khoảng 2 năm.");
                from = f; to = t;
                return true;
            }
            catch (ArgumentException ex) { Theme.Error(this, ex.Message); return false; }
        }

        private void Reload()
        {
            if (!ReadRange()) return;
            List<StatsService.Row> time = cbMode.SelectedIndex == 0 ? StatsService.ByDay(from, to) : StatsService.ByMonth(from, to);
            Fill(gTime, time, true);
            Fill(gMovie, StatsService.ByMovie(from, to), true);
            Fill(gStatus, StatsService.ByStatus(from, to), false);
            Fill(gPay, StatsService.ByPayment(from, to), true);
            long rev = time.Sum(r => r.Revenue);
            int tk = time.Sum(r => r.Tickets);
            lblSummary.Text = "Tổng doanh thu: " + Theme.Money(rev) + "   •   Tổng vé đã bán: " + tk;
        }

        private void Fill(DataGridView g, List<StatsService.Row> rows, bool pct)
        {
            g.Rows.Clear();
            long total = rows.Sum(r => r.Revenue);
            foreach (StatsService.Row r in rows)
                g.Rows.Add(r.Label, r.Tickets, Theme.Money(r.Revenue),
                        pct && total > 0 ? (r.Revenue * 100.0 / total).ToString("0.0", Theme.Inv) + "%" : "—");
            Theme.ClearSel(g);
        }

        /// <summary>Xuất toàn bộ báo cáo ra file CSV (mở được bằng Excel, có BOM UTF-8 để không lỗi tiếng Việt).</summary>
        public void ExportCsv()
        {
            if (!ReadRange()) return;
            using (SaveFileDialog fc = new SaveFileDialog())
            {
                fc.FileName = "bao_cao_doanh_thu_" + from.ToString("yyyyMMdd", Theme.Inv) + "_" + to.ToString("yyyyMMdd", Theme.Inv) + ".csv";
                fc.Filter = "CSV (*.csv)|*.csv";
                if (fc.ShowDialog(this) != DialogResult.OK) return;
                StringBuilder sb = new StringBuilder();
                sb.Append("BÁO CÁO DOANH THU VÉ CINELUXE\nTừ ngày;").Append(Fmt.Date(from)).Append(";Đến ngày;").Append(Fmt.Date(to)).Append("\n\n");
                Section(sb, "DOANH THU THEO NGÀY", StatsService.ByDay(from, to));
                Section(sb, "DOANH THU THEO THÁNG", StatsService.ByMonth(from, to));
                Section(sb, "DOANH THU THEO PHIM", StatsService.ByMovie(from, to));
                Section(sb, "TRẠNG THÁI VÉ", StatsService.ByStatus(from, to));
                Section(sb, "HÌNH THỨC THANH TOÁN (số đơn)", StatsService.ByPayment(from, to));
                try
                {
                    File.WriteAllText(fc.FileName, sb.ToString().Replace("\n", "\r\n"), new UTF8Encoding(true));
                    Theme.Info(this, "Đã xuất báo cáo:\n" + fc.FileName);
                }
                catch (IOException ex) { Theme.Error(this, "Không ghi được file: " + ex.Message); }
            }
        }

        private static void Section(StringBuilder sb, string title, List<StatsService.Row> rows)
        {
            sb.Append(title).Append("\nMục;Số vé/đơn;Doanh thu (VND)\n");
            foreach (StatsService.Row r in rows) sb.Append(r.Label.Trim()).Append(';').Append(r.Tickets).Append(';').Append(r.Revenue).Append('\n');
            sb.Append('\n');
        }
    }
}
