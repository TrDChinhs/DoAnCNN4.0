using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DoAn6._0; 

namespace DoAn6._0
{
    public partial class DangKy : Form
    {
        private Modify modify = new Modify();

        public DangKy()
        {
            InitializeComponent();
            SetupPasswordTextBoxes();
        }

        private void SetupPasswordTextBoxes()
        {
            string passwordControlName = "txtMatKhauDangKy";
            string confirmPasswordControlName = "txtXacNhanMatKhau";

            SetPasswordChar(passwordControlName);
            SetPasswordChar(confirmPasswordControlName);
        }

        private void SetPasswordChar(string controlName)
        {
            Control[] foundControls = this.Controls.Find(controlName, true);
            if (foundControls.Length > 0 && foundControls[0] is TextBox txt)
            {
                txt.PasswordChar = '*';
            }
            else
            {
                Console.WriteLine($"Warning: Control '{controlName}' not found for PasswordChar setup.");
            }
        }

        private void btnDangKy_Click_1(object sender, EventArgs e)
        {
            string nameControl = "txtTenDangKy";
            string emailControl = "txtEmailDangKy";
            string passwordControl = "txtMatKhauDangKy";
            string confirmPasswordControl = "txtXacNhanMatKhau";

            // Lấy giá trị
            string tenTk = GetTextBoxValue(nameControl);
            string email = GetTextBoxValue(emailControl);
            string matKhau = GetTextBoxValue(passwordControl);
            string xacNhanMK = GetTextBoxValue(confirmPasswordControl);

            // --- Validation ---
            if (!ValidateInput(tenTk, email, matKhau, xacNhanMK, nameControl, emailControl, passwordControl, confirmPasswordControl))
            {
                return; // Dừng lại nếu validation thất bại
            }
            // --- Hết Validation ---

            // --- Kiểm tra tồn tại ---
            if (!CheckAccountExistence(tenTk, email, nameControl, emailControl))
            {
                return; // Dừng lại nếu kiểm tra thất bại hoặc tài khoản đã tồn tại
            }
            // --- Hết kiểm tra tồn tại ---


            // --- Gọi đăng ký ---
            RegisterAccount(tenTk, email, matKhau);
        }

        // --- Tách nhỏ hàm Validation ---
        private bool ValidateInput(string tenTk, string email, string matKhau, string xacNhanMK,
                                   string nameControl, string emailControl, string passwordControl, string confirmPasswordControl)
        {
            if (string.IsNullOrWhiteSpace(tenTk))
            { ShowError("Vui lòng nhập Tên tài khoản.", nameControl); return false; }

            if (string.IsNullOrWhiteSpace(email))
            { ShowError("Vui lòng nhập Email.", emailControl); return false; }

            if (!IsValidEmail(email))
            { ShowError("Định dạng Email không hợp lệ.", emailControl); return false; }

            if (string.IsNullOrWhiteSpace(matKhau))
            { ShowError("Vui lòng nhập Mật khẩu.", passwordControl); return false; }

            if (matKhau != xacNhanMK)
            { ShowError("Xác nhận mật khẩu không khớp.", confirmPasswordControl); return false; }

            return true; // Tất cả hợp lệ
        }

        // --- Tách nhỏ hàm kiểm tra tồn tại ---
        private bool CheckAccountExistence(string tenTk, string email, string nameControl, string emailControl)
        {
            try
            {
                if (modify.DoesAccountExist(tenTk, email))
                {
                    MessageBox.Show("Tên tài khoản hoặc Email này đã được đăng ký!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Focus vào trường bị trùng lặp
                    if (modify.DoesAccountExist(tenTk, null)) FocusControl(nameControl);
                    else FocusControl(emailControl);
                    return false; // Đã tồn tại, không tiếp tục
                }
                return true; // Chưa tồn tại, có thể tiếp tục
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Không thể kiểm tra tài khoản tồn tại. Lỗi: {ex.Message}", "Lỗi Cơ sở dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false; // Không kiểm tra được, không tiếp tục
            }
        }

        // --- Tách nhỏ hàm thực hiện đăng ký ---
        private void RegisterAccount(string tenTk, string email, string matKhau)
        {
            try
            {
                Console.WriteLine($"Attempting registration for: {tenTk}, {email}");
                // Gọi hàm đăng ký từ Modify class
                string createdCRID = modify.RegisterAccountWithTransaction(tenTk, email, matKhau); // Vẫn truyền plain text

                if (!string.IsNullOrEmpty(createdCRID)) // Thành công
                {
                    MessageBox.Show($"Đăng ký tài khoản thành công!\nMã Chủ ruộng của bạn là: {createdCRID}", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                else // Thất bại (lỗi đã được Modify class báo)
                {
                    // Không cần hiển thị MessageBox lỗi chi tiết ở đây nữa (trừ khi muốn)
                    // Chỉ hiển thị thông báo chung
                    MessageBox.Show("Đăng ký thất bại do lỗi xử lý. Vui lòng thử lại.", "Lỗi đăng ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex) // Lỗi ngoại lệ khác không mong muốn
            {
                MessageBox.Show($"Đã xảy ra lỗi không mong muốn: {ex.Message}", "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Console.WriteLine($"Lỗi ngoại lệ tại RegisterAccount: {ex}");
            }
        }


        // --- Các hàm phụ trợ (GetTextBoxValue, ShowError, FocusControl, IsValidEmail) ---
        private string GetTextBoxValue(string controlName)
        {
            Control[] foundControls = this.Controls.Find(controlName, true);
            if (foundControls.Length > 0 && foundControls[0] is TextBox txt)
            { return txt.Text.Trim(); }
            Console.WriteLine($"Warning: Control '{controlName}' not found or not a TextBox in GetTextBoxValue.");
            return string.Empty;
        }

        private void ShowError(string message, string controlNameToFocus)
        {
            MessageBox.Show(message, "Thông tin không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            FocusControl(controlNameToFocus);
        }

        private void FocusControl(string controlName)
        {
            Control[] foundControls = this.Controls.Find(controlName, true);
            if (foundControls.Length > 0) { foundControls[0].Focus(); }
            else { Console.WriteLine($"Warning: Could not set focus. Control '{controlName}' not found."); }
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email.Trim();
            }
            catch { return false; }
            // Không cần return cuối nếu mọi nhánh try/catch đều return
        }

        private void DangKy_Load(object sender, EventArgs e)
        {
            SetupPasswordTextBoxes();
        }

    }
} 