using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CineLuxe.Data;
using CineLuxe.Models;
using CineLuxe.Services;

namespace CineLuxe.UI
{
    /// <summary>Quản lý phim: tìm kiếm, lọc, thêm / sửa / xóa phim (use case Quản lý phim).</summary>
    public class AdminMoviesPanel : Panel
    {
        private readonly DataStore db = DataStore.Get();
        private readonly Input txtSearch = Theme.Field();
        private readonly ComboBox cbStatus = new ComboBox();
        private readonly DataGridView table = Theme.Table("MÃ PHIM", "TÊN PHIM", "THỂ LOẠI", "THỜI LƯỢNG", "TUỔI", "ĐỊNH DẠNG", "KHỞI CHIẾU", "SUẤT SẮP TỚI", "TRẠNG THÁI");
        private readonly List<Movie> shown = new List<Movie>();
        private readonly Label lblCount = Theme.Muted("");

        public AdminMoviesPanel()
        {
            Dock = DockStyle.Fill; BackColor = Theme.BG;
            Padding = new Padding(28, 22, 28, 24);

            table.Columns[1].FillWeight = 200; table.Columns[2].FillWeight = 150;
            table.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) OpenEditDialog(); };
            table.Dock = DockStyle.Fill;
            Controls.Add(table);

            Stack north = new Stack();
            Stack t = new Stack();
            t.Dock = DockStyle.None;
            t.Add(Theme.Lbl("Quản Lý Phim", FontStyle.Bold, 26, Theme.TEXT));
            t.Add(Theme.Lbl("Danh sách phim trong hệ thống • thêm, chỉnh sửa, ngừng chiếu", FontStyle.Regular, 12, Theme.MUTED));
            RoundButton add = Theme.Primary("+ Thêm phim mới"), edit = Theme.Outline("Sửa"), del = Theme.Outline("Xóa");
            add.Click += (s, e) => OpenAddDialog();
            edit.Click += (s, e) => OpenEditDialog();
            del.Click += (s, e) => DeleteSelected();
            north.Add(Lay.Border(t, null, Lay.Row(8, add, edit, del)));

            cbStatus.DropDownStyle = ComboBoxStyle.DropDownList; cbStatus.Font = Theme.Fnt(FontStyle.Regular, 13);
            cbStatus.Items.AddRange(new object[] { "Tất cả trạng thái", "Đang chiếu", "Sắp chiếu", "Ngừng chiếu" });
            cbStatus.SelectedIndex = 0; cbStatus.Width = 170;
            txtSearch.MinimumSize = new Size(300, 40); txtSearch.MaximumSize = new Size(300, 40);
            txtSearch.Submit += (s, e) => RefreshList();
            cbStatus.SelectedIndexChanged += (s, e) => RefreshList();
            RoundButton find = Theme.Outline("Tìm kiếm");
            find.Click += (s, e) => RefreshList();
            north.Add(Lay.Row(8, txtSearch, cbStatus, find, lblCount), 10, false);
            Panel np = Lay.Pad(north, 0, 0, 0, 12);
            np.Dock = DockStyle.Top;
            Controls.Add(np);
        }

        public void RefreshList()
        {
            table.Rows.Clear(); shown.Clear();
            string q = txtSearch.Text.Trim().ToLowerInvariant();
            int st = cbStatus.SelectedIndex;
            foreach (Movie m in db.Movies)
            {
                if (q.Length > 0 && !(m.Title.ToLowerInvariant().Contains(q) || m.Director.ToLowerInvariant().Contains(q)
                        || m.Id.ToLowerInvariant().Contains(q) || m.Genre.ToLowerInvariant().Contains(q))) continue;
                if (st == 1 && m.Status != MovieStatus.DANG_CHIEU) continue;
                if (st == 2 && m.Status != MovieStatus.SAP_CHIEU) continue;
                if (st == 3 && m.Status != MovieStatus.NGUNG) continue;
                shown.Add(m);
                table.Rows.Add(m.Id, m.Title, m.Genre, m.Duration + " phút", m.Age, m.Formats, Fmt.Date(m.ReleaseDate),
                        ScheduleService.UpcomingOf(m.Id).Count + " suất", m.Status.Label());
            }
            lblCount.Text = "Hiển thị " + shown.Count + " / " + db.Movies.Count + " phim";
            Theme.ClearSel(table);
        }

        private Movie Selected()
        {
            int r = Theme.SelectedIndex(table);
            if (r < 0) { Theme.Error(this, "Vui lòng chọn một phim trong danh sách."); return null; }
            return shown[r];
        }

        public void OpenAddDialog() { MovieDialog(null); }
        private void OpenEditDialog() { Movie m = Selected(); if (m != null) MovieDialog(m); }

        private void DeleteSelected()
        {
            Movie m = Selected();
            if (m == null) return;
            if (!Theme.Confirm(this, "Xóa phim \"" + m.Title + "\" và các suất chiếu chưa bán vé?")) return;
            try { ScheduleService.RemoveMovie(m); RefreshList(); }
            catch (InvalidOperationException ex) { Theme.Error(this, ex.Message + "\nHãy chuyển trạng thái sang \"Ngừng chiếu\" thay vì xóa."); }
        }

        private static Input Wide() { Input i = Theme.Field(); i.MinimumSize = new Size(340, 40); return i; }

        private void MovieDialog(Movie edit)
        {
            Input title = Wide(), genre = Wide(), director = Wide(), formats = Wide(), release = Wide();
            NumericUpDown dur = new NumericUpDown();
            dur.Minimum = 30; dur.Maximum = 300; dur.Value = 120; dur.Font = Theme.Fnt(FontStyle.Regular, 14);
            NumericUpDown imdb = new NumericUpDown();
            imdb.Minimum = 0; imdb.Maximum = 10; imdb.DecimalPlaces = 1; imdb.Increment = 0.1M; imdb.Value = 7.0M; imdb.Font = Theme.Fnt(FontStyle.Regular, 14);
            ComboBox age = new ComboBox();
            age.DropDownStyle = ComboBoxStyle.DropDownList; age.Items.AddRange(new object[] { "P", "T13", "T16", "T18" }); age.SelectedIndex = 0;
            age.Font = Theme.Fnt(FontStyle.Regular, 14);
            ComboBox status = new ComboBox();
            status.DropDownStyle = ComboBoxStyle.DropDownList; status.Font = Theme.Fnt(FontStyle.Regular, 14);
            status.Items.AddRange(new object[] { "Đang chiếu", "Sắp chiếu", "Ngừng chiếu" }); status.SelectedIndex = 0;
            CheckBox vn = new CheckBox();
            vn.Text = "Phim Việt Nam"; vn.AutoSize = true; vn.Font = Theme.Fnt(FontStyle.Regular, 13);
            TextBox desc = new TextBox();
            desc.Multiline = true; desc.Height = 80; desc.ScrollBars = ScrollBars.Vertical; desc.Font = Theme.Fnt(FontStyle.Regular, 13);

            if (edit != null)
            {
                title.Text = edit.Title; genre.Text = edit.Genre; director.Text = edit.Director; formats.Text = edit.Formats;
                release.Text = Fmt.Date(edit.ReleaseDate);
                dur.Value = Math.Max(30, Math.Min(300, edit.Duration));
                imdb.Value = (decimal)Math.Max(0, Math.Min(10, edit.Imdb));
                age.SelectedItem = edit.Age;
                status.SelectedIndex = (int)edit.Status;
                vn.Checked = edit.Vietnamese; desc.Text = edit.Description;
            }
            else { release.Text = Fmt.Date(DateTime.Today); formats.Text = "2D"; }

            TableLayoutPanel f = new TableLayoutPanel();
            f.AutoSize = true; f.AutoSizeMode = AutoSizeMode.GrowAndShrink; f.ColumnCount = 2;
            f.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            f.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            object[][] rows = {
                new object[] { "Tên phim", title }, new object[] { "Thể loại", genre }, new object[] { "Đạo diễn", director },
                new object[] { "Thời lượng (phút)", dur }, new object[] { "Độ tuổi", age }, new object[] { "Định dạng chiếu", formats },
                new object[] { "Điểm IMDb", imdb }, new object[] { "Trạng thái", status },
                new object[] { "Ngày khởi chiếu (dd/MM/yyyy)", release }, new object[] { "", vn }, new object[] { "Mô tả", desc } };
            for (int i = 0; i < rows.Length; i++)
            {
                f.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                Label cap = Theme.Lbl((string)rows[i][0], FontStyle.Bold, 12, Theme.MUTED);
                cap.Margin = new Padding(0, 10, 12, 4);
                Control c = (Control)rows[i][1];
                c.Margin = new Padding(0, 4, 0, 4);
                if (c is TextBox) c.Width = 340;
                f.Controls.Add(cap, 0, i);
                f.Controls.Add(c, 1, i);
            }
            f.RowCount = rows.Length;

            if (!Dialogs.OkCancel(this, edit == null ? "Thêm phim mới" : "Sửa phim " + edit.Id, f, "OK")) return;
            try
            {
                if (title.Text.Trim().Length == 0 || genre.Text.Trim().Length == 0)
                    throw new ArgumentException("Tên phim và thể loại không được để trống.");
                DateTime rel = Fmt.ParseDate(release.Text);
                Movie m = edit;
                if (m == null)
                {
                    m = new Movie("MOV-" + (9000 + db.NextSeq() % 1000), "", "", "", 0, "P", "", 0, MovieStatus.DANG_CHIEU, rel, "");
                    db.Movies.Add(m);
                }
                m.Title = title.Text.Trim(); m.Genre = genre.Text.Trim(); m.Director = director.Text.Trim();
                m.Duration = (int)dur.Value; m.Age = (string)age.SelectedItem; m.Formats = formats.Text.Trim();
                m.Imdb = Math.Round((double)imdb.Value, 1); m.Status = (MovieStatus)status.SelectedIndex;
                m.ReleaseDate = rel; m.Vietnamese = vn.Checked; m.Description = desc.Text.Trim();
                db.Save();
                RefreshList();
            }
            catch (ArgumentException ex) { Theme.Error(this, ex.Message); }
        }
    }
}
