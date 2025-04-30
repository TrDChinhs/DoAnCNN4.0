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
            if (!string.IsNullOrEmpty(this.currentLoggedInUsername))
            {
                CapNhatTaiKhoan capNhatTaiKhoan = new CapNhatTaiKhoan(this.currentLoggedInUsername);
                capNhatTaiKhoan.Show();
            }
            else
            {
                MessageBox.Show("Lỗi: Không xác định được thông tin người dùng để cập nhật.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DangNhap loginForm = new DangNhap();
                loginForm.Show();
                loginForm.FormClosed += (s, args) => this.Show();
            }
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
