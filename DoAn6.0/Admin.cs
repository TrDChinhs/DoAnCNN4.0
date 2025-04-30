using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization; 
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; 
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn6._0
{
    public partial class Admin : Form
    {
        Modify modify;
        bool isAddingMode = false; 

        public Admin()
        {
            InitializeComponent();
            modify = new Modify();
        }

        private void Admin_Load(object sender, EventArgs e)
        {
            LoadDataGridView();
            LoadChuRuongComboBox();
            ConfigureDataGridView();
            SetEditMode(false);
            ClearInputFields(); 
        }

        private void ConfigureDataGridView()
        {
            if (dgvRuong.Columns.Contains("R_IDRuong"))
                dgvRuong.Columns["R_IDRuong"].HeaderText = "ID Ruộng";
            if (dgvRuong.Columns.Contains("R_DienTich"))
            {
                dgvRuong.Columns["R_DienTich"].HeaderText = "Diện Tích (m²)";
                dgvRuong.Columns["R_DienTich"].DefaultCellStyle.Format = "N3"; 
                dgvRuong.Columns["R_DienTich"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight; 
            }
            if (dgvRuong.Columns.Contains("R_ViTri"))
                dgvRuong.Columns["R_ViTri"].HeaderText = "Vị Trí";
            if (dgvRuong.Columns.Contains("R_LoaiDat"))
                dgvRuong.Columns["R_LoaiDat"].HeaderText = "Loại Đất";
            if (dgvRuong.Columns.Contains("CR_IDChuRuong"))
                dgvRuong.Columns["CR_IDChuRuong"].HeaderText = "ID Chủ Ruộng";
            if (dgvRuong.Columns.Contains("CR_HoTen"))
                dgvRuong.Columns["CR_HoTen"].HeaderText = "Tên Chủ Ruộng";

            if (dgvRuong.Columns.Contains("R_ViTri")) dgvRuong.Columns["R_ViTri"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (dgvRuong.Columns.Contains("CR_HoTen")) dgvRuong.Columns["CR_HoTen"].Width = 150;
            if (dgvRuong.Columns.Contains("R_IDRuong")) dgvRuong.Columns["R_IDRuong"].Width = 80;
            if (dgvRuong.Columns.Contains("CR_IDChuRuong")) dgvRuong.Columns["CR_IDChuRuong"].Width = 100;

            dgvRuong.AllowUserToAddRows = false; 
            dgvRuong.ReadOnly = true; 
            dgvRuong.SelectionMode = DataGridViewSelectionMode.FullRowSelect; 
            dgvRuong.MultiSelect = false; 
        }

        private void LoadDataGridView()
        {
            try
            {
                DataTable dt = modify.GetAllRuongWithOwnerInfo();
                dgvRuong.DataSource = dt;
                dgvRuong.ClearSelection(); 
                ConfigureDataGridView(); 
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi tải dữ liệu ruộng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void LoadChuRuongComboBox()
        {
            DataTable dtChuRuong = modify.GetAllChuRuong(); // đã có sẵn trong Modify.cs

            if (dtChuRuong != null)
            {
                // Thêm dòng giả đại diện cho "không gán chủ ruộng"
                DataRow emptyRow = dtChuRuong.NewRow();
                emptyRow["CR_IDChuRuong"] = DBNull.Value;
                emptyRow["CR_HoTen"] = "--- Không gán chủ ruộng ---";
                dtChuRuong.Rows.InsertAt(emptyRow, 0);

                cmbChuRuong.DataSource = dtChuRuong;
                cmbChuRuong.DisplayMember = "CR_HoTen";
                cmbChuRuong.ValueMember = "CR_IDChuRuong";
            }
        }

        private void dgvRuong_SelectionChanged(object sender, EventArgs e)
        {
            // Chỉ xử lý nếu có hàng được chọn và hàng đó có dữ liệu (không phải hàng header)
            if (dgvRuong.CurrentRow != null && dgvRuong.CurrentRow.DataBoundItem != null)
            {
                isAddingMode = false; // Chọn một hàng nghĩa là đang ở chế độ sửa
                SetEditMode(true); // Kích hoạt các điều khiển để sửa

                // Lấy dữ liệu hàng được chọn dưới dạng DataRowView
                DataRowView drv = (DataRowView)dgvRuong.CurrentRow.DataBoundItem;

                // --- Điền dữ liệu vào các TextBox ---
                txtIDRuong.Text = drv["R_IDRuong"]?.ToString() ?? ""; // Hiển thị ID Ruộng

                // Định dạng Diện tích khi tải lên TextBox
                if (drv["R_DienTich"] != DBNull.Value && drv["R_DienTich"] != null)
                {
                    decimal dienTich;
                    // Sử dụng CultureInfo.InvariantCulture để đảm bảo dấu thập phân là '.' khi phân tích
                    if (Decimal.TryParse(drv["R_DienTich"].ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out dienTich))
                    {
                        // Hiển thị với dấu thập phân của hệ thống hiện tại và 3 chữ số sau dấu phẩy
                        txtDienTich.Text = dienTich.ToString("N3");
                    }
                    else
                    {
                        txtDienTich.Text = ""; // Xử lý lỗi nếu không parse được
                    }
                }
                else
                {
                    txtDienTich.Text = ""; // Để trống nếu giá trị là NULL
                }

                txtViTri.Text = drv["R_ViTri"]?.ToString() ?? ""; // Hiển thị Vị trí
                txtLoaiDat.Text = drv["R_LoaiDat"]?.ToString() ?? ""; // Hiển thị Loại Đất

                // --- Chọn Chủ Ruộng tương ứng trong ComboBox ---
                object ownerId = drv["CR_IDChuRuong"]; // Lấy ID Chủ ruộng từ dữ liệu
                if (ownerId != DBNull.Value && ownerId != null)
                {
                    // Chọn giá trị trong ComboBox khớp với ownerId
                    cmbChuRuong.SelectedValue = ownerId;
                }
                else
                {
                    // Chọn tùy chọn "Không Gán Chủ" nếu ID là NULL
                    // Tìm index của item có giá trị DBNull.Value
                    for (int i = 0; i < cmbChuRuong.Items.Count; i++)
                    {
                        DataRowView item = cmbChuRuong.Items[i] as DataRowView;
                        if (item != null && item.Row["CR_IDChuRuong"] == DBNull.Value)
                        {
                            cmbChuRuong.SelectedIndex = i;
                            break;
                        }
                    }
                    // Dự phòng nếu không tìm thấy item "Không Gán Chủ" (không nên xảy ra nếu LoadChuRuongComboBox đúng)
                    if (cmbChuRuong.SelectedValue != DBNull.Value) cmbChuRuong.SelectedIndex = -1;
                }

                // --- Đặt trạng thái ReadOnly ---
                txtIDRuong.ReadOnly = true; // Không cho sửa ID khi chọn hàng có sẵn
            }
            else
            {
                // Nếu bỏ chọn (ví dụ: khi lưới rỗng hoặc click vào header),
                // xóa các trường và vô hiệu hóa sửa (trừ khi đang ở chế độ thêm)
                if (!isAddingMode)
                {
                    ClearInputFields();
                    SetEditMode(false);
                }
            }
        }

        // Thiết lập chế độ chỉnh sửa (kích hoạt/vô hiệu hóa controls)
        private void SetEditMode(bool isEdit)
        {
            txtDienTich.ReadOnly = !isEdit;
            txtViTri.ReadOnly = !isEdit;
            txtLoaiDat.ReadOnly = !isEdit;
            cmbChuRuong.Enabled = isEdit;
        }

        // Xóa nội dung các trường nhập liệu
        private void ClearInputFields()
        {
            txtIDRuong.Text = "";
            txtDienTich.Text = "";
            txtViTri.Text = "";
            txtLoaiDat.Text = "";
            cmbChuRuong.SelectedIndex = -1;
        }

        private void txtDienTich_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                e.Handled = false;
                return;
            }
            if (char.IsDigit(e.KeyChar))
            {
                e.Handled = false; 
                return;
            }
            string decimalSeparator = CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            if (e.KeyChar.ToString() == decimalSeparator)
            {
                if (txtDienTich.Text.Contains(decimalSeparator))
                {
                    e.Handled = true;
                }
                else
                {
                    e.Handled = false; 
                }
                return;
            }
            e.Handled = true;
        }

        private void Admin_Load_1(object sender, EventArgs e)
        {
            LoadChuRuongComboBox(); 
            // TODO: This line of code loads data into the 'quanLyNongTraiDataSet1.QLRuong' table. You can move, or remove it, as needed.
            this.qLRuongTableAdapter.Fill(this.quanLyNongTraiDataSet1.QLRuong);
        }

        private void btnLamMoi_Click_1(object sender, EventArgs e)
        {
            LoadDataGridView(); // Tải lại lưới
            LoadChuRuongComboBox(); // Tải lại danh sách chủ ruộng
            ClearInputFields(); // Xóa các trường nhập liệu
            SetEditMode(false); // Vô hiệu hóa chỉnh sửa sau khi làm mới
            isAddingMode = false; // Thoát chế độ thêm (nếu có)
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            isAddingMode = true;
            ClearInputFields();
            SetEditMode(true);
            txtIDRuong.ReadOnly = false;
            dgvRuong.ClearSelection();
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            string idRuong = txtIDRuong.Text.Trim();
            string viTri = txtViTri.Text.Trim();
            string loaiDat = txtLoaiDat.Text.Trim();
            decimal dienTich;
            object idChuRuong = cmbChuRuong.SelectedValue ?? DBNull.Value;

            if (string.IsNullOrEmpty(idRuong))
            {
                MessageBox.Show("Vui lòng nhập mã ruộng!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtDienTich.Text, out dienTich))
            {
                MessageBox.Show("Vui lòng nhập diện tích hợp lệ!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<SqlParameter> parameters = new List<SqlParameter>
            {
                new SqlParameter("@IDRuong", idRuong),
                new SqlParameter("@DienTich", dienTich),
                new SqlParameter("@ViTri", viTri),
                new SqlParameter("@LoaiDat", loaiDat),
                new SqlParameter("@IDChuRuong", idChuRuong)
            };

            string query = isAddingMode
                ? "INSERT INTO QLRuong (R_IDRuong, R_DienTich, R_ViTri, R_LoaiDat, CR_IDChuRuong) VALUES (@IDRuong, @DienTich, @ViTri, @LoaiDat, @IDChuRuong)"
                : "UPDATE QLRuong SET R_DienTich=@DienTich, R_ViTri=@ViTri, R_LoaiDat=@LoaiDat, CR_IDChuRuong=@IDChuRuong WHERE R_IDRuong=@IDRuong";

            int result = modify.ExecuteCommand(query, parameters);
            if (result > 0)
            {
                MessageBox.Show("Lưu dữ liệu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDataGridView();
                ClearInputFields();
                SetEditMode(false);
                isAddingMode = false;
            }
            else
            {
                MessageBox.Show("Lưu dữ liệu thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnXoa_Click_1(object sender, EventArgs e)
        {
            if (dgvRuong.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn một ruộng để xóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Lấy IDRuong và kiểm tra null hoặc trống
            object cellValue = dgvRuong.CurrentRow.Cells["R_IDRuong"].Value;
            if (cellValue == null || string.IsNullOrWhiteSpace(cellValue.ToString()))
            {
                MessageBox.Show("Mã ruộng không hợp lệ hoặc trống!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string rIdToDelete = cellValue.ToString();

            DialogResult confirm = MessageBox.Show($"Bạn có chắc muốn xóa ruộng '{rIdToDelete}' không?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                bool success = modify.DeleteRuong(rIdToDelete);
                if (success)
                {
                    MessageBox.Show("Xóa thành công!");
                    LoadDataGridView();
                    ClearInputFields();
                }
            }
        }
    }
}