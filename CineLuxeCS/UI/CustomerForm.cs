using System;
using System.Drawing;
using System.Windows.Forms;
using CineLuxe.Models;

namespace CineLuxe.UI
{
    /// <summary>Thanh đầu trang trắng có đường kẻ dưới.</summary>
    public class HeaderBar : Panel
    {
        public HeaderBar()
        {
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = Color.White; Dock = DockStyle.Top;
            AutoSize = true; AutoSizeMode = AutoSizeMode.GrowAndShrink;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen p = new Pen(Theme.BORDER)) e.Graphics.DrawLine(p, 0, Height - 1, Width, Height - 1);
        }
    }

    /// <summary>Giao diện Khách hàng: Trang chủ / Đặt vé / Vé của tôi / Tài khoản.</summary>
    public class CustomerForm : AppForm
    {
        private readonly User user;
        private readonly Panel content = new Panel();
        private readonly HomePanel home;
        private readonly BookingPanel booking;
        private readonly MyTicketsPanel tickets;
        private readonly AccountPanel account;
        private readonly Button[] nav = new Button[4];
        private readonly Input search = Theme.Field();
        private readonly Label lblUser = Theme.Lbl("", FontStyle.Bold, 13, Theme.TEXT);

        public CustomerForm(User user) : base("CineLuxury - Khách hàng", 1280, 820)
        {
            this.user = user;
            content.Dock = DockStyle.Fill; content.BackColor = Theme.BG;

            home = new HomePanel(this);
            booking = new BookingPanel(this, user);
            tickets = new MyTicketsPanel(this, user);
            account = new AccountPanel(this, user);
            content.Controls.Add(home);
            content.Controls.Add(booking);
            content.Controls.Add(tickets);
            content.Controls.Add(account);

            Controls.Add(content);
            Controls.Add(BuildHeader());      // thêm sau để được dock lên trên cùng
            ShowHome();
        }

        private Control BuildHeader()
        {
            HeaderBar h = new HeaderBar();
            h.Padding = new Padding(24, 10, 24, 10);

            Label logo = Theme.Lbl("CineLuxury", FontStyle.Bold, 24, Theme.RUBY);

            string[] names = { "Trang chủ", "Đặt vé", "Vé của tôi", "Tài khoản" };
            for (int i = 0; i < 4; i++)
            {
                int idx = i;
                nav[i] = Theme.NavButton(names[i]);
                nav[i].Click += (s, e) =>
                {
                    switch (idx)
                    {
                        case 0: ShowHome(); break;
                        case 1: OpenBooking(null); break;
                        case 2: ShowTickets(); break;
                        default: ShowAccount(); break;
                    }
                };
            }
            FlowLayoutPanel menu = Lay.Row(4, nav[0], nav[1], nav[2], nav[3]);

            search.MinimumSize = new Size(230, 40); search.MaximumSize = new Size(230, 40);
            search.Submit += (s, e) => DoSearch();
            RoundButton go = Theme.Primary("Tìm");
            go.Click += (s, e) => DoSearch();
            RoundButton out_ = Theme.Outline("Đăng xuất");
            out_.Click += (s, e) => Logout();
            FlowLayoutPanel right = Lay.Row(10, search, go, lblUser, out_);

            TableLayoutPanel border = Lay.Border(logo, menu, right);
            menu.Dock = DockStyle.None; menu.Anchor = AnchorStyles.None;      // canh giữa
            h.Controls.Add(border);
            RefreshUser();
            return h;
        }

        private void DoSearch()
        {
            home.SetQuery(search.Text);
            ShowPage(home, 0);
        }

        public void RefreshUser() { lblUser.Text = user.FullName + "  •  " + user.Tier(); }

        private void ShowPage(Control page, int idx)
        {
            for (int i = 0; i < 4; i++) nav[i].ForeColor = i == idx ? Theme.RUBY : Theme.MUTED;
            foreach (Control c in content.Controls) c.Visible = c == page;
            page.BringToFront();
        }

        public void ShowHome() { search.Text = ""; home.SetQuery(""); ShowPage(home, 0); }

        /// <summary>movie == null: mở trang đặt vé với phim đầu tiên đang chiếu.</summary>
        public void OpenBooking(Movie movie) { booking.SetMovie(movie); ShowPage(booking, 1); }
        public void ShowTickets() { tickets.RefreshList(); ShowPage(tickets, 2); }
        public void ShowAccount() { account.RefreshView(); ShowPage(account, 3); }
    }
}
