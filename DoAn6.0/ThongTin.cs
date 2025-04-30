using System;
using System.Data;
using System.Globalization; // Để parse decimal chính xác
using System.Windows.Forms;

namespace DoAn6._0
{
    public partial class ThongTin : Form
    {
        private Modify modify;
        private string _currentCrId; // **QUAN TRỌNG:** Biến này cần được gán ID chủ ruộng đang đăng nhập


        // Enum để quản lý trạng thái form
        private enum FormState { Viewing, Adding, Editing }
        private FormState _currentState = FormState.Viewing; // Trạng thái ban đầu

        // Constructor - Nhận ID chủ ruộng khi form được tạo
        public ThongTin(string currentChuRuongId)
        {
            InitializeComponent();
            modify = new Modify();
            _currentCrId = currentChuRuongId;

            if (string.IsNullOrEmpty(_currentCrId))
            {
                MessageBox.Show("Không xác định được thông tin Chủ ruộng.", "Lỗi Người Dùng", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.Load += (s, e) => this.Close();
            }
        }

        // --- Form Load Event ---

        private void ThongTin_Load(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentCrId)) return;
            LoadRuongComboBox();
            SetDetailControlState(FormState.Viewing); // Bắt đầu ở trạng thái xem
        }

        // --- Load ComboBoxes ---
        private void LoadRuongComboBox()
        {
            try
            {
                DataTable dtRuong = modify.GetRuongListForUser(_currentCrId);

              
                if (dtRuong != null)
                {
                    DataRow dr = dtRuong.NewRow();
                    dr["R_IDRuong"] = DBNull.Value;
                    dr["R_ViTri"] = "--- Chọn Ruộng ---"; // Hoặc tên cột hiển thị khác
                    dtRuong.Rows.InsertAt(dr, 0);

                    cmbRuong.DataSource = dtRuong;
                    cmbRuong.DisplayMember = "R_ViTri"; // Thay bằng tên cột bạn muốn hiển thị
                    cmbRuong.ValueMember = "R_IDRuong";
                    cmbRuong.SelectedIndex = 0;
                }
                else
                {
                    MessageBox.Show("Không thể tải danh sách ruộng.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    cmbRuong.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách ruộng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbRuong.Enabled = false;
            }
        }

        private void LoadMuaVuComboBox(string ruongID)
        {
            try
            {
                DataTable dtMuaVu = modify.GetMuaVuListForRuong(ruongID);
                cmbMuaVu.DataSource = null; // Xóa binding cũ
                cmbMuaVu.Items.Clear(); // Xóa item cũ

                if (dtMuaVu != null)
                {
                    DataRow dr = dtMuaVu.NewRow();
                    dr["MV_IDMuaVu"] = DBNull.Value;
                    dr["MV_TenMuaVu"] = "--- Chọn Mùa Vụ ---";
                    dtMuaVu.Rows.InsertAt(dr, 0);

                    cmbMuaVu.DataSource = dtMuaVu;
                    cmbMuaVu.DisplayMember = "MV_TenMuaVu";
                    cmbMuaVu.ValueMember = "MV_IDMuaVu";
                    cmbMuaVu.SelectedIndex = 0;
                    cmbMuaVu.Enabled = true;
                }
                else
                {
                    cmbMuaVu.Items.Add("Lỗi tải mùa vụ");
                    cmbMuaVu.SelectedIndex = 0;
                    cmbMuaVu.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tải danh sách mùa vụ: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                cmbMuaVu.Enabled = false;
            }
        }

        // --- ComboBox Selection Changed Events ---
        private void cmbRuong_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ClearFormControls(); // Xóa chi tiết cũ
            SetDetailControlState(false); // Khóa ô nhập liệu chi tiết
            cmbMuaVu.DataSource = null;
            cmbMuaVu.Items.Clear();
            cmbMuaVu.Enabled = false;

            if (cmbRuong.SelectedIndex > 0 && cmbRuong.SelectedValue != null && cmbRuong.SelectedValue != DBNull.Value)
            {
                string selectedRuongID = cmbRuong.SelectedValue.ToString();
                LoadMuaVuComboBox(selectedRuongID);
                SetDetailControlState(FormState.Viewing); // Sẵn sàng để xem hoặc thêm mới cho ruộng này
            }
            else
            {
                // Nếu chọn lại "--- Chọn Ruộng ---"
                SetDetailControlState(FormState.Viewing); // Về trạng thái xem mặc định
                btnThem.Enabled = false; // Không thể thêm khi chưa chọn ruộng
                btnSua.Enabled = false;
                btnXoa.Enabled = false;
                btnLuu.Enabled = false;
            }
        }

        private void cmbMuaVu_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            ClearFormControls(); // Xóa chi tiết cũ
            SetDetailControlState(false); // Khóa ô nhập liệu

            if (cmbMuaVu.SelectedIndex > 0 && cmbMuaVu.SelectedValue != null && cmbMuaVu.SelectedValue != DBNull.Value)
            {
                string selectedMuaVuID = cmbMuaVu.SelectedValue.ToString();
                LoadDataForDisplay(selectedMuaVuID);
                SetDetailControlState(FormState.Viewing); // Đặt trạng thái xem sau khi load
            }
            else
            {
                // Nếu chọn lại "--- Chọn Mùa Vụ ---"
                SetDetailControlState(FormState.Viewing); // Về trạng thái xem mặc định
                btnSua.Enabled = false; // Không thể sửa/xóa khi chưa chọn mùa vụ
                btnXoa.Enabled = false;
                btnLuu.Enabled = false;
                // Nút Thêm vẫn có thể bật nếu Ruộng đã được chọn
                btnThem.Enabled = (cmbRuong.SelectedIndex > 0);
            }
        }

        // --- Data Loading and Clearing ---
        private void LoadDataForDisplay(string idMuaVuToLoad)
        {
            if (string.IsNullOrEmpty(idMuaVuToLoad)) return;

            DataTable dt = modify.GetThongTinMuaVuChiTiet(idMuaVuToLoad);
            ClearFormControls(); // Xóa trước khi load

            if (dt != null && dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                try
                {
                    // Gán dữ liệu - Đảm bảo tên cột khớp với DataTable trả về
                    txtIDMuaVu.Text = row["MV_IDMuaVu"]?.ToString();
                    txtTenMuaVu.Text = row["MV_TenMuaVu"]?.ToString();
                    txtTenCayTrong.Text = row["CT_TenCay"]?.ToString(); // Đổi CT_TenCay nếu tên cột khác
                    txtTrangThai.Text = row["MV_TrangThai"]?.ToString();

                    // DateTimePickers (Giả sử ShowCheckBox = true)
                    SetDateTimePickerValue(dateTimePicker1, row["MV_NgayGieo"]);
                    SetDateTimePickerValue(dateTimePicker2, row["MV_NgayDuKienThuHoach"]); // Đổi tên cột nếu cần
                    SetDateTimePickerValue(dateTimePicker3, row["TH_NgayThuHoach"]);       // Đổi tên cột nếu cần

                    // Numbers (decimal/float)
                    txtSLDuKien.Text = GetDecimalString(row["TH_SanLuongDuKien"]); // Đổi tên cột nếu cần
                    txtSLThucTe.Text = GetDecimalString(row["TH_SanLuongThucTe"]); // Đổi tên cột nếu cần

                    // Ruộng Info (có thể lấy từ cmbRuong thay vì load lại)
                    txtIDRuong.Text = row["R_IDRuong"]?.ToString();
                    txtDienTich.Text = GetDecimalString(row["R_DienTich"], "N3"); // Ví dụ 3 chữ số thập phân
                    txtLoaiDat.Text = row["R_LoaiDat"]?.ToString();
                    txtViTtri.Text = row["R_ViTri"]?.ToString();

                    // Chủ Ruộng Info
                    txtIDChuRuong.Text = row["CR_IDChuRuong"]?.ToString();
                    txtTenChuRuong.Text = row["CR_HoTen"]?.ToString();

                    // Sau khi load xong, khóa các control lại (chế độ xem)
                    SetDetailControlState(false);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Lỗi khi hiển thị dữ liệu Mùa Vụ: {ex.Message}", "Lỗi Hiển Thị", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    ClearFormControls();
                }
            }
            else
            {
                MessageBox.Show($"Không tìm thấy thông tin chi tiết cho Mùa Vụ ID: {idMuaVuToLoad}", "Không Tìm Thấy", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                ClearFormControls();
            }
        }

        private void ClearFormControls()
        {
            txtIDMuaVu.Text = "";
            txtTenMuaVu.Text = "";
            dateTimePicker1.Value = DateTime.Now; dateTimePicker1.Checked = false; // Bỏ check nếu không có giá trị
            dateTimePicker2.Value = DateTime.Now; dateTimePicker2.Checked = false;
            dateTimePicker3.Value = DateTime.Now; dateTimePicker3.Checked = false;
            txtTenCayTrong.Text = "";
            txtTrangThai.Text = "";
            txtSLDuKien.Text = "";
            txtSLThucTe.Text = "";

            // Thông tin ruộng có thể giữ lại hoặc xóa tùy ý
            txtIDRuong.Text = cmbRuong.SelectedIndex > 0 ? cmbRuong.SelectedValue.ToString() : ""; // Lấy từ combobox nếu đã chọn
            txtIDChuRuong.Text = _currentCrId; // Có thể điền sẵn ID chủ ruộng
            txtDienTich.Text = ""; // Các thông tin này thường gắn với Ruộng
            txtLoaiDat.Text = "";
            txtViTtri.Text = "";
        }

        // Helper để gán giá trị DateTimePicker và xử lý null
        private void SetDateTimePickerValue(DateTimePicker dtp, object dbValue)
        {
            if (dbValue != null && dbValue != DBNull.Value)
            {
                dtp.Value = Convert.ToDateTime(dbValue);
                dtp.Checked = true; // Đánh dấu là có giá trị
            }
            else
            {
                dtp.Value = DateTime.Now; // Đặt giá trị mặc định nào đó
                dtp.Checked = false; // Bỏ check để chỉ ra là null
            }
        }

        // Helper để lấy chuỗi từ giá trị decimal/numeric và xử lý null
        private string GetDecimalString(object dbValue, string format = "N2") // Mặc định 2 chữ số thập phân
        {
            if (dbValue != null && dbValue != DBNull.Value)
            {
                try { return Convert.ToDecimal(dbValue).ToString(format); }
                catch { return "Lỗi"; } // Hoặc string.Empty
            }
            return string.Empty;
        }

        // Helper để parse chuỗi thành decimal? (nullable decimal)
        private decimal? ParseNullableDecimal(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            // Sử dụng CultureInfo.InvariantCulture để đảm bảo dấu thập phân là '.'
            if (decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
            {
                return result;
            }
            return null; // Trả về null nếu parse thất bại
        }

        // --- State Management ---
        private void SetDetailControlState(FormState newState)
        {
            _currentState = newState;
            bool isDataSelected = (cmbMuaVu.DataSource != null && cmbMuaVu.SelectedIndex > 0);
            bool isRuongSelected = (cmbRuong.DataSource != null && cmbRuong.SelectedIndex > 0);

            // Khóa/Mở các control nhập liệu chi tiết
            SetDetailControlState(newState != FormState.Viewing);

            // Điều khiển các nút và combobox
            switch (_currentState)
            {
                case FormState.Viewing:
                    btnThem.Enabled = isRuongSelected;
                    btnSua.Enabled = isDataSelected;
                    btnXoa.Enabled = isDataSelected;
                    btnLuu.Enabled = false;
                    // btnHuy.Enabled = false;
                    cmbRuong.Enabled = true;
                    cmbMuaVu.Enabled = isRuongSelected;
                    txtIDMuaVu.ReadOnly = true; // Luôn khóa ID khi xem
                    break;

                case FormState.Adding:
                    txtIDMuaVu.ReadOnly = false; // *** MỞ KHÓA ID KHI THÊM ***
                    btnThem.Enabled = false;
                    btnSua.Enabled = false;
                    btnXoa.Enabled = false;
                    btnLuu.Enabled = true;
                    // btnHuy.Enabled = true;
                    cmbRuong.Enabled = false;
                    cmbMuaVu.Enabled = false;
                    txtIDMuaVu.Focus(); // Focus vào ô nhập ID đầu tiên
                    break;

                case FormState.Editing:
                    txtIDMuaVu.ReadOnly = true; // *** KHÓA ID KHI SỬA ***
                    btnThem.Enabled = false;
                    btnSua.Enabled = false;
                    btnXoa.Enabled = false;
                    btnLuu.Enabled = true;
                    // btnHuy.Enabled = true;
                    cmbRuong.Enabled = false;
                    cmbMuaVu.Enabled = false;
                    txtTenMuaVu.Focus(); // Focus vào ô Tên
                    break;
            }
            // Luôn khóa các thông tin của Ruộng và Chủ ruộng ở form này
            txtIDRuong.ReadOnly = true;
            txtIDChuRuong.ReadOnly = true;
            txtDienTich.ReadOnly = true;
            txtLoaiDat.ReadOnly = true;
            txtViTtri.ReadOnly = true;
        }

        // Helper để bật/tắt trạng thái ReadOnly/Enabled cho các control nhập liệu
        private void SetDetailControlState(bool enabled)
        {
            // Chỉ bật/tắt các control NHẬP LIỆU chi tiết Mùa Vụ
            // txtIDMuaVu sẽ được quản lý riêng trong SetDetailControlState
            txtTenMuaVu.ReadOnly = !enabled;
            txtTenCayTrong.ReadOnly = !enabled;
            txtTrangThai.ReadOnly = !enabled;
            txtSLDuKien.ReadOnly = !enabled;
            txtSLThucTe.ReadOnly = !enabled;
            dateTimePicker1.Enabled = enabled;
            dateTimePicker2.Enabled = enabled;
            dateTimePicker3.Enabled = enabled;
        }


        // --- Button Click Events ---
        

        // Hàm phụ trợ để load chi tiết ruộng vào ô readonly khi thêm mùa vụ mới
        private void LoadRuongDetailsForAdding()
        {
            // Bạn có thể lấy các giá trị từ DataRow của cmbRuong.SelectedItem
            // Hoặc truy vấn lại CSDL nếu cần thông tin đầy đủ hơn
            if (cmbRuong.SelectedIndex > 0 && cmbRuong.SelectedItem is DataRowView drv)
            {
                // Giả sử các cột này tồn tại trong DataTable nguồn của cmbRuong
                // txtDienTich.Text = GetDecimalString(drv.Row["R_DienTich"], "N3");
                // txtLoaiDat.Text = drv.Row["R_LoaiDat"]?.ToString();
                // txtViTtri.Text = drv.Row["R_ViTri"]?.ToString();
                // Tạm thời để trống hoặc bạn có thể bỏ các textbox này nếu không cần thiết khi thêm/sửa mùa vụ
                txtDienTich.Text = "";
                txtLoaiDat.Text = "";
                txtViTtri.Text = "";
            }
        }

        // (Tùy chọn) Thêm nút Hủy để quay lại trạng thái Viewing

        private void btnThem_Click_1(object sender, EventArgs e)
        {
            if (cmbRuong.SelectedIndex <= 0 || cmbRuong.SelectedValue == DBNull.Value)
            {
                MessageBox.Show("Vui lòng chọn một Ruộng trước khi thêm Mùa Vụ.", "Chưa Chọn Ruộng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            // Chuyển trạng thái sang Adding (sẽ mở khóa txtIDMuaVu)
            SetDetailControlState(FormState.Adding);
            // Xóa trắng form, chuẩn bị nhập liệu
            ClearFormControls();
            txtIDRuong.Text = cmbRuong.SelectedValue.ToString();
            txtIDChuRuong.Text = _currentCrId;
            LoadRuongDetailsForAdding(); // Nạp thông tin ruộng nếu cần
                                         // txtIDMuaVu sẽ được focus do lệnh trong SetDetailControlState(Adding)
        }

        private void btnSua_Click(object sender, EventArgs e)
        {
            if (_currentState == FormState.Viewing && cmbMuaVu.DataSource != null && cmbMuaVu.SelectedIndex > 0)
            {
                SetDetailControlState(FormState.Editing); // Chuyển sang sửa (ID bị khóa)
            }
            else if (_currentState != FormState.Viewing) { /* Đã ở trạng thái sửa/thêm */ }
            else
            {
                MessageBox.Show("Vui lòng chọn một Mùa Vụ để sửa.", "Chưa Chọn Mùa Vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            // Chỉ xóa khi đang ở trạng thái xem và có mùa vụ được chọn
            if (_currentState != FormState.Viewing || cmbMuaVu.DataSource == null || cmbMuaVu.SelectedIndex <= 0 || string.IsNullOrEmpty(txtIDMuaVu.Text))
            {
                MessageBox.Show("Vui lòng chọn một Mùa Vụ hợp lệ để xóa.", "Chưa Chọn Mùa Vụ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string mvIdToDelete = txtIDMuaVu.Text;
            string mvNameToDelete = txtTenMuaVu.Text;

            if (MessageBox.Show($"Bạn có chắc chắn muốn xóa Mùa Vụ:\nID: {mvIdToDelete}\nTên: {mvNameToDelete}?",
                                "Xác Nhận Xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                bool success = modify.DeleteMuaVu(mvIdToDelete);
                if (success)
                {
                    MessageBox.Show("Xóa Mùa Vụ thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Làm mới combobox Mùa Vụ và xóa chi tiết
                    string selectedRuongID = cmbRuong.SelectedValue?.ToString();
                    ClearFormControls(); // Xóa dữ liệu hiển thị cũ
                    SetDetailControlState(false); // Khóa control lại
                    cmbMuaVu.DataSource = null; // Reset cmbMuaVu
                    cmbMuaVu.Items.Clear();
                    cmbMuaVu.Enabled = false;
                    if (!string.IsNullOrEmpty(selectedRuongID))
                    {
                        LoadMuaVuComboBox(selectedRuongID); // Load lại danh sách mùa vụ cho ruộng đó
                    }
                    SetDetailControlState(FormState.Viewing); // Quay về trạng thái xem
                }
                // else Lỗi đã được báo trong Modify.DeleteMuaVu
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            // --- Validation ---
            string idMuaVu = txtIDMuaVu.Text.Trim(); // Lấy ID người dùng nhập

            if (_currentState == FormState.Adding)
            {
                // 1. Kiểm tra ID trống
                if (string.IsNullOrWhiteSpace(idMuaVu))
                {
                    MessageBox.Show("ID Mùa Vụ không được để trống.", "Thiếu Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtIDMuaVu.Focus();
                    return;
                }
                // 2. Kiểm tra định dạng ID (ví dụ: đúng 4 ký tự)
                if (idMuaVu.Length != 4) // Giả sử CSDL là CHAR(4)
                {
                    MessageBox.Show("ID Mùa Vụ phải có đúng 4 ký tự.", "Sai Định Dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtIDMuaVu.Focus();
                    return;
                }
                // 3. *** KIỂM TRA ID TỒN TẠI ***
                if (modify.CheckMuaVuExists(idMuaVu)) // Cần hàm này trong Modify.cs
                {
                    MessageBox.Show($"ID Mùa Vụ '{idMuaVu}' đã tồn tại. Vui lòng nhập ID khác.", "Trùng ID", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtIDMuaVu.Focus();
                    return;
                }
            }
            else // Editing
            {
                // Khi sửa, ID lấy từ ô text (đang readonly) phải có giá trị
                if (string.IsNullOrWhiteSpace(idMuaVu))
                {
                    MessageBox.Show("Lỗi: Không xác định được ID Mùa Vụ cần cập nhật.", "Lỗi Dữ Liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            // --- Các bước Validation khác giữ nguyên ---
            if (cmbRuong.SelectedIndex <= 0 || cmbRuong.SelectedValue == DBNull.Value) { MessageBox.Show("Ruộng chưa được chọn hợp lệ.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtTenMuaVu.Text)) { MessageBox.Show("Tên Mùa Vụ không được để trống.", "Thiếu Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtTenMuaVu.Focus(); return; }
            // Kiểm tra trùng tên (chỉ cần khi thêm, hoặc khi sửa nếu tên thay đổi)
            string currentMvIdForCheck = (_currentState == FormState.Editing) ? idMuaVu : null;
            if (modify.CheckTenMuaVuExists(txtTenMuaVu.Text.Trim(), currentMvIdForCheck)) { MessageBox.Show("Tên Mùa Vụ này đã tồn tại. Vui lòng chọn tên khác.", "Trùng Tên", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtTenMuaVu.Focus(); return; }

            if (string.IsNullOrWhiteSpace(txtTenCayTrong.Text)) { MessageBox.Show("Tên Cây Trồng không được để trống.", "Thiếu Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtTenCayTrong.Focus(); return; }
            if (string.IsNullOrWhiteSpace(txtTrangThai.Text)) { MessageBox.Show("Trạng thái không được để trống.", "Thiếu Thông Tin", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtTrangThai.Focus(); return; }

            // Validate Dates Logic... (giữ nguyên)
            DateTime? ngayGieo = dateTimePicker1.Checked ? (DateTime?)dateTimePicker1.Value : null;
            DateTime? ngayDuKien = dateTimePicker2.Checked ? (DateTime?)dateTimePicker2.Value : null;
            DateTime? ngayThuHoach = dateTimePicker3.Checked ? (DateTime?)dateTimePicker3.Value : null;
            if (ngayGieo.HasValue && ngayDuKien.HasValue && ngayDuKien.Value < ngayGieo.Value) { MessageBox.Show("Ngày thu hoạch dự kiến không thể trước ngày gieo.", "Lỗi Ngày Tháng", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (ngayGieo.HasValue && ngayThuHoach.HasValue && ngayThuHoach.Value < ngayGieo.Value) { MessageBox.Show("Ngày thu hoạch thực tế không thể trước ngày gieo.", "Lỗi Ngày Tháng", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            // Parse numbers... (giữ nguyên)
            decimal? slDuKien = ParseNullableDecimal(txtSLDuKien.Text);
            decimal? slThucTe = ParseNullableDecimal(txtSLThucTe.Text);
            if (txtSLDuKien.Text.Length > 0 && !slDuKien.HasValue) { MessageBox.Show("Định dạng Sản lượng dự kiến không hợp lệ.", "Lỗi Định Dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtSLDuKien.Focus(); return; }
            if (txtSLThucTe.Text.Length > 0 && !slThucTe.HasValue) { MessageBox.Show("Định dạng Sản lượng thực tế không hợp lệ.", "Lỗi Định Dạng", MessageBoxButtons.OK, MessageBoxIcon.Warning); txtSLThucTe.Focus(); return; }

            // --- Thu thập dữ liệu ---
            string idRuong = cmbRuong.SelectedValue.ToString();
            string tenMuaVu = txtTenMuaVu.Text.Trim();
            string tenCayTrong = txtTenCayTrong.Text.Trim();
            string trangThai = txtTrangThai.Text.Trim();
            string donViSL = "Tấn"; // Hoặc lấy từ control khác

            // --- Gọi hàm Lưu ---
            bool success = false;
            string mvIdAffected = idMuaVu; // ID bây giờ lấy từ input

            try
            {
                if (_currentState == FormState.Adding)
                {
                    // *** KHÔNG GỌI GenerateNewMuaVuId NỮA ***
                    success = modify.InsertMuaVu(mvIdAffected, tenMuaVu, ngayGieo, ngayDuKien, ngayThuHoach,
                                                 idRuong, tenCayTrong, trangThai, slDuKien, slThucTe, donViSL);
                }
                else // Editing
                {
                    success = modify.UpdateMuaVu(mvIdAffected, tenMuaVu, ngayGieo, ngayDuKien, ngayThuHoach,
                                                 tenCayTrong, trangThai, slDuKien, slThucTe, donViSL);
                }

                // --- Xử lý kết quả (giữ nguyên) ---
                if (success)
                {
                    MessageBox.Show("Lưu thông tin Mùa Vụ thành công!", "Thông Báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    string selectedRuongIdBeforeRefresh = cmbRuong.SelectedValue?.ToString();
                    LoadMuaVuComboBox(selectedRuongIdBeforeRefresh);
                    cmbMuaVu.SelectedValue = mvIdAffected; // Tự động chọn lại
                                                           // SetDetailControlState(FormState.Viewing); // Sự kiện SelectedIndexChanged sẽ làm việc này
                }
                // else Lỗi đã được báo trong hàm Modify
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Đã xảy ra lỗi không mong muốn khi lưu: {ex.Message}", "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}