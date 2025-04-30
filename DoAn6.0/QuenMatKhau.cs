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
using DoAn6._0;

namespace DoAn6._0 // Đảm bảo namespace này đúng
{
    public partial class QuenMatKhau : Form
    {
        Modify modify = new Modify();

        public QuenMatKhau()
        {
            InitializeComponent();
            // Giả sử bạn có các TextBox tên là: txtEmailQuenMK, txtMatKhauMoi, txtXacNhanMatKhauMoi
            if (txtMatKhauMoi != null) txtMatKhauMoi.PasswordChar = '*';
            if (txtXacNhanMatKhauMoi != null) txtXacNhanMatKhauMoi.PasswordChar = '*';
        }

        // --- Đã thêm các MessageBox báo lỗi chi tiết ---
        private void btnKhoiPhuc_Click(object sender, EventArgs e) // Giả sử nút của bạn tên là btnKhoiPhuc
        {
            // 1. Lấy dữ liệu
            string email = txtEmailQuenMK.Text.Trim();
            string matKhauMoi = txtMatKhauMoi.Text; // Không Trim() mật khẩu
            string xacNhanMKMoi = txtXacNhanMatKhauMoi.Text;

            // 2. Validation - Kiểm tra đầu vào
            // Kiểm tra Email rỗng
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Vui lòng nhập địa chỉ Email!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmailQuenMK.Focus(); // Đặt focus vào ô Email
                return; // Dừng lại
            }

            // Kiểm tra định dạng Email hợp lệ
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Định dạng Email không hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmailQuenMK.Focus();
                return; // Dừng lại
            }

            // Kiểm tra Mật khẩu mới rỗng
            if (string.IsNullOrWhiteSpace(matKhauMoi))
            {
                MessageBox.Show("Vui lòng nhập Mật khẩu mới!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhauMoi.Focus(); // Đặt focus vào ô Mật khẩu mới
                return; // Dừng lại
            }

            // Kiểm tra độ dài Mật khẩu mới (ví dụ: ít nhất 6 ký tự)
            if (matKhauMoi.Length < 6)
            {
                MessageBox.Show("Mật khẩu mới phải có ít nhất 6 ký tự!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtMatKhauMoi.Focus();
                return; // Dừng lại
            }

            // Kiểm tra Mật khẩu xác nhận có khớp không
            if (matKhauMoi != xacNhanMKMoi)
            {
                MessageBox.Show("Mật khẩu xác nhận không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtXacNhanMatKhauMoi.Focus(); // Đặt focus vào ô Xác nhận
                txtXacNhanMatKhauMoi.Text = ""; // Có thể xóa trắng ô xác nhận
                return; // Dừng lại
            }

            // 3. Kiểm tra Email có tồn tại trong hệ thống không
            if (!modify.DoesEmailExist(email))
            {
                MessageBox.Show("Địa chỉ Email này không tồn tại trong hệ thống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtEmailQuenMK.Focus(); // Đặt focus lại ô Email
                return; // Dừng lại
            }

            // 4. Tạo câu lệnh UPDATE và Parameters (Lưu plain text như yêu cầu)
            string query = "UPDATE TaiKhoan SET TK_MatKhau = @NewMatKhau WHERE TK_Email = @Email";
            List<SqlParameter> parameters = new List<SqlParameter>
             {
                 new SqlParameter("@NewMatKhau", SqlDbType.NVarChar) { Value = matKhauMoi }, // Lưu mật khẩu plain text mới
                 new SqlParameter("@Email", SqlDbType.NVarChar) { Value = email }
             };

            // 5. Thực thi lệnh UPDATE
            int affectedRows = modify.ExecuteCommand(query, parameters);

            if (affectedRows > 0)
            {
                MessageBox.Show("Cập nhật mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close(); // Đóng form quên mật khẩu
            }
            else if (affectedRows == 0) // Vẫn nên giữ lại kiểm tra này phòng trường hợp email bị xóa giữa chừng
            {
                MessageBox.Show("Không tìm thấy tài khoản với Email đã nhập để cập nhật. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            // else (affectedRows < 0): Lỗi SQL đã được xử lý hoặc ghi log trong Modify.ExecuteCommand
            else
            {
                MessageBox.Show("Đã xảy ra lỗi trong quá trình cập nhật mật khẩu. Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Hàm kiểm tra định dạng email (giữ nguyên)
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email.Trim(); } catch { return false; }
        }

        private void QuenMatKhau_Load(object sender, EventArgs e)
        {
            // Code khởi tạo form nếu cần
        }
    }
}