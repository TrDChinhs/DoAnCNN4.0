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

namespace DoAn6._0
{
    public partial class CapNhatTaiKhoan : Form
    {
        private string currentTenTaiKhoan;
        Modify modify; 

        public CapNhatTaiKhoan(string tenTaiKhoan) 
        {
            InitializeComponent();
            this.currentTenTaiKhoan = tenTaiKhoan; 
            modify = new Modify(); 
        }

        private void CapNhatTaiKhoan_Load(object sender, EventArgs e)
        {
            LoadUserInfo(); 
        }

        private void LoadUserInfo()
        {
            if (string.IsNullOrEmpty(currentTenTaiKhoan))
            {
                MessageBox.Show("Không xác định được tài khoản!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Close(); 
                return;
            }

            txtTenTaiKhoan.Text = currentTenTaiKhoan;
            txtTenTaiKhoan.ReadOnly = true;

            DataRow userInfo = modify.GetChuRuongInfo(currentTenTaiKhoan);

            if (userInfo != null)
            {
                txtHoTen.Text = userInfo["CR_HoTen"] != DBNull.Value ? userInfo["CR_HoTen"].ToString() : "";
                txtSoCMND.Text = userInfo["CR_CMND"] != DBNull.Value ? userInfo["CR_CMND"].ToString() : "";
                txtDiaChi.Text = userInfo["CR_DiaChi"] != DBNull.Value ? userInfo["CR_DiaChi"].ToString() : "";

                if (userInfo["CR_NgaySinh"] != DBNull.Value)
                {
                    dateTimePicker1.Value = Convert.ToDateTime(userInfo["CR_NgaySinh"]);
                }
                else
                {
                    dateTimePicker1.Value = DateTime.Now; 
                }

                if (userInfo["CR_GioiTinh"] != DBNull.Value)
                {
                    string gioiTinh = userInfo["CR_GioiTinh"].ToString().Trim();
                    if (gioiTinh.Equals("Nam", StringComparison.OrdinalIgnoreCase))
                    {
                        checkBoxNam.Checked = true;
                        checkBoxNu.Checked = false;
                    }
                    else if (gioiTinh.Equals("Nữ", StringComparison.OrdinalIgnoreCase) || gioiTinh.Equals("Nu", StringComparison.OrdinalIgnoreCase))
                    {
                        checkBoxNam.Checked = false;
                        checkBoxNu.Checked = true;
                    }
                    else
                    {
                        checkBoxNam.Checked = false;
                        checkBoxNu.Checked = false;
                    }
                }
                else
                {
                    // Không có thông tin giới tính
                    checkBoxNam.Checked = false;
                    checkBoxNu.Checked = false;
                }

                // Sự kiện để đảm bảo chỉ chọn 1 giới tính
                checkBoxNam.CheckedChanged += GioiTinh_CheckedChanged;
                checkBoxNu.CheckedChanged += GioiTinh_CheckedChanged;
            }
            else
            {
                MessageBox.Show("Không tìm thấy thông tin chi tiết cho tài khoản này.\nCó thể tài khoản mới được tạo và chưa có thông tin trong QLChuRuong.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                // Có thể để trống các trường hoặc đặt giá trị mặc định
                txtHoTen.Text = "";
                txtSoCMND.Text = "";
                txtDiaChi.Text = "";
                dateTimePicker1.Value = DateTime.Now;
                checkBoxNam.Checked = false;
                checkBoxNu.Checked = false;
                // Gắn sự kiện check cho trường hợp chưa có dữ liệu
                checkBoxNam.CheckedChanged += GioiTinh_CheckedChanged;
                checkBoxNu.CheckedChanged += GioiTinh_CheckedChanged;
            }
        }

        // Đảm bảo chỉ một CheckBox giới tính được chọn tại một thời điểm
        private void GioiTinh_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox changedCheckbox = sender as CheckBox;
            if (changedCheckbox == null || !changedCheckbox.Checked) return; // Chỉ xử lý khi check=true

            if (changedCheckbox == checkBoxNam)
            {
                checkBoxNu.Checked = false;
            }
            else if (changedCheckbox == checkBoxNu)
            {
                checkBoxNam.Checked = false;
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            // 1. Lấy dữ liệu từ các controls
            string hoTen = txtHoTen.Text.Trim();
            string cmnd = txtSoCMND.Text.Trim();
            string diaChi = txtDiaChi.Text.Trim();
            DateTime? ngaySinh = null; 

            ngaySinh = dateTimePicker1.Value;

            string gioiTinh = null;
            if (checkBoxNam.Checked)
            {
                gioiTinh = "Nam";
            }
            else if (checkBoxNu.Checked)
            {
                gioiTinh = "Nữ";
            }

            // 2. (Tùy chọn) Validate dữ liệu (ví dụ: Họ tên không được trống)
            if (string.IsNullOrEmpty(hoTen))
            {
                MessageBox.Show("Vui lòng nhập Họ và Tên.", "Thiếu thông tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHoTen.Focus();
                return;
            }

            // 3. Gọi phương thức cập nhật trong lớp Modify
            bool success = modify.UpdateChuRuongInfo(currentTenTaiKhoan, hoTen, ngaySinh, gioiTinh, cmnd, diaChi);

            // 4. Hiển thị thông báo kết quả
            if (success)
            {
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Cập nhật thông tin thất bại! Vui lòng kiểm tra lại hoặc liên hệ quản trị viên.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}