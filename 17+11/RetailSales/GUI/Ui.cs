using System;
using System.Windows.Forms;
using RetailSales.BUS;

namespace RetailSales.GUI
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
