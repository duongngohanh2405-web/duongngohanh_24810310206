using System;
using System.Drawing;
using System.Windows.Forms;

namespace CineLuxe.UI
{
    /// <summary>Hộp thoại nhỏ dùng chung (nhập chữ, chọn trong danh sách, hộp OK/Hủy chứa nội dung tùy ý).</summary>
    public static class Dialogs
    {
        /// <summary>Hiện hộp thoại chứa content với nút OK / Hủy. Trả về true nếu bấm OK.</summary>
        public static bool OkCancel(IWin32Window owner, string title, Control content, string okText)
        {
            using (Form f = new Form())
            {
                f.Text = title; f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog; f.MaximizeBox = false; f.MinimizeBox = false;
                f.ShowInTaskbar = false; f.AutoScaleMode = AutoScaleMode.None; f.BackColor = Color.White;
                f.AutoSize = true; f.AutoSizeMode = AutoSizeMode.GrowAndShrink; f.Padding = new Padding(18);
                f.Font = Theme.Fnt(FontStyle.Regular, 14);

                Stack st = new Stack();
                content.Margin = Padding.Empty;
                st.Add(content);
                RoundButton ok = Theme.Primary(okText), cancel = Theme.Outline("Hủy");
                ok.Click += (s, e) => { f.DialogResult = DialogResult.OK; f.Close(); };
                cancel.Click += (s, e) => { f.DialogResult = DialogResult.Cancel; f.Close(); };
                FlowLayoutPanel row = Lay.Row(8, ok, cancel);
                row.Anchor = AnchorStyles.Right;
                Panel rowWrap = new Panel();
                rowWrap.AutoSize = true; rowWrap.AutoSizeMode = AutoSizeMode.GrowAndShrink;
                rowWrap.Padding = new Padding(0, 0, 0, 0);
                row.Dock = DockStyle.Right;
                rowWrap.Controls.Add(row);
                st.Add(rowWrap, 14);
                f.Controls.Add(st);
                f.KeyPreview = true;
                f.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) { f.DialogResult = DialogResult.Cancel; f.Close(); } };
                return f.ShowDialog(owner) == DialogResult.OK;
            }
        }

        /// <summary>Hộp thoại chỉ có nút Đóng, chứa nội dung tùy ý.</summary>
        public static void Message(IWin32Window owner, string title, Control content)
        {
            using (Form f = new Form())
            {
                f.Text = title; f.StartPosition = FormStartPosition.CenterParent;
                f.FormBorderStyle = FormBorderStyle.FixedDialog; f.MaximizeBox = false; f.MinimizeBox = false;
                f.ShowInTaskbar = false; f.AutoScaleMode = AutoScaleMode.None; f.BackColor = Color.White;
                f.AutoSize = true; f.AutoSizeMode = AutoSizeMode.GrowAndShrink; f.Padding = new Padding(18);
                f.Font = Theme.Fnt(FontStyle.Regular, 14);
                Stack st = new Stack();
                st.Add(content);
                RoundButton close = Theme.Primary("Đóng");
                close.Click += (s, e) => f.Close();
                st.Add(close, 14, false);
                close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
                f.Controls.Add(st);
                f.KeyPreview = true;
                f.KeyDown += (s, e) => { if (e.KeyCode == Keys.Escape) f.Close(); };
                f.ShowDialog(owner);
            }
        }

        /// <summary>Hỏi một đoạn chữ. Trả về null nếu bấm Hủy.</summary>
        public static string Prompt(IWin32Window owner, string title, string message, string initial)
        {
            Stack st = new Stack();
            st.Add(Theme.Lbl(message, FontStyle.Regular, 14, Theme.TEXT, 380));
            Input box = Theme.Field();
            box.Text = initial ?? "";
            box.MinimumSize = new Size(380, 40);
            st.Add(box, 8);
            if (!OkCancel(owner, title, st, "OK")) return null;
            return box.Text;
        }

        /// <summary>Chọn một mục trong danh sách. Trả về null nếu bấm Hủy.</summary>
        public static string Choose(IWin32Window owner, string title, string message, string[] options)
        {
            Stack st = new Stack();
            st.Add(Theme.Lbl(message, FontStyle.Regular, 14, Theme.TEXT, 340));
            ComboBox cb = new ComboBox();
            cb.DropDownStyle = ComboBoxStyle.DropDownList; cb.Font = Theme.Fnt(FontStyle.Regular, 14);
            cb.Items.AddRange(options); cb.SelectedIndex = 0; cb.MinimumSize = new Size(340, 0);
            st.Add(cb, 8);
            if (!OkCancel(owner, title, st, "OK")) return null;
            return (string)cb.SelectedItem;
        }
    }
}
