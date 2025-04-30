using System;
using System.Data;
using System.Windows.Forms;

namespace DoAn6._0
{
    public partial class CapNhatTaiKhoan : Form
    {
        private Modify modify;
        private string _currentUsername; // Lưu tên tài khoản đang đăng nhập
        private string _currentCrId;     // Lưu ID Chủ ruộng tương ứng

        // Constructor nhận tên tài khoản
        public CapNhatTaiKhoan(string username)
        {
            InitializeComponent();
            modify = new Modify();
            _currentUsername = username;

            if (string.IsNullOrEmpty(_currentUsername))
            {
                MessageBox.Show("Không xác định được tên tài khoản.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Load += (s, e) => this.Close(); // Đóng form nếu không có username
                return;
            }

            // Lấy CR_ID tương ứng ngay khi khởi tạo (nếu cần dùng sớm)
            _currentCrId = modify.GetCrIdFromUsername(_currentUsername);
            if (string.IsNullOrEmpty(_currentCrId))
            {
                MessageBox.Show("Không tìm thấy thông tin Chủ ruộng liên kết với tài khoản này.", "Lỗi Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                // Có thể không đóng form ngay, cho phép xem tên TK nhưng không lưu được
                // Hoặc đóng form:
                // this.Load += (s, e) => this.Close();
            }
        }

        // Sự kiện Form Load

        private void CapNhatTaiKhoan_Load_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentUsername)) return; // Thoát nếu không có username

            LoadAccountInfo();

            // Gắn sự kiện cho CheckBox để đảm bảo chỉ chọn 1
            checkBoxNam.CheckedChanged += CheckBoxGioiTinh_CheckedChanged;
            checkBoxNu.CheckedChanged += CheckBoxGioiTinh_CheckedChanged;
        }

        // Hàm load thông tin tài khoản lên form
        private void LoadAccountInfo()
        {
            txtTenTaiKhoan.Text = _currentUsername; // Hiển thị tên tài khoản
            txtTenTaiKhoan.ReadOnly = true;       // Không cho sửa tên tài khoản

            DataRow dr = modify.GetChuRuongInfo(_currentUsername);

            if (dr != null)
            {
                try
                {
                    txtHoTen.Text = dr["CR_HoTen"]?.ToString();
                    txtSoCMND.Text = dr["CR_CMND"]?.ToString();
                    txtDiaChi.Text = dr["CR_DiaChi"]?.ToString();

                    // Xử lý Ngày sinh (nullable)
                    if (dr["CR_NgaySinh"] != DBNull.Value && dr["CR_NgaySinh"] != null)
                    {
                        dateTimePicker1.Value = Convert.ToDateTime(dr["CR_NgaySinh"]);
                        dateTimePicker1.Checked = true; // Đánh dấu có giá trị
                        dateTimePicker1.Format = DateTimePickerFormat.Short; // Định dạng ngày ngắn
                    }
                    else
                    {
                        dateTimePicker1.Checked = false; // Không có giá trị
                        dateTimePicker1.Format = DateTimePickerFormat.Custom; // Có thể ẩn ngày tháng đi
                        dateTimePicker1.CustomFormat = " ";
                    }


                    // Xử lý Giới tính
                    string gioiTinh = dr["CR_GioiTinh"]?.ToString();
                    if (!string.IsNullOrEmpty(gioiTinh))
                    {
                        if (gioiTinh.Equals("Nam", StringComparison.OrdinalIgnoreCase))
                        {
                            checkBoxNam.Checked = true;
                            checkBoxNu.Checked = false;
                        }
                        else if (gioiTinh.Equals("Nữ", StringComparison.OrdinalIgnoreCase) || gioiTinh.Equals("Nu", StringComparison.OrdinalIgnoreCase))
                        {
                            checkBoxNu.Checked = true;
                            checkBoxNam.Checked = false;
                        }
                        else
                        {
                            checkBoxNam.Checked = false;
                            checkBoxNu.Checked = false;
                        }
                    }
                    else
                    {
                        checkBoxNam.Checked = false;
                        checkBoxNu.Checked = false;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi hiển thị thông tin tài khoản: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                // Có thể xảy ra nếu GetCrIdFromUsername thành công nhưng GetChuRuongInfo thất bại
                MessageBox.Show("Không tìm thấy thông tin chi tiết của Chủ ruộng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Vô hiệu hóa nút lưu nếu không load được info
                btnLuu.Enabled = false;
            }
            // Xử lý lại format khi Checked = false sau khi gán giá trị
            dateTimePicker1.ValueChanged += (s, ev) => {
                if (dateTimePicker1.Checked) dateTimePicker1.Format = DateTimePickerFormat.Short;
            };
        }


        // Đảm bảo chỉ một CheckBox giới tính được chọn
        private void CheckBoxGioiTinh_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox changedCheckbox = sender as CheckBox;
            if (changedCheckbox == null || !changedCheckbox.Checked)
                return;

            if (changedCheckbox == checkBoxNam)
            {
                checkBoxNu.Checked = false;
            }
            else if (changedCheckbox == checkBoxNu)
            {
                checkBoxNam.Checked = false;
            }
        }

        // Sự kiện nhấn nút Lưu

        private void btnLuu_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentCrId))
            {
                MessageBox.Show("Không thể lưu do không xác định được ID Chủ ruộng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // --- Thu thập dữ liệu từ Form ---
            string hoTen = txtHoTen.Text.Trim();
            // Lấy ngày sinh, chỉ lấy giá trị nếu Checked = true
            DateTime? ngaySinh = dateTimePicker1.Checked ? (DateTime?)dateTimePicker1.Value : null;
            string gioiTinh = "";
            if (checkBoxNam.Checked)
            {
                gioiTinh = "Nam";
            }
            else if (checkBoxNu.Checked)
            {
                gioiTinh = "Nữ";
            }
            string cmnd = txtSoCMND.Text.Trim();
            string diaChi = txtDiaChi.Text.Trim();

            // --- Validation cơ bản ---
            if (string.IsNullOrWhiteSpace(hoTen))
            {
                MessageBox.Show("Họ và tên không được để trống.", "Thiếu thông tin", MessageBoxButtons.OK);
                txtHoTen.Focus();
                return;
            }
            // Thêm các validation khác nếu cần (ví dụ: định dạng CMND...)


            // --- Gọi hàm cập nhật ---
            try
            {
                bool success = modify.UpdateChuRuongInfo(_currentCrId, hoTen, ngaySinh, gioiTinh, cmnd, diaChi);

                if (success)
                {
                    MessageBox.Show("Cập nhật thông tin tài khoản thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.Close(); // Đóng form sau khi cập nhật thành công
                }
                else
                {
                    // Lỗi có thể đã được báo trong Modify.UpdateChuRuongInfo
                    MessageBox.Show("Cập nhật thông tin thất bại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi không mong muốn khi lưu: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Thêm xử lý sự kiện ValueChanged cho DateTimePicker để đổi format khi check/uncheck

        private void dateTimePicker1_ValueChanged_1(object sender, EventArgs e)
        {
            if (dateTimePicker1.Checked)
            {
                dateTimePicker1.Format = DateTimePickerFormat.Short;
            }
            else
            {
                dateTimePicker1.Format = DateTimePickerFormat.Custom;
                dateTimePicker1.CustomFormat = " "; // Ẩn ngày tháng khi không check
            }
        }
    } 
}