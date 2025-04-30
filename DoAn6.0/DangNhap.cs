using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace DoAn6._0
{
    public partial class DangNhap : Form
    {
        public DangNhap()
        {
            InitializeComponent();
            if (txtMatKhau != null)
            {
                txtMatKhau.PasswordChar = '*';
                txtMatKhau.Text = "";
                this.txtMatKhau.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMatKhau_KeyDown);
            }
            if (txtTaiKhoan != null)
            {
                txtTaiKhoan.Text = "";
            }

            this.DoubleBuffered = true;

            // Initialize panels safely
            if (this.panel1 != null)
            {
                this.panel1.Paint += Panel1_Paint;
                this.panel1.Resize += (s, e) => this.panel1.Invalidate();
            }
            if (this.panelRounded != null)
            {
                this.panelRounded.Paint += PanelRounded_Paint;
                this.panelRounded.Resize += (s, e) => this.panelRounded.Invalidate();
            }
        }

        // --- Methods for rounded panels (Keep as they are) ---
        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;
            int radius = 10;
            Rectangle bounds = new Rectangle(Point.Empty, panel.Size); 
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = CreateRoundedRectPath(bounds, radius, true, true, false, false)) 
            {
                try
                {
                    panel.Region = new Region(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error setting panel region: " + ex.Message);
                    panel.Region = new Region(bounds);
                }
            }
        }

        private void PanelRounded_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = sender as Panel;
            if (panel == null) return;
            int radius = 10;
            Rectangle bounds = new Rectangle(Point.Empty, panel.Size);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (GraphicsPath path = CreateRoundedRectPath(bounds, radius, false, false, true, true)) // BottomLeft, BottomRight
            {
                try
                {
                    panel.Region = new Region(path);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error setting panel region: " + ex.Message);
                    panel.Region = new Region(bounds);
                }
            }
        }

        private GraphicsPath CreateRoundedRectPath(Rectangle bounds, int radius, bool topLeft = true, bool topRight = true, bool bottomLeft = true, bool bottomRight = true)
        {
            GraphicsPath path = new GraphicsPath();
            int diameter = radius * 2;

            if (bounds.Width <= 0 || bounds.Height <= 0) // Prevent invalid bounds
            {
                path.AddRectangle(bounds); // Add empty or invalid rect
                path.CloseFigure();
                return path;
            }


            if (diameter <= 0)
            {
                path.AddRectangle(bounds);
                path.CloseFigure();
                return path;
            }
            if (diameter > bounds.Width) diameter = bounds.Width;
            if (diameter > bounds.Height) diameter = bounds.Height;

            Rectangle arcRect = new Rectangle(bounds.Location, new Size(diameter, diameter));

            // Top-Left Arc
            if (topLeft && diameter > 0) path.AddArc(arcRect, 180, 90);
            else path.AddLine(bounds.Left, bounds.Top + (topLeft ? 0 : radius), bounds.Left, bounds.Top); // Adjust line start/end


            // Top Edge
            path.AddLine(bounds.Left + (topLeft ? radius : 0), bounds.Top, bounds.Right - (topRight ? radius : 0), bounds.Top);

            // Top-Right Arc
            arcRect.X = bounds.Right - diameter;
            if (topRight && diameter > 0) path.AddArc(arcRect, 270, 90);
            else path.AddLine(bounds.Right, bounds.Top, bounds.Right, bounds.Top + (topRight ? 0 : radius));

            // Right Edge
            path.AddLine(bounds.Right, bounds.Top + (topRight ? radius : 0), bounds.Right, bounds.Bottom - (bottomRight ? radius : 0));

            // Bottom-Right Arc
            arcRect.Y = bounds.Bottom - diameter;
            if (bottomRight && diameter > 0) path.AddArc(arcRect, 0, 90);
            else path.AddLine(bounds.Right, bounds.Bottom - (bottomRight ? 0 : radius), bounds.Right, bounds.Bottom);


            // Bottom Edge
            path.AddLine(bounds.Right - (bottomRight ? radius : 0), bounds.Bottom, bounds.Left + (bottomLeft ? radius : 0), bounds.Bottom);

            // Bottom-Left Arc
            arcRect.X = bounds.Left;
            if (bottomLeft && diameter > 0) path.AddArc(arcRect, 90, 90);
            else path.AddLine(bounds.Left, bounds.Bottom, bounds.Left, bounds.Bottom - (bottomLeft ? 0 : radius));


            // Left Edge
            path.AddLine(bounds.Left, bounds.Bottom - (bottomLeft ? radius : 0), bounds.Left, bounds.Top + (topLeft ? radius : 0));


            path.CloseFigure();
            return path;
        }
        // --- End of rounded panels methods ---

        private void btnDangNhap_Click(object sender, EventArgs e)
        {
            string tenDangNhap = txtTaiKhoan.Text.Trim();
            string matKhau = txtMatKhau.Text.Trim();

            // Basic validation
            if (string.IsNullOrEmpty(tenDangNhap))
            {
                MessageBox.Show("Vui lòng nhập Tên đăng nhập!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTaiKhoan.Focus();
                return;
            }
            if (string.IsNullOrEmpty(matKhau))
            {
                MessageBox.Show("Vui lòng nhập Mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhau.Focus();
                return;
            }

            Modify modify = new Modify();

            // 1. Check Login Credentials
            bool isValidLogin = modify.CheckLogin(tenDangNhap, matKhau);

            if (isValidLogin)
            {
                string crId = modify.GetCrIdFromUsername(tenDangNhap);

                // *** THÊM ĐOẠN KIỂM TRA ADMIN ***
                bool isAdmin = string.IsNullOrEmpty(crId); // Hoặc dùng tên đăng nhập: tenDangNhap.Equals("admin", StringComparison.OrdinalIgnoreCase);

                if (isAdmin) // Nếu là admin
                {
                    MessageBox.Show("Đăng nhập với quyền Admin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Mở Form quản lý của Admin (Bạn cần tạo form này)
                    Admin adminForm = new Admin(); // Giả sử tên form là AdminDashboard
                    adminForm.Show();
                    adminForm.FormClosed += (s, args) => this.Show();
                }
                else // Nếu là người dùng thường
                {
                    // Kiểm tra xem người dùng thường này có CR_ID hợp lệ không (dù đăng nhập đúng)
                    if (!string.IsNullOrEmpty(crId))
                    {
                        MessageBox.Show("Đăng nhập thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        // Mở form GiaoDien cho người dùng thường, truyền cả tenDangNhap và crId
                        GiaoDien giaoDien = new GiaoDien(tenDangNhap, crId);
                        giaoDien.Show();
                        this.Hide(); // Ẩn form đăng nhập
                    }
                    else
                    {
                        // Trường hợp đăng nhập đúng nhưng không có CR_ID (dữ liệu lỗi/thiếu)
                        MessageBox.Show("Đăng nhập thành công, nhưng không tìm thấy thông tin chủ ruộng liên kết. Vui lòng liên hệ quản trị viên.", "Lỗi Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                // Xử lý đăng nhập thất bại như cũ
                MessageBox.Show("Tên đăng nhập hoặc Mật khẩu không chính xác. Vui lòng thử lại!", "Lỗi đăng nhập", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtMatKhau.Text = "";
                txtMatKhau.Focus();
            }
        }

        // Handle Enter key press in password field
        private void txtMatKhau_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Prevent the 'ding' sound
                btnDangNhap_Click(sender, e); // Trigger the login button click event
            }
        }

        // LinkLabel event handlers (Keep as they are)
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            QuenMatKhau quenMatKhauForm = new QuenMatKhau();
            quenMatKhauForm.Show();
            quenMatKhauForm.FormClosed += (s, args) => this.Show();
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            DangKy dangKyForm = new DangKy();
            dangKyForm.Show();
            dangKyForm.FormClosed += (s, args) => this.Show();
        }

        private void DangNhap_Load(object sender, EventArgs e)
        {
        }

        private void DangNhap_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }
    }
}