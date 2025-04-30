using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DoAn6._0
{
    internal class Modify
    {
        public Modify() { }

        public int ExecuteCommand(string query, List<SqlParameter> parameters)
        {
            int affectedRows = 0;
            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        if (parameters != null) { cmd.Parameters.AddRange(parameters.ToArray()); }
                        affectedRows = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Lỗi SQL khi thực thi: {query} - Lỗi: {ex.Message} (Số lỗi: {ex.Number})");
                    affectedRows = -1; 
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi thực thi lệnh: {query} - Lỗi: {ex.Message}");
                    affectedRows = -2; 
                }
            }
            return affectedRows;
        }

        public bool CheckLogin(string tenTaiKhoan, string matKhau)
        {
            bool isValid = false;
            // Using COLLATE for case-sensitive comparison if needed (adjust collation if your DB is different)
            string query = "SELECT COUNT(*) FROM TaiKhoan WHERE TK_TenTaiKhoan = @TenTaiKhoan COLLATE SQL_Latin1_General_CP1_CS_AS AND TK_MatKhau = @MatKhau COLLATE SQL_Latin1_General_CP1_CS_AS";
            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@TenTaiKhoan", tenTaiKhoan);
                        cmd.Parameters.AddWithValue("@MatKhau", matKhau);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            isValid = (Convert.ToInt32(result) > 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi kiểm tra đăng nhập cho {tenTaiKhoan}: {ex.Message}");
                    isValid = false;
                }
            }
            return isValid;
        }

        public bool DoesAccountExist(string username, string email)
        {
            int count = 0;
            // Check specifically for username OR email
            string query = "SELECT COUNT(*) FROM TaiKhoan WHERE TK_TenTaiKhoan = @TenTaiKhoan OR TK_Email = @Email";
            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Handle potential nulls if checking only one field sometimes
                        cmd.Parameters.AddWithValue("@TenTaiKhoan", string.IsNullOrEmpty(username) ? (object)DBNull.Value : username);
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi kiểm tra tài khoản tồn tại (User: {username}, Email: {email}): {ex.Message}");
                    // Depending on desired behavior on error, might return true to prevent action
                    return true; // Assume exists if DB error occurs during check
                }
            }
            return count > 0;
        }

        public bool DoesEmailExist(string email)
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM TaiKhoan WHERE TK_Email = @Email";
            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@Email", string.IsNullOrEmpty(email) ? (object)DBNull.Value : email);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi kiểm tra email tồn tại ({email}): {ex.Message}");
                    // Return false on DB error, as we can't confirm existence
                    return false;
                }
            }
            return count > 0;
        }

        private string GetNextCR_IDChuRuong(SqlConnection connection, SqlTransaction transaction)
        {
            string nextId = "CR01"; // Default starting ID
            // Query to find the highest numeric part of existing IDs like 'CR##'
            string query = "SELECT MAX(TRY_CAST(SUBSTRING(CR_IDChuRuong, 3, 2) AS INT)) FROM dbo.QLChuRuong WITH (TABLOCKX, HOLDLOCK) WHERE CR_IDChuRuong LIKE 'CR[0-9][0-9]'";
            try
            {
                using (SqlCommand cmd = new SqlCommand(query, connection, transaction))
                {
                    object result = cmd.ExecuteScalar();
                    int maxNumber = 0;
                    if (result != null && result != DBNull.Value)
                    {
                        maxNumber = (int)result;
                    }
                    int nextNumber = maxNumber + 1;
                    nextId = $"CR{nextNumber:D2}"; // Format as CR01, CR02, ..., CR10, ...
                    // Basic check to prevent overflow if you have many users
                    if (nextNumber > 99) // Assuming CR## format limit
                    {
                        throw new Exception("Không thể tạo ID Chủ ruộng mới, đã đạt giới hạn CR99.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi khi lấy Next CR_IDChuRuong: {ex.Message}");
                throw; // Re-throw to be caught by the transaction handler
            }
            return nextId;
        }

        public string RegisterAccountWithTransaction(string tenTaiKhoan, string email, string matKhauPlainText)
        {
            string newCRID = null;
            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                sqlConnection.Open();
                SqlTransaction transaction = sqlConnection.BeginTransaction();
                try
                {
                    // Bước 0: Lấy ID Chủ ruộng tiếp theo
                    newCRID = GetNextCR_IDChuRuong(sqlConnection, transaction);

                    // Bước 1: Thêm vào bảng TaiKhoan (CR_IDChuRuong tạm thời là NULL)
                    string queryTaiKhoan = "INSERT INTO TaiKhoan (TK_TenTaiKhoan, TK_MatKhau, TK_Email, CR_IDChuRuong) VALUES (@TenTK, @MatKhau, @Email, NULL)";
                    using (SqlCommand cmdTaiKhoan = new SqlCommand(queryTaiKhoan, sqlConnection, transaction))
                    {
                        cmdTaiKhoan.Parameters.Add(new SqlParameter("@TenTK", SqlDbType.NVarChar, 50) { Value = tenTaiKhoan }); // Specify length
                        cmdTaiKhoan.Parameters.Add(new SqlParameter("@MatKhau", SqlDbType.NVarChar, 255) { Value = matKhauPlainText }); // Specify length
                        cmdTaiKhoan.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = email }); // Specify length
                        if (cmdTaiKhoan.ExecuteNonQuery() <= 0) throw new Exception("Bước 1 thất bại: Không thể thêm vào bảng TaiKhoan.");
                        Console.WriteLine($"Bước 1: Inserted TaiKhoan {tenTaiKhoan} with NULL CR_IDChuRuong.");
                    }

                    // Bước 2: Thêm vào bảng QLChuRuong (Đã bỏ CR_TenTaiKhoan)
                    string queryChuRuong = "INSERT INTO QLChuRuong (CR_IDChuRuong, CR_HoTen) VALUES (@IDChuRuong, @HoTen)";
                    using (SqlCommand cmdChuRuong = new SqlCommand(queryChuRuong, sqlConnection, transaction))
                    {
                        cmdChuRuong.Parameters.Add(new SqlParameter("@IDChuRuong", SqlDbType.Char, 4) { Value = newCRID });
                        cmdChuRuong.Parameters.Add(new SqlParameter("@HoTen", SqlDbType.NVarChar, 50) { Value = tenTaiKhoan }); // Default HoTen to TenTK initially
                        // Đã bỏ Parameter @TenTaiKhoan
                        if (cmdChuRuong.ExecuteNonQuery() <= 0) throw new Exception("Bước 2 thất bại: Không thể thêm vào bảng QLChuRuong.");
                        Console.WriteLine($"Bước 2: Inserted QLChuRuong {newCRID}.");
                    }

                    // Bước 3: Cập nhật CR_IDChuRuong trong TaiKhoan
                    string queryUpdateTaiKhoan = "UPDATE TaiKhoan SET CR_IDChuRuong = @IDChuRuong WHERE TK_TenTaiKhoan = @TenTK";
                    using (SqlCommand cmdUpdateTaiKhoan = new SqlCommand(queryUpdateTaiKhoan, sqlConnection, transaction))
                    {
                        cmdUpdateTaiKhoan.Parameters.Add(new SqlParameter("@IDChuRuong", SqlDbType.Char, 4) { Value = newCRID });
                        cmdUpdateTaiKhoan.Parameters.Add(new SqlParameter("@TenTK", SqlDbType.NVarChar, 50) { Value = tenTaiKhoan }); // Specify length
                        if (cmdUpdateTaiKhoan.ExecuteNonQuery() <= 0) throw new Exception("Bước 3 thất bại: Không thể cập nhật CR_IDChuRuong trong TaiKhoan.");
                        Console.WriteLine($"Bước 3: Updated TaiKhoan {tenTaiKhoan} with CR_IDChuRuong {newCRID}.");
                    }

                    transaction.Commit();
                    Console.WriteLine($"Đăng ký thành công cho {tenTaiKhoan} với CR_ID {newCRID}. Transaction committed.");
                    return newCRID; // Trả về CR_ID đã tạo
                }
                catch (Exception ex)
                {
                    string detailedError = $"Lỗi chi tiết trong Transaction Đăng ký:\n{ex.ToString()}"; // Log detailed error
                    Console.WriteLine(detailedError);
                    MessageBox.Show($"Đăng ký thất bại. Đã xảy ra lỗi: {ex.Message}", "Lỗi Transaction Đăng Ký", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    try { transaction.Rollback(); Console.WriteLine("Transaction rolled back."); }
                    catch (Exception rollbackEx) { Console.WriteLine($"Lỗi khi Rollback: {rollbackEx.Message}"); }
                    return null; // Trả về null khi thất bại
                }
            }
        }


        public DataRow GetChuRuongInfo(string tenTaiKhoan)
        {
            DataRow dr = null;
            // Query đã sửa để JOIN và lấy thông tin qua tenTaiKhoan
            string query = @"SELECT CR.CR_HoTen, CR.CR_NgaySinh, CR.CR_GioiTinh, CR.CR_CMND, CR.CR_DiaChi
                           FROM QLChuRuong CR
                           INNER JOIN TaiKhoan TK ON CR.CR_IDChuRuong = TK.CR_IDChuRuong
                           WHERE TK.TK_TenTaiKhoan = @TenTaiKhoan";
            DataTable dataTable = new DataTable();

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@TenTaiKhoan", tenTaiKhoan);
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                    if (dataTable.Rows.Count > 0)
                    {
                        dr = dataTable.Rows[0];
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy thông tin Chủ Ruộng cho tài khoản {tenTaiKhoan}: {ex.Message}");
                    MessageBox.Show($"Không thể tải thông tin tài khoản: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return dr;
        }

        public bool UpdateChuRuongInfo(string crIdChuRuong, string hoTen, DateTime? ngaySinh, string gioiTinh, string cmnd, string diaChi)
        {
            int affectedRows = 0;
            // Query đã sửa để dùng CR_IDChuRuong trong WHERE
            string query = @"UPDATE QLChuRuong
                           SET CR_HoTen = @HoTen,
                               CR_NgaySinh = @NgaySinh,
                               CR_GioiTinh = @GioiTinh,
                               CR_CMND = @CMND,
                               CR_DiaChi = @DiaChi
                           WHERE CR_IDChuRuong = @IDChuRuong";

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Thêm tham số @IDChuRuong
                        cmd.Parameters.Add(new SqlParameter("@IDChuRuong", SqlDbType.Char, 4) { Value = crIdChuRuong });
                        cmd.Parameters.AddWithValue("@HoTen", string.IsNullOrEmpty(hoTen) ? (object)DBNull.Value : hoTen);
                        cmd.Parameters.AddWithValue("@NgaySinh", ngaySinh.HasValue ? (object)ngaySinh.Value : (object)DBNull.Value);
                        cmd.Parameters.AddWithValue("@GioiTinh", string.IsNullOrEmpty(gioiTinh) ? (object)DBNull.Value : gioiTinh);
                        cmd.Parameters.AddWithValue("@CMND", string.IsNullOrEmpty(cmnd) ? (object)DBNull.Value : cmnd);
                        cmd.Parameters.AddWithValue("@DiaChi", string.IsNullOrEmpty(diaChi) ? (object)DBNull.Value : diaChi);
                        // Đã bỏ tham số @TenTaiKhoan

                        affectedRows = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    // Sửa log lỗi để dùng crIdChuRuong
                    Console.WriteLine($"Lỗi SQL khi cập nhật Chủ Ruộng ({crIdChuRuong}): {ex.Message} (Số lỗi: {ex.Number})");
                    MessageBox.Show($"Lỗi SQL khi cập nhật: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    // Sửa log lỗi để dùng crIdChuRuong
                    Console.WriteLine($"Lỗi không xác định khi cập nhật Chủ Ruộng ({crIdChuRuong}): {ex.Message}");
                    MessageBox.Show($"Lỗi không xác định: {ex.Message}", "Lỗi Hệ Thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return affectedRows > 0;
        }


        public DataTable GetThongTinMuaVuChiTiet(string idMuaVu) // Không bị ảnh hưởng
        {
            DataTable dataTable = new DataTable();
            // --- SỬA CÂU QUERY Ở ĐÂY ---
            string query = @"
                    SELECT
                        MV.MV_IDMuaVu, MV.MV_TenMuaVu, MV.MV_ThoiGianGieo AS MV_NgayGieo,
                        MV.MV_ThoiGianThuHoachDuKien AS MV_NgayDuKienThuHoach,
                        MV.MV_ThoiGianThuHoachThucTe AS TH_NgayThuHoach,
                        MV.MV_TenCayTrong AS CT_TenCay, MV.MV_TrangThai,
                        MV.MV_SanLuongDuKien AS TH_SanLuongDuKien,
                        MV.MV_SanLuongThucTe AS TH_SanLuongThucTe,
                        MV.MV_DonViSanLuong,
                        R.R_IDRuong, R.R_DienTich, R.R_ViTri, R.R_LoaiDat,
                        CR.CR_IDChuRuong,
                        CR.CR_HoTen -- <<< THÊM DÒNG NÀY ĐỂ LẤY TÊN CHỦ RUỘNG
                    FROM QLMuaVu AS MV
                    INNER JOIN QLRuong AS R ON MV.R_IDRuong = R.R_IDRuong
                    INNER JOIN QLChuRuong AS CR ON R.CR_IDChuRuong = CR.CR_IDChuRuong
                    WHERE MV.MV_IDMuaVu = @IDMuaVu";
            // --- HẾT PHẦN SỬA QUERY ---

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@IDMuaVu", idMuaVu); // Đảm bảo ID đúng kiểu CHAR(4) nếu cần
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd)) { adapter.Fill(dataTable); }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy thông tin chi tiết Mùa Vụ {idMuaVu}: {ex.Message}");
                    MessageBox.Show($"Không thể tải thông tin chi tiết: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null;
                }
            }
            return dataTable;
        }

        public string GetChuRuongNameById(string crId)
        {
            string hoTen = null;
            string query = "SELECT CR_HoTen FROM QLChuRuong WHERE CR_IDChuRuong = @ID";

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Char, 4) { Value = crId });
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            hoTen = result.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy tên Chủ ruộng {crId}: {ex.Message}");
                    // Không cần báo MessageBox ở đây, trả về null là đủ
                }
            }
            return hoTen; // Trả về tên hoặc null nếu không tìm thấy/lỗi
        }
        public string GetCrIdFromUsername(string tenTaiKhoan) // Không bị ảnh hưởng
        {
            string crId = null;
            string query = "SELECT CR_IDChuRuong FROM TaiKhoan WHERE TK_TenTaiKhoan = @TenTaiKhoan";
            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.AddWithValue("@TenTaiKhoan", tenTaiKhoan);
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            crId = result.ToString();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy CR_IDChuRuong cho {tenTaiKhoan}: {ex.Message}");
                    // Handle error if needed, e.g., return null or throw
                }
            }
            return crId;
        }

        // ================================================================
        // --- BEGIN: Các phương thức mới cho Admin Dashboard ---
        // ================================================================

        public DataTable GetAllChuRuong()
        {
            DataTable dataTable = new DataTable();
            // Lấy ID và Tên, loại trừ admin (nếu CR_IDChuRuong là NULL)
            string query = "SELECT CR_IDChuRuong, CR_HoTen FROM QLChuRuong WHERE CR_IDChuRuong IS NOT NULL ORDER BY CR_HoTen";

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy danh sách tất cả Chủ ruộng: {ex.Message}");
                    MessageBox.Show($"Không thể tải danh sách chủ ruộng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null; // Trả về null nếu có lỗi
                }
            }
            return dataTable;
        }

        public DataTable GetAllRuongWithOwnerInfo()
        {
            DataTable dataTable = new DataTable();
            // Lấy tất cả cột từ QLRuong và thêm cột Họ tên từ QLChuRuong
            // Dùng LEFT JOIN để vẫn lấy được ruộng chưa có chủ
            string query = @"SELECT
                                R.R_IDRuong, R.R_DienTich, R.R_ViTri, R.R_LoaiDat,
                                R.CR_IDChuRuong,
                                ISNULL(CR.CR_HoTen, '[Chưa gán chủ]') AS CR_HoTen
                             FROM QLRuong R
                             LEFT JOIN QLChuRuong CR ON R.CR_IDChuRuong = CR.CR_IDChuRuong
                             ORDER BY R.R_IDRuong";

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy danh sách tất cả Ruộng kèm chủ sở hữu: {ex.Message}");
                    MessageBox.Show($"Không thể tải danh sách ruộng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null; // Trả về null nếu có lỗi
                }
            }
            return dataTable;
        }

        public bool CheckRuongExists(string rId)
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM QLRuong WHERE R_IDRuong = @ID";
            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // ID Ruộng là char(4)
                        cmd.Parameters.Add(new SqlParameter("@ID", SqlDbType.Char, 4) { Value = rId });
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi kiểm tra ID Ruộng tồn tại ({rId}): {ex.Message}");
                    // Trả về true khi lỗi để ngăn việc cố gắng thêm trùng lặp
                    return true;
                }
            }
            return count > 0;
        }

        public bool DeleteRuong(string rId)
        {
            // Trước khi xóa, kiểm tra xem có Mùa Vụ nào đang tham chiếu đến Ruộng này không
            string checkQuery = "SELECT COUNT(*) FROM QLMuaVu WHERE R_IDRuong = @IDRuong";
            int muaVuCount = 0;
            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, sqlConnection))
                    {
                        checkCmd.Parameters.Add(new SqlParameter("@IDRuong", SqlDbType.Char, 4) { Value = rId });
                        object result = checkCmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            muaVuCount = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi kiểm tra Mùa Vụ liên quan đến Ruộng {rId}: {ex.Message}");
                    MessageBox.Show($"Lỗi khi kiểm tra dữ liệu liên quan: {ex.Message}\nKhông thể tiếp tục xóa.", "Lỗi Kiểm Tra", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false; // Không thể xóa nếu không kiểm tra được
                }
            }

            if (muaVuCount > 0)
            {
                MessageBox.Show($"Không thể xóa ruộng '{rId}' vì đang có {muaVuCount} mùa vụ liên kết với ruộng này.\nVui lòng xóa các mùa vụ liên quan trước.", "Lỗi Ràng Buộc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Nếu không có Mùa Vụ nào liên kết, tiến hành xóa
            string deleteQuery = "DELETE FROM QLRuong WHERE R_IDRuong = @IDRuong";
            List<SqlParameter> parameters = new List<SqlParameter>

            { new SqlParameter("@IDRuong", SqlDbType.Char, 4) { Value = rId }};

            int affectedRows = ExecuteCommand(deleteQuery, parameters);

            if (affectedRows <= 0)
            {
                Console.WriteLine($"Lỗi hoặc không tìm thấy ruộng khi xóa Ruộng ID: {rId}");
                // ExecuteCommand đã ghi log lỗi, có thể thêm MessageBox nếu cần
                MessageBox.Show($"Không thể xóa ruộng '{rId}'. Đã xảy ra lỗi hoặc không tìm thấy ruộng.", "Lỗi Xóa Ruộng", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return affectedRows > 0;
        }

        public DataTable GetMuaVuListForRuong(string rIdRuong)
        {
            DataTable dataTable = new DataTable();
            // Chỉ lấy ID và Tên Mùa Vụ để hiển thị trong ComboBox
            string query = @"SELECT MV_IDMuaVu, MV_TenMuaVu
                     FROM QLMuaVu
                     WHERE R_IDRuong = @IDRuong
                     ORDER BY MV_TenMuaVu"; // Hoặc sắp xếp theo tiêu chí khác

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Đảm bảo kiểu dữ liệu và kích thước khớp với CSDL (CHAR(4))
                        cmd.Parameters.Add(new SqlParameter("@IDRuong", SqlDbType.Char, 4) { Value = rIdRuong });
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy danh sách Mùa Vụ cho Ruộng {rIdRuong}: {ex.Message}");
                    MessageBox.Show($"Không thể tải danh sách mùa vụ cho ruộng đã chọn: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null; // Trả về null hoặc DataTable trống khi lỗi
                }
            }
            return dataTable;
        }

        /// <summary>
        /// Lấy danh sách Ruộng (ID và Tên/Vị trí) cho một Chủ ruộng cụ thể.
        /// </summary>
        /// <param name="crIdChuRuong">ID của Chủ ruộng.</param>
        /// <returns>DataTable chứa R_IDRuong và R_ViTri, hoặc null nếu lỗi.</returns>
        public DataTable GetRuongListForUser(string crIdChuRuong)
        {
            DataTable dataTable = new DataTable();
            // Lấy ID và Vị trí Ruộng để hiển thị trong ComboBox
            // Sắp xếp theo Vị trí để dễ tìm
            string query = "SELECT R_IDRuong, R_ViTri FROM QLRuong WHERE CR_IDChuRuong = @CRID ORDER BY R_ViTri";

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Đảm bảo kiểu dữ liệu khớp CSDL (ví dụ: CHAR(4))
                        cmd.Parameters.Add(new SqlParameter("@CRID", SqlDbType.Char, 4) { Value = crIdChuRuong });
                        using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy danh sách Ruộng cho Chủ ruộng {crIdChuRuong}: {ex.Message}");
                    MessageBox.Show($"Không thể tải danh sách ruộng: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return null; // Trả về null để báo lỗi
                }
            }
            return dataTable;
        }

        /// <summary>
        /// Xóa một Mùa Vụ khỏi cơ sở dữ liệu dựa trên ID.
        /// </summary>
        /// <param name="idMuaVu">ID Mùa Vụ cần xóa.</param>
        /// <returns>True nếu xóa thành công, False nếu thất bại.</returns>
        public bool DeleteMuaVu(string idMuaVu)
        {
            string query = "DELETE FROM QLMuaVu WHERE MV_IDMuaVu = @IDMV";
            int affectedRows = 0;

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Đảm bảo kiểu dữ liệu khớp CSDL (ví dụ: CHAR(4))
                        cmd.Parameters.Add(new SqlParameter("@IDMV", SqlDbType.Char, 4) { Value = idMuaVu });
                        affectedRows = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Lỗi SQL khi xóa Mùa Vụ {idMuaVu}: {ex.Message} (Số lỗi: {ex.Number})");
                    // Kiểm tra lỗi khóa ngoại nếu cần (mặc dù CSDL nên có ON DELETE CASCADE nếu hợp lý)
                    if (ex.Number == 547) // Lỗi Foreign Key Constraint Violation
                    {
                        MessageBox.Show("Không thể xóa mùa vụ này vì có thể còn dữ liệu liên quan (ví dụ: Chi Phí).", "Lỗi Ràng Buộc", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi SQL khi xóa: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi không xác định khi xóa Mùa Vụ {idMuaVu}: {ex.Message}");
                    MessageBox.Show($"Lỗi hệ thống khi xóa: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return affectedRows > 0;
        }

        /// <summary>
        /// Kiểm tra xem Tên Mùa Vụ đã tồn tại hay chưa (hữu ích khi thêm/sửa).
        /// </summary>
        /// <param name="tenMuaVu">Tên Mùa Vụ cần kiểm tra.</param>
        /// <param name="excludeIdMuaVu">ID Mùa Vụ cần loại trừ khỏi việc kiểm tra (dùng khi cập nhật).</param>
        /// <returns>True nếu tên đã tồn tại, False nếu chưa.</returns>
        public bool CheckTenMuaVuExists(string tenMuaVu, string excludeIdMuaVu = null)
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM QLMuaVu WHERE MV_TenMuaVu = @TenMV";
            // Nếu có ID cần loại trừ (khi đang sửa), thêm điều kiện vào query
            if (!string.IsNullOrEmpty(excludeIdMuaVu))
            {
                query += " AND MV_IDMuaVu != @ExcludeID";
            }

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        cmd.Parameters.Add(new SqlParameter("@TenMV", SqlDbType.NVarChar, 100) { Value = tenMuaVu });
                        if (!string.IsNullOrEmpty(excludeIdMuaVu))
                        {
                            cmd.Parameters.Add(new SqlParameter("@ExcludeID", SqlDbType.Char, 4) { Value = excludeIdMuaVu });
                        }
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi kiểm tra Tên Mùa Vụ tồn tại ('{tenMuaVu}', exclude: {excludeIdMuaVu}): {ex.Message}");
                    // Trả về true khi lỗi để an toàn, tránh việc cố gắng thêm/sửa trùng
                    return true;
                }
            }
            return count > 0;
        }

        /// <summary>
        /// Thêm một Mùa Vụ mới vào cơ sở dữ liệu.
        /// </summary>
        /// <param name="idMuaVu">ID Mùa Vụ mới (phải là duy nhất).</param>
        /// <param name="tenMuaVu">Tên Mùa Vụ.</param>
        /// <param name="ngayGieo">Thời gian gieo (nullable).</param>
        /// <param name="ngayDuKien">Thời gian thu hoạch dự kiến (nullable).</param>
        /// <param name="ngayThuHoach">Thời gian thu hoạch thực tế (nullable).</param>
        /// <param name="idRuong">ID Ruộng liên kết.</param>
        /// <param name="tenCayTrong">Tên cây trồng.</param>
        /// <param name="trangThai">Trạng thái mùa vụ.</param>
        /// <param name="slDuKien">Sản lượng dự kiến (nullable).</param>
        /// <param name="slThucTe">Sản lượng thực tế (nullable).</param>
        /// <param name="donViSL">Đơn vị sản lượng.</param>
        /// <returns>True nếu thêm thành công, False nếu thất bại.</returns>
        public bool InsertMuaVu(string idMuaVu, string tenMuaVu, DateTime? ngayGieo, DateTime? ngayDuKien,
                                DateTime? ngayThuHoach, string idRuong, string tenCayTrong, string trangThai,
                                decimal? slDuKien, decimal? slThucTe, string donViSL)
        {
            // **QUAN TRỌNG:** Đảm bảo các tên cột (MV_IDMuaVu, MV_TenMuaVu,...) khớp với CSDL của bạn.
            string query = @"INSERT INTO QLMuaVu
                               (MV_IDMuaVu, MV_TenMuaVu, MV_ThoiGianGieo, MV_ThoiGianThuHoachDuKien, MV_ThoiGianThuHoachThucTe,
                                R_IDRuong, MV_TenCayTrong, MV_TrangThai, MV_SanLuongDuKien, MV_SanLuongThucTe, MV_DonViSanLuong)
                             VALUES
                               (@IDMV, @TenMV, @NgayGieo, @NgayDuKien, @NgayTH,
                                @IDRuong, @TenCay, @TrangThai, @SLDK, @SLTT, @DonVi)";
            int affectedRows = 0;

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Thêm Parameters - Đảm bảo kiểu dữ liệu và kích thước khớp CSDL
                        cmd.Parameters.Add(new SqlParameter("@IDMV", SqlDbType.Char, 4) { Value = idMuaVu }); // Ví dụ CHAR(4)
                        cmd.Parameters.Add(new SqlParameter("@TenMV", SqlDbType.NVarChar, 100) { Value = (object)tenMuaVu ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@NgayGieo", SqlDbType.Date) { Value = (object)ngayGieo ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@NgayDuKien", SqlDbType.Date) { Value = (object)ngayDuKien ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@NgayTH", SqlDbType.Date) { Value = (object)ngayThuHoach ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@IDRuong", SqlDbType.Char, 4) { Value = idRuong }); // Ví dụ CHAR(4)
                        cmd.Parameters.Add(new SqlParameter("@TenCay", SqlDbType.NVarChar, 50) { Value = (object)tenCayTrong ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@TrangThai", SqlDbType.NVarChar, 50) { Value = (object)trangThai ?? DBNull.Value });
                        // Cho Decimal, cần chỉ định Precision và Scale nếu CSDL yêu cầu
                        cmd.Parameters.Add(new SqlParameter("@SLDK", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = (object)slDuKien ?? DBNull.Value }); // Ví dụ Decimal(10,2)
                        cmd.Parameters.Add(new SqlParameter("@SLTT", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = (object)slThucTe ?? DBNull.Value }); // Ví dụ Decimal(10,2)
                        cmd.Parameters.Add(new SqlParameter("@DonVi", SqlDbType.NVarChar, 20) { Value = (object)donViSL ?? DBNull.Value });

                        affectedRows = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Lỗi SQL khi thêm Mùa Vụ {idMuaVu}: {ex.Message} (Số lỗi: {ex.Number})");
                    // Kiểm tra lỗi trùng khóa chính (Primary Key Violation)
                    if (ex.Number == 2627 || ex.Number == 2601)
                    {
                        MessageBox.Show($"Lỗi: ID Mùa Vụ '{idMuaVu}' đã tồn tại.", "Lỗi Trùng ID", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show($"Lỗi khi thêm mùa vụ: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi không xác định khi thêm Mùa Vụ {idMuaVu}: {ex.Message}");
                    MessageBox.Show($"Lỗi hệ thống khi thêm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            return affectedRows > 0;
        }


        /// <summary>
        /// Cập nhật thông tin một Mùa Vụ hiện có.
        /// </summary>
        /// <param name="idMuaVu">ID Mùa Vụ cần cập nhật.</param>
        /// <param name="tenMuaVu">Tên Mùa Vụ mới.</param>
        /// <param name="ngayGieo">Thời gian gieo mới (nullable).</param>
        /// <param name="ngayDuKien">Thời gian thu hoạch dự kiến mới (nullable).</param>
        /// <param name="ngayThuHoach">Thời gian thu hoạch thực tế mới (nullable).</param>
        /// <param name="tenCayTrong">Tên cây trồng mới.</param>
        /// <param name="trangThai">Trạng thái mới.</param>
        /// <param name="slDuKien">Sản lượng dự kiến mới (nullable).</param>
        /// <param name="slThucTe">Sản lượng thực tế mới (nullable).</param>
        /// <param name="donViSL">Đơn vị sản lượng mới.</param>
        /// <returns>True nếu cập nhật thành công, False nếu thất bại.</returns>
        public bool UpdateMuaVu(string idMuaVu, string tenMuaVu, DateTime? ngayGieo, DateTime? ngayDuKien,
                                DateTime? ngayThuHoach, string tenCayTrong, string trangThai,
                                decimal? slDuKien, decimal? slThucTe, string donViSL)
        {
            // **QUAN TRỌNG:** Đảm bảo tên cột khớp CSDL. Không cập nhật R_IDRuong ở đây.
            string query = @"UPDATE QLMuaVu SET
                               MV_TenMuaVu = @TenMV,
                               MV_ThoiGianGieo = @NgayGieo,
                               MV_ThoiGianThuHoachDuKien = @NgayDuKien,
                               MV_ThoiGianThuHoachThucTe = @NgayTH,
                               MV_TenCayTrong = @TenCay,
                               MV_TrangThai = @TrangThai,
                               MV_SanLuongDuKien = @SLDK,
                               MV_SanLuongThucTe = @SLTT,
                               MV_DonViSanLuong = @DonVi
                             WHERE MV_IDMuaVu = @IDMV";
            int affectedRows = 0;

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Thêm Parameters - Đảm bảo kiểu dữ liệu và kích thước khớp CSDL
                        cmd.Parameters.Add(new SqlParameter("@TenMV", SqlDbType.NVarChar, 100) { Value = (object)tenMuaVu ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@NgayGieo", SqlDbType.Date) { Value = (object)ngayGieo ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@NgayDuKien", SqlDbType.Date) { Value = (object)ngayDuKien ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@NgayTH", SqlDbType.Date) { Value = (object)ngayThuHoach ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@TenCay", SqlDbType.NVarChar, 50) { Value = (object)tenCayTrong ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@TrangThai", SqlDbType.NVarChar, 50) { Value = (object)trangThai ?? DBNull.Value });
                        cmd.Parameters.Add(new SqlParameter("@SLDK", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = (object)slDuKien ?? DBNull.Value }); // Ví dụ Decimal(10,2)
                        cmd.Parameters.Add(new SqlParameter("@SLTT", SqlDbType.Decimal) { Precision = 10, Scale = 2, Value = (object)slThucTe ?? DBNull.Value }); // Ví dụ Decimal(10,2)
                        cmd.Parameters.Add(new SqlParameter("@DonVi", SqlDbType.NVarChar, 20) { Value = (object)donViSL ?? DBNull.Value });
                        // Tham số ID cho mệnh đề WHERE
                        cmd.Parameters.Add(new SqlParameter("@IDMV", SqlDbType.Char, 4) { Value = idMuaVu }); // Ví dụ CHAR(4)

                        affectedRows = cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex)
                {
                    Console.WriteLine($"Lỗi SQL khi cập nhật Mùa Vụ {idMuaVu}: {ex.Message} (Số lỗi: {ex.Number})");
                    MessageBox.Show($"Lỗi khi cập nhật mùa vụ: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi không xác định khi cập nhật Mùa Vụ {idMuaVu}: {ex.Message}");
                    MessageBox.Show($"Lỗi hệ thống khi cập nhật: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }
            // Kiểm tra xem có dòng nào thực sự được cập nhật không (có thể ID không tồn tại)
            if (affectedRows == 0)
            {
                Console.WriteLine($"Cập nhật Mùa Vụ {idMuaVu} không thành công (có thể ID không tồn tại).");
                // Không cần báo lỗi nếu ID không tìm thấy là hợp lệ
            }
            return affectedRows > 0;
        }

        /// <summary>
        /// Kiểm tra xem một ID Mùa Vụ đã tồn tại trong bảng QLMuaVu hay chưa.
        /// </summary>
        /// <param name="idMuaVu">ID Mùa Vụ cần kiểm tra.</param>
        /// <returns>True nếu ID đã tồn tại, False nếu chưa tồn tại.</returns>
        public bool CheckMuaVuExists(string idMuaVu)
        {
            int count = 0;
            string query = "SELECT COUNT(*) FROM QLMuaVu WHERE MV_IDMuaVu = @IDMV";

            using (SqlConnection sqlConnection = Connection.GetSqlConnection())
            {
                try
                {
                    sqlConnection.Open();
                    using (SqlCommand cmd = new SqlCommand(query, sqlConnection))
                    {
                        // Đảm bảo kiểu dữ liệu và kích thước khớp với CSDL (ví dụ: CHAR(4))
                        cmd.Parameters.Add(new SqlParameter("@IDMV", SqlDbType.Char, 4) { Value = idMuaVu });

                        object result = cmd.ExecuteScalar(); // ExecuteScalar hiệu quả hơn cho việc lấy một giá trị đơn lẻ

                        if (result != null && result != DBNull.Value)
                        {
                            count = Convert.ToInt32(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi kiểm tra ID Mùa Vụ tồn tại ({idMuaVu}): {ex.Message}");
                    MessageBox.Show($"Lỗi khi kiểm tra sự tồn tại của ID Mùa vụ: {ex.Message}", "Lỗi CSDL", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    // Quan trọng: Trả về true khi có lỗi để ngăn việc cố gắng tạo bản ghi trùng lặp
                    // nếu việc kiểm tra không thành công.
                    return true;
                }
            }
            // Nếu count > 0 tức là ID đã tồn tại
            return count > 0;
        }
    }
}