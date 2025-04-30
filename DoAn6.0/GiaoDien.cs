using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn6._0
{
    public partial class GiaoDien : Form
    {
        private string currentLoggedInUsername;
        private string currentCrIdChuRuong;

        public GiaoDien(string username, string crId)
        {
            InitializeComponent();
            this.currentLoggedInUsername = username;
            this.currentCrIdChuRuong = crId; 
        }

        private void button1_Click(object sender, EventArgs e) 
        {
            DuBaoThoiTiet duBaoThoiTiet = new DuBaoThoiTiet();
            duBaoThoiTiet.Show();
            duBaoThoiTiet.FormClosed += (s, args) => this.Show();
        }

        private void button2_Click(object sender, EventArgs e) 
        {
            string loggedInUsername = currentLoggedInUsername; 

            if (string.IsNullOrEmpty(loggedInUsername))
            {
                MessageBox.Show("Không xác định được tài khoản đang đăng nhập.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Tạo và hiển thị form cập nhật, truyền username vào
            CapNhatTaiKhoan formCapNhat = new CapNhatTaiKhoan(loggedInUsername);
            formCapNhat.Show(); // Dùng ShowDialog để khóa form GiaoDien lại
        }

        private void btnThongTin_Click(object sender, EventArgs e)
        {
            ThongTin thongTin = new ThongTin(this.currentCrIdChuRuong);
            thongTin.Show();
            thongTin.FormClosed += (s, args) => this.Show();
        }

        private void GiaoDien_FormClosed(object sender, FormClosedEventArgs e)
        {
            DangNhap dangNhap = new DangNhap();
            dangNhap.Show();
            this.Hide();
        }
    } 
}
