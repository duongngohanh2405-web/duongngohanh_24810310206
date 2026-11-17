using System;
using System.Globalization;
using System.Windows.Forms;
using HRM.BUS;

namespace HRM.GUI
{
    internal static class Ui
    {
        public static Label AddLabel(Control parent, string text, int x, int y)
        {
            var l = new Label { Text = text, Left = x, Top = y + 4, AutoSize = true };
            parent.Controls.Add(l);
            return l;
        }

        public static TextBox AddText(Control parent, int x, int y, int w)
        {
            var t = new TextBox { Left = x, Top = y, Width = w };
            parent.Controls.Add(t);
            return t;
        }

        public static Button AddButton(Control parent, string text, int x, int y, int w, EventHandler click)
        {
            var b = new Button { Text = text, Left = x, Top = y, Width = w, Height = 30 };
            b.Click += click;
            parent.Controls.Add(b);
            return b;
        }

        public static ComboBox AddCombo(Control parent, int x, int y, int w)
        {
            var c = new ComboBox { Left = x, Top = y, Width = w, DropDownStyle = ComboBoxStyle.DropDownList };
            parent.Controls.Add(c);
            return c;
        }

        // Chọn tháng/năm: hiển thị MM/yyyy, bấm mũi tên lên xuống để đổi tháng
        public static DateTimePicker AddMonthPicker(Control parent, int x, int y, int w)
        {
            var d = new DateTimePicker
            {
                Left = x, Top = y, Width = w,
                Format = DateTimePickerFormat.Custom,
                CustomFormat = "MM/yyyy",
                ShowUpDown = true,
                Value = DateTime.Today
            };
            parent.Controls.Add(d);
            return d;
        }

        public static string ToThangNam(DateTime d)
        {
            return d.ToString("MM/yyyy", CultureInfo.InvariantCulture);   // ví dụ 11/2026
        }

        public static DataGridView NewGrid()
        {
            return new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                MultiSelect = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false,
                BackgroundColor = System.Drawing.Color.White
            };
        }

        public static void SetHeader(DataGridView g, string column, string header, string format = null)
        {
            DataGridViewColumn c = g.Columns[column];
            if (c == null) return;
            c.HeaderText = header;
            if (format != null)
            {
                c.DefaultCellStyle.Format = format;
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
        }

        // Số thập phân theo định dạng của máy (vi-VN dùng dấu phẩy, en-US dùng dấu chấm)
        public static bool TryParseDouble(string s, out double v)
        {
            return double.TryParse((s ?? "").Trim(), NumberStyles.Float, CultureInfo.CurrentCulture, out v);
        }

        public static bool TryParseDecimal(string s, out decimal v)
        {
            return decimal.TryParse((s ?? "").Trim(), NumberStyles.Number, CultureInfo.CurrentCulture, out v);
        }

        public static string ViDuThapPhan()
        {
            return (0.5).ToString(CultureInfo.CurrentCulture);
        }

        public static void ShowError(Exception ex)
        {
            MessageBox.Show(ErrorHelper.ToMessage(ex), "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void ShowInfo(string msg)
        {
            MessageBox.Show(msg, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
