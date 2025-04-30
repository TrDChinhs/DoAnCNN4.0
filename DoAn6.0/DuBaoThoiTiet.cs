using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Windows.Forms.DataVisualization.Charting;
using System.Globalization;
using MetroFramework;
using MetroFramework.Forms;

namespace DoAn6._0
{
    public partial class DuBaoThoiTiet : MetroForm 
    {
        private readonly Color formBackColor = Color.FromArgb(16, 18, 24);
        private readonly Font dataLabelFont = new Font("Microsoft Sans Serif", 6.5f);
        private DateTime _currentlySelectedDate = DateTime.MinValue; 
        private Dictionary<string, Image> weatherIcons;

        // --- Constructor ---
        public DuBaoThoiTiet()
        {
            InitializeComponent();
            ConfigureStyleManager();
            InitializeIconDictionary();
            SetupInitialUI();
        }

        public class SelectedDayDetails
        {
            public int? MinTemp { get; set; }
            public int? MaxTemp { get; set; }
            public double? AvgHumidity { get; set; }
            public int? MaxRainChance { get; set; }
            public double? AvgWindSpeed { get; set; }
            public string RepTrangThai { get; set; }
            public string RepIconKey { get; set; }
        }

        public class DailyForecastSummary
        {
            public DateTime Date { get; set; }
            public string DayName { get; set; }
            public string IconKey { get; set; }
            public int? MaxTemp { get; set; }
            public int? MinTemp { get; set; }
        }

        private void ConfigureStyleManager()
        {
            if (this.components == null) { this.components = new System.ComponentModel.Container(); }
            var styleManager = this.components.Components.OfType<MetroFramework.Components.MetroStyleManager>().FirstOrDefault();
            if (styleManager != null) { styleManager.Theme = MetroFramework.MetroThemeStyle.Dark; this.StyleManager = styleManager; Console.WriteLine("DEBUG: Đã áp dụng Metro Style Manager."); }
            else { this.Theme = MetroThemeStyle.Dark; Console.WriteLine("WARNING: MetroStyleManager không tìm thấy."); }
        }

        private void InitializeIconDictionary()
        {
            this.weatherIcons = new Dictionary<string, Image>(StringComparer.OrdinalIgnoreCase);
            try
            {
                if (Properties.Resources.TroiNang != null) this.weatherIcons.Add("nang", Properties.Resources.TroiNang); else Console.WriteLine("WARNING: Resource 'TroiNang' không tồn tại.");
                if (Properties.Resources.TroiAmU != null) this.weatherIcons.Add("may", Properties.Resources.TroiAmU); else Console.WriteLine("WARNING: Resource 'TroiAmU' không tồn tại.");
                if (Properties.Resources.TroiMua != null) { this.weatherIcons.Add("mua", Properties.Resources.TroiMua); this.weatherIcons.Add("dong", Properties.Resources.TroiMua); } else { Console.WriteLine("WARNING: Resource 'TroiMua' không tồn tại."); }

                string defaultIconResourceName = "default_icon";
                try
                {
                    Image defaultIconResource = Properties.Resources.ResourceManager.GetObject(defaultIconResourceName) as Image;
                    if (defaultIconResource != null) { this.weatherIcons.Add("default", defaultIconResource); Console.WriteLine($"DEBUG: Đã thêm icon mặc định '{defaultIconResourceName}'."); }
                    else { Console.WriteLine($"WARNING: Không tìm thấy resource ảnh mặc định '{defaultIconResourceName}'."); }
                }
                catch (Exception resEx) { Console.WriteLine($"WARNING: Lỗi khi truy cập Resource '{defaultIconResourceName}': {resEx.Message}"); }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi khởi tạo Icon Dictionary: {ex.Message}", "Lỗi Resource", MessageBoxButtons.OK, MessageBoxIcon.Error);
                if (this.weatherIcons == null) this.weatherIcons = new Dictionary<string, Image>();
            }
        }

        // --- Thiết lập UI ban đầu ---
        private void SetupInitialUI()
        {
            // Chart Nhiệt độ
            if (chartTemperature != null)
            {
                chartTemperature.BackColor = formBackColor; /*...*/ chartTemperature.ChartAreas.Clear(); /*...*/ chartTemperature.Series.Clear(); /*...*/ chartTemperature.Legends.Clear();
                ChartArea tempArea = new ChartArea(); 
                ConfigureChartAreaBase(tempArea, "TempArea");
                chartTemperature.ChartAreas.Add(tempArea);
            }

            // THÊM: Chart Xác suất Mưa
            if (chartPrecipitation != null)
            {
                chartPrecipitation.BackColor = formBackColor;
                chartPrecipitation.ChartAreas.Clear(); chartPrecipitation.Series.Clear(); chartPrecipitation.Legends.Clear();
                ChartArea precipArea = new ChartArea();
                ConfigureChartAreaBase(precipArea, "PrecipArea"); 
                chartPrecipitation.ChartAreas.Add(precipArea);
            }

            // THÊM: Chart Gió
            if (chartWind != null)
            {
                chartWind.BackColor = formBackColor;
                chartWind.ChartAreas.Clear(); chartWind.Series.Clear(); chartWind.Legends.Clear();
                ChartArea windArea = new ChartArea();
                ConfigureChartAreaBase(windArea, "WindArea"); 
                chartWind.ChartAreas.Add(windArea);
            }

            ClearLabels();
        }

        // --- Xóa Labels ---
        private void ClearLabels()
        {
            if (lblTodayName != null) lblTodayName.Text = "";
            if (lblDescription != null) lblDescription.Text = "Chọn một ngày";
            if (lblMaxTempValue != null) lblMaxTempValue.Text = "--";
            if (lblHumidityValue != null) lblHumidityValue.Text = "--%";
            if (lblRainChanceValue != null) lblRainChanceValue.Text = "--%";
            if (labelwind != null) labelwind.Text = "-- km/h";
            if (picMainWeatherIcon != null) picMainWeatherIcon.Image = null;
        }

        // --- Cấu hình Chart Area ---
        private void ConfigureTemperatureChartArea(ChartArea chartArea)
        {
            if (chartArea == null) return;
            chartArea.BackColor = Color.Transparent; chartArea.BorderWidth = 0; chartArea.BorderDashStyle = ChartDashStyle.NotSet;
            var axisY = chartArea.AxisY; axisY.Enabled = AxisEnabled.False; axisY.MajorGrid.Enabled = false; axisY.IsStartedFromZero = false;
            var axisX = chartArea.AxisX; axisX.Enabled = AxisEnabled.True; axisX.Title = ""; axisX.LineWidth = 0; axisX.MajorTickMark.Enabled = false; axisX.MinorTickMark.Enabled = false; axisX.LabelStyle.Format = "HH:mm"; axisX.LabelStyle.Enabled = true; axisX.LabelStyle.ForeColor = Color.Gray; axisX.IntervalType = DateTimeIntervalType.Hours; axisX.Interval = 3; axisX.IntervalOffset = 3; axisX.IntervalOffsetType = DateTimeIntervalType.Hours; axisX.MajorGrid.Enabled = false;
            chartArea.Position = new ElementPosition(2, 5, 96, 90);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) { dataLabelFont?.Dispose(); components?.Dispose(); }
            base.Dispose(disposing);
        }

        private void DuBaoThoiTiet_Load(object sender, EventArgs e)
        {
            List<DailyForecastSummary> dailySummaries = GetDailyForecastSummaries();
            Console.WriteLine($"Số ngày dự báo tóm tắt lấy được: {dailySummaries.Count}");
            PopulateDaySelectorsRich(dailySummaries);

            DateTime initialDateToShow = DateTime.MinValue;
            if (dailySummaries.Any())
            {
                initialDateToShow = dailySummaries[0].Date;
                                                        
                if (flowLayoutPanelDays.Controls.Count > 0) { /* ... highlight control đầu tiên ... */ }
                _currentlySelectedDate = initialDateToShow;
            }
            else { initialDateToShow = DateTime.Today; /* ... */ }

            Console.WriteLine($"Hiển thị ban đầu cho ngày: {initialDateToShow.ToShortDateString()}");
            LoadChartForDay(initialDateToShow);
            UpdateLabelsForSelectedDay(initialDateToShow); 
        }

        private List<DailyForecastSummary> GetDailyForecastSummaries()
        {
            List<DailyForecastSummary> dailySummaries = new List<DailyForecastSummary>();
            using (SqlConnection connection = Connection.GetSqlConnection())
            {
                try
                {
                    // Truy vấn lấy Min, Max Temp và IconKey đại diện
                    string query = @"SELECT CAST(NgayGio AS DATE) AS ForecastDate, MIN(NhietDo) AS MinTemp, MAX(NhietDo) AS MaxTemp, (SELECT TOP 1 T2.IconKey FROM DuBaoThoiTiet T2 WHERE CAST(T2.NgayGio AS DATE) = CAST(T1.NgayGio AS DATE) ORDER BY ABS(DATEDIFF(hour, T2.NgayGio, DATEADD(hour, 12, CAST(CAST(T2.NgayGio AS DATE) AS DATETIME2(0))))) ASC) AS RepresentativeIconKey FROM DuBaoThoiTiet T1 GROUP BY CAST(NgayGio AS DATE) ORDER BY ForecastDate";
                    SqlCommand command = new SqlCommand(query, connection);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    CultureInfo viVn = new CultureInfo("vi-VN");
                    while (reader.Read())
                    {
                        DateTime forecastDate = Convert.ToDateTime(reader["ForecastDate"]);
                        string dayName = forecastDate.ToString("ddd", viVn);
                        if (forecastDate.Date == DateTime.Today) dayName = "H.Nay";
                        if (forecastDate.Date == DateTime.Today.AddDays(-1)) dayName = "H.Qua";
                        if (forecastDate.Date == DateTime.Today.AddDays(1)) dayName = "N.Mai";
                        dailySummaries.Add(new DailyForecastSummary { Date = forecastDate, DayName = dayName, MinTemp = reader["MinTemp"] != DBNull.Value ? Convert.ToInt32(reader["MinTemp"]) : (int?)null, MaxTemp = reader["MaxTemp"] != DBNull.Value ? Convert.ToInt32(reader["MaxTemp"]) : (int?)null, IconKey = reader["RepresentativeIconKey"] != DBNull.Value ? reader["RepresentativeIconKey"].ToString().ToLower() : "default" });
                    }
                    reader.Close();
                }
                catch (Exception ex) { MessageBox.Show($"Lỗi khi tải dữ liệu tóm tắt hàng ngày: {ex.Message}", "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            }
            return dailySummaries;
        }


        // --- Tạo UI chọn ngày phong phú (Hiển thị Icon, Temp) ---
        // Thay thế hàm PopulateDaySelectorsSimple bằng hàm này
        private void PopulateDaySelectorsRich(List<DailyForecastSummary> summaries)
        {
            flowLayoutPanelDays.Controls.Clear();
            Console.WriteLine($"DEBUG: Bắt đầu PopulateDaySelectorsRich với {summaries.Count} ngày.");
            int panelCount = 0;

            foreach (var summary in summaries) // Lặp qua danh sách summary
            {
                try
                {
                    // Tạo Panel chứa
                    Panel dayPanel = new Panel
                    {
                        Size = new Size(95, 130),
                        Margin = new Padding(4),
                        BackColor = Color.FromArgb(40, 40, 40),
                        Cursor = Cursors.Hand,
                        Tag = summary.Date 
                    };

                    // Tạo các control con bên trong Panel
                    Label lblDay = new Label { Text = summary.DayName, ForeColor = Color.White, Font = new Font("Segoe UI", 9f, FontStyle.Bold), Dock = DockStyle.Top, TextAlign = ContentAlignment.MiddleCenter, Height = 25 };
                    // Lấy Max/Min Temp từ summary
                    Label lblTemp = new Label { Text = $"{(summary.MaxTemp?.ToString() ?? "--")}°/{(summary.MinTemp?.ToString() ?? "--")}°", ForeColor = Color.LightGray, Font = new Font("Segoe UI", 8f), Dock = DockStyle.Bottom, TextAlign = ContentAlignment.MiddleCenter, Height = 20 };
                    PictureBox picIcon = new PictureBox { SizeMode = PictureBoxSizeMode.Zoom, Dock = DockStyle.Fill, BackColor = Color.Transparent };

                    // Gán ảnh từ Dictionary dùng summary.IconKey
                    string iconKey = (!string.IsNullOrEmpty(summary.IconKey)) ? summary.IconKey : "default";
                    if (this.weatherIcons != null && this.weatherIcons.ContainsKey(iconKey)) { picIcon.Image = this.weatherIcons[iconKey]; }
                    else { if (this.weatherIcons != null && this.weatherIcons.ContainsKey("default")) { picIcon.Image = this.weatherIcons["default"]; } else { picIcon.Image = null; } Console.WriteLine($"WARNING: Không tìm thấy icon cho key '{iconKey}'."); }

                    // Thêm control con vào Panel
                    dayPanel.Controls.Add(lblTemp); dayPanel.Controls.Add(lblDay); dayPanel.Controls.Add(picIcon);

                    // Gán sự kiện Click (vẫn dùng DayControl_Click cũ)
                    dayPanel.Click += DayControl_Click; lblDay.Click += DayControl_Click; lblTemp.Click += DayControl_Click; picIcon.Click += DayControl_Click;

                    // Thêm Panel vào FlowLayoutPanel
                    flowLayoutPanelDays.Controls.Add(dayPanel);
                    panelCount++;
                }
                catch (Exception exPanel) { Console.WriteLine($"Lỗi khi tạo panel cho ngày {summary.Date.ToShortDateString()}: {exPanel.Message}"); }
            }
            Console.WriteLine($"DEBUG: Kết thúc PopulateDaySelectorsRich. Đã thêm {panelCount} panel.");
        }

        // --- Xử lý Click vào ngày ---
        private void DayControl_Click(object sender, EventArgs e)
        {
            Control clickedControl = sender as Control;
            Panel selectedPanel = null;
            DateTime clickedDate = DateTime.MinValue;

            // Xác định Panel và Date được click
            if (clickedControl is Panel) { selectedPanel = (Panel)clickedControl; }
            else if (clickedControl != null && clickedControl.Parent is Panel) { selectedPanel = (Panel)clickedControl.Parent; }

            if (selectedPanel != null && selectedPanel.Tag is DateTime)
            {
                clickedDate = (DateTime)selectedPanel.Tag;
            }
            else if (clickedControl is Label && clickedControl.Tag is DateTime)
            { 
                clickedDate = (DateTime)clickedControl.Tag;
                selectedPanel = clickedControl.Parent as Panel; // Cố gắng lấy panel để highlight
            }
            else
            {
                return; // Không xác định được ngày
            }


            if (_currentlySelectedDate.Date != clickedDate.Date)
            {
                _currentlySelectedDate = clickedDate;

                // Bỏ highlight control cũ, highlight control mới
                foreach (Control ctl in flowLayoutPanelDays.Controls) { ctl.BackColor = Color.FromArgb(40, 40, 40); }
                if (selectedPanel != null) { selectedPanel.BackColor = Color.FromArgb(0, 122, 204); }
                else { clickedControl.BackColor = Color.FromArgb(0, 122, 204); } // Highlight Label nếu không có panel

                // Tải lại biểu đồ và label chi tiết
                LoadChartForDay(clickedDate);
                UpdateLabelsForSelectedDay(clickedDate); // Hàm này vẫn truy vấn DB
            }
        }

        // --- Truy vấn DB và cập nhật Labels chính cho ngày được chọn ---
        private void UpdateLabelsForSelectedDay(DateTime selectedDate)
        {
            SelectedDayDetails details = null;
            using (SqlConnection connection = Connection.GetSqlConnection())
            {
                // Câu truy vấn SQL tổng hợp (đã sửa)
                string query = @"WITH RankedData AS (SELECT *, ROW_NUMBER() OVER(ORDER BY ABS(DATEDIFF(hour, NgayGio, DATEADD(hour, 12, CAST(CAST(NgayGio AS DATE) AS DATETIME2(0))) ))) as rn_noon FROM DuBaoThoiTiet WHERE CAST(NgayGio AS DATE) = @SelectedDate) SELECT MIN(T1.NhietDo) AS MinTemp, MAX(T1.NhietDo) AS MaxTemp, AVG(CAST(T1.DoAm AS FLOAT)) AS AvgHumidity, MAX(T1.XacSuatMua) AS MaxRainChance, AVG(T1.TocDoGio) AS AvgWindSpeed, (SELECT TOP 1 R.TrangThai FROM RankedData R WHERE R.rn_noon = 1) AS RepTrangThai, (SELECT TOP 1 R.IconKey FROM RankedData R WHERE R.rn_noon = 1) AS RepIconKey FROM DuBaoThoiTiet T1 WHERE CAST(NgayGio AS DATE) = @SelectedDate;";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@SelectedDate", selectedDate.Date);
                try
                {
                    connection.Open(); SqlDataReader reader = command.ExecuteReader();
                    if (reader.Read()) { details = new SelectedDayDetails { MinTemp = reader["MinTemp"] != DBNull.Value ? Convert.ToInt32(reader["MinTemp"]) : (int?)null, MaxTemp = reader["MaxTemp"] != DBNull.Value ? Convert.ToInt32(reader["MaxTemp"]) : (int?)null, AvgHumidity = reader["AvgHumidity"] != DBNull.Value ? Convert.ToDouble(reader["AvgHumidity"]) : (double?)null, MaxRainChance = reader["MaxRainChance"] != DBNull.Value ? Convert.ToInt32(reader["MaxRainChance"]) : (int?)null, AvgWindSpeed = reader["AvgWindSpeed"] != DBNull.Value ? Convert.ToDouble(reader["AvgWindSpeed"]) : (double?)null, RepTrangThai = reader["RepTrangThai"] != DBNull.Value ? reader["RepTrangThai"].ToString() : null, RepIconKey = reader["RepIconKey"] != DBNull.Value ? reader["RepIconKey"].ToString().ToLower() : "default" }; }
                    reader.Close();
                }
                catch (Exception ex) { MessageBox.Show($"Lỗi SQL khi lấy chi tiết ngày {selectedDate.ToShortDateString()}: {ex.Message}", "Lỗi SQL", MessageBoxButtons.OK, MessageBoxIcon.Error); ClearLabels(); return; }
            }

            if (details == null) { ClearLabels(); return; }

            CultureInfo viVn = new CultureInfo("vi-VN");
            if (lblTodayName != null) { lblTodayName.Text = $"{selectedDate.ToString("dddd", viVn)}\n{selectedDate.ToString("dd/MM/yyyy")}"; }

            // Sửa: Dùng if-else if thay cho switch expression và ưu tiên RepTrangThai
            if (lblDescription != null)
            {
                string descText; string key = details.RepIconKey ?? "default";
                if (!string.IsNullOrEmpty(details.RepTrangThai) && details.RepTrangThai != "N/A") { descText = details.RepTrangThai; }
                else { if (key == "nang") descText = "Trời nắng"; else if (key == "may") descText = "Nhiều mây"; else if (key == "mua") descText = "Có mưa"; else if (key == "dong") descText = "Có dông"; else descText = "Không xác định"; }
                lblDescription.Text = descText;
            }

            if (lblMaxTempValue != null) { lblMaxTempValue.Text = details.MaxTemp.HasValue ? details.MaxTemp.Value.ToString() : "--"; }
            if (lblHumidityValue != null) { lblHumidityValue.Text = details.AvgHumidity.HasValue ? details.AvgHumidity.Value.ToString("0") + "%" : "--%"; }
            if (lblRainChanceValue != null) { lblRainChanceValue.Text = details.MaxRainChance.HasValue ? details.MaxRainChance.Value.ToString() + "%" : "--%"; }
            // !! Đảm bảo tên labelwind đúng !!
            if (labelwind != null) { labelwind.Text = details.AvgWindSpeed.HasValue ? details.AvgWindSpeed.Value.ToString("0.0") + " km/h" : "-- km/h"; }

            // Cập nhật Icon chính
            if (picMainWeatherIcon != null && this.weatherIcons != null)
            {
                string iconKey = details.RepIconKey ?? "default";
                if (this.weatherIcons.ContainsKey(iconKey)) { picMainWeatherIcon.Image = this.weatherIcons[iconKey]; }
                else if (this.weatherIcons.ContainsKey("default")) { picMainWeatherIcon.Image = this.weatherIcons["default"]; }
                else { picMainWeatherIcon.Image = null; }
            }
        }


        // --- Load Chart For Day ---
        private void LoadChartForDay(DateTime selectedDate)
        {
            List<WeatherData> chartData = GetChartData(selectedDate);

            if (chartData == null || !chartData.Any())
            {
                Console.WriteLine($"WARNING: Không có dữ liệu nào từ SQL cho ngày {selectedDate.ToShortDateString()}. Xóa các biểu đồ.");
                ClearChart(chartTemperature);
                ClearChart(chartPrecipitation); // Giữ nguyên tên control chartPrecipitation
                ClearChart(chartWind);
                return;
            }

            // Vẽ biểu đồ Nhiệt độ (giữ nguyên)
            var tempData = chartData.Where(d => d.NhietDo.HasValue).ToList();
            if (tempData.Any()) { PopulateTemperatureChart(tempData); }
            else { ClearChart(chartTemperature); }

            var rainfallData = chartData.Where(d => d.LuongMua.HasValue).ToList();
            if (rainfallData.Any())
            {
                PopulateRainfallChart(rainfallData); // *** Gọi hàm vẽ Lượng Mưa với dữ liệu đúng ***
            }
            else
            {
                ClearChart(chartPrecipitation); // Xóa biểu đồ Lượng mưa nếu không có dữ liệu
            }

            // Vẽ biểu đồ Gió (giữ nguyên)
            var windData = chartData.Where(d => d.TocDoGio.HasValue).ToList();
            if (windData.Any()) { PopulateWindChart(windData); }
            else { ClearChart(chartWind); }
        }

        // Hàm xóa dữ liệu cho một control Chart cụ thể
        private void ClearChart(Chart chartToClear)
        {
            if (chartToClear != null && chartToClear.IsHandleCreated)
            {
                while (chartToClear.Series.Count > 0) { chartToClear.Series.RemoveAt(0); }
                chartToClear.Annotations.Clear();
                if (chartToClear.ChartAreas.Count > 0)
                {
                    var chartArea = chartToClear.ChartAreas[0]; chartArea.AxisX.CustomLabels.Clear();
                    chartArea.AxisY.Minimum = Double.NaN; chartArea.AxisY.Maximum = Double.NaN;
                    chartArea.AxisX.Minimum = Double.NaN; chartArea.AxisX.Maximum = Double.NaN;
                    chartArea.AxisX.ScaleView.ZoomReset();
                }
                chartToClear.Invalidate();
            }
        }

        private void ConfigureChartAreaBase(ChartArea chartArea, string areaName)
        {
            if (chartArea == null) return;
            chartArea.Name = areaName; // Đặt tên cho Area
            chartArea.BackColor = Color.Transparent;
            chartArea.BorderWidth = 0;
            chartArea.BorderDashStyle = ChartDashStyle.NotSet;

            // Trục Y ẩn
            var axisY = chartArea.AxisY;
            axisY.Enabled = AxisEnabled.False;
            axisY.MajorGrid.Enabled = false;
            axisY.IsStartedFromZero = false; // Cho phép trục Y không bắt đầu từ 0

            // Trục X thời gian (giống biểu đồ nhiệt độ)
            var axisX = chartArea.AxisX;
            axisX.Enabled = AxisEnabled.True; axisX.Title = ""; axisX.LineWidth = 0;
            axisX.MajorTickMark.Enabled = false; axisX.MinorTickMark.Enabled = false;
            axisX.LabelStyle.Format = "HH:mm"; axisX.LabelStyle.Enabled = true;
            axisX.LabelStyle.ForeColor = Color.Gray; axisX.IntervalType = DateTimeIntervalType.Hours;
            axisX.Interval = 3; axisX.IntervalOffset = 3; axisX.IntervalOffsetType = DateTimeIntervalType.Hours;
            axisX.MajorGrid.Enabled = false;

            // Vị trí tương đối
            chartArea.Position = new ElementPosition(2, 5, 96, 90);
        }

        // --- Hàm vẽ biểu đồ Xác suất Mưa ---
        // File: DuBaoThoiTiet.cs

        private void PopulateRainfallChart(List<WeatherData> data) // Đảm bảo tên hàm đúng
        {
            Chart targetChart = chartPrecipitation; // Chart để vẽ lượng mưa
            if (targetChart == null || !targetChart.IsHandleCreated || targetChart.ChartAreas.Count == 0)
            {
                Console.WriteLine("WARNING: chartPrecipitation chưa sẵn sàng in PopulateRainfallChart.");
                return;
            }

            ClearChartAndSeries(targetChart); // Xóa trước khi vẽ

            var chartArea = targetChart.ChartAreas[0];
            string seriesName = "LuongMuaSeries";

            // Lọc dữ liệu hợp lệ
            var validData = data.Where(item => item.LuongMua.HasValue && item.LuongMua.Value >= 0).ToList();

            if (!validData.Any())
            {
                Console.WriteLine($"INFO PopulateRainfallChart: No valid rainfall data (>=0) to plot.");
                // Reset trục Y về mặc định nếu không có dữ liệu
                try
                {
                    chartArea.AxisY.Minimum = Double.NaN;
                    chartArea.AxisY.Maximum = Double.NaN;
                    chartArea.AxisY.IsStartedFromZero = false; // Reset
                }
                catch (Exception exAxisReset) { Console.WriteLine("Error resetting Y Axis: " + exAxisReset.Message); }
                return; // Thoát khỏi hàm
            }

            // --- Tạo và cấu hình Series (giữ nguyên code cũ) ---
            var series = new Series(seriesName)
            {
                ChartType = SeriesChartType.Column,
                Color = Color.FromArgb(180, 30, 144, 255),
                BorderColor = Color.FromArgb(30, 144, 255),
                XValueType = ChartValueType.DateTime,
                YValueType = ChartValueType.Double,
                IsValueShownAsLabel = true,
                LabelForeColor = Color.White,
                Font = dataLabelFont,
                LabelFormat = "0.0 'mm'",
                SmartLabelStyle = new SmartLabelStyle() { /* ... */ }
            };
            series["PointWidth"] = "0.6";

            try
            {
                series.Points.DataBindXY(validData.Select(d => d.NgayGio).ToList(),
                                         validData.Select(d => d.LuongMua.Value).ToList());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi DataBindXY cho biểu đồ Lượng Mưa: {ex.Message}", "Lỗi Binding", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            targetChart.Series.Add(series);

            // --- Logic đặt Min/Max cho Trục X (giữ nguyên code cũ) ---
            try
            {
                // ... (Code đặt AxisX.Minimum, AxisX.Maximum như cũ) ...
                DateTime minDate = validData.Min(d => d.NgayGio);
                DateTime maxDate = validData.Max(d => d.NgayGio);
                if (minDate <= DateTime.MinValue.AddYears(1900) || maxDate <= DateTime.MinValue.AddYears(1900) || minDate > maxDate.AddSeconds(1))
                { chartArea.AxisX.Minimum = Double.NaN; chartArea.AxisX.Maximum = Double.NaN; }
                else if (minDate == maxDate)
                { chartArea.AxisX.Minimum = minDate.AddHours(-1).ToOADate(); chartArea.AxisX.Maximum = maxDate.AddHours(1).ToOADate(); }
                else
                { TimeSpan padding = TimeSpan.FromMinutes(30); chartArea.AxisX.Minimum = minDate.Subtract(padding).ToOADate(); chartArea.AxisX.Maximum = maxDate.Add(padding).ToOADate(); }
                chartArea.AxisX.Interval = 3; // Đảm bảo interval là 3 giờ
                chartArea.AxisX.IntervalType = DateTimeIntervalType.Hours;
                chartArea.AxisX.IntervalOffset = 0;
                chartArea.AxisX.IntervalOffsetType = DateTimeIntervalType.Hours;
                chartArea.AxisX.LabelStyle.Format = "HH:mm";
                chartArea.AxisX.ScrollBar.Enabled = false;
            }
            catch (Exception) { /* ... Log lỗi và reset trục X ... */ chartArea.AxisX.Minimum = Double.NaN; chartArea.AxisX.Maximum = Double.NaN; }


            // *** THÊM VÀO ĐÂY: Cài đặt cố định cho Trục Y (Lượng mưa) ***
            try
            {
                // Đặt giá trị tối đa CỐ ĐỊNH cho trục Y.
                // Chọn giá trị phù hợp với lượng mưa tối đa bạn muốn hiển thị (ví dụ: 10mm, 20mm, 50mm?)
                double maxRainfallToShow = 8.5; // <-- *** THAY ĐỔI GIÁ TRỊ NÀY NẾU CẦN ***

                chartArea.AxisY.Maximum = maxRainfallToShow;
                chartArea.AxisY.Minimum = 0; // Luôn bắt đầu từ 0 cho lượng mưa
                chartArea.AxisY.IsStartedFromZero = true; // Đảm bảo bắt đầu từ 0

                // Vẫn giữ trục Y ẩn nếu bạn muốn (như trong ConfigureChartAreaBase)
                chartArea.AxisY.Enabled = AxisEnabled.False;
                // chartArea.AxisY.LabelStyle.Enabled = false; // Không cần nếu Enabled = False
                // chartArea.AxisY.MajorGrid.Enabled = false; // Không cần nếu Enabled = False
                // chartArea.AxisY.MajorTickMark.Enabled = false; // Không cần nếu Enabled = False
            }
            catch (Exception exY)
            {
                Console.WriteLine($"Lỗi khi đặt Min/Max Y ({seriesName}): {exY.Message}");
                chartArea.AxisY.Minimum = Double.NaN; // Reset nếu lỗi
                chartArea.AxisY.Maximum = Double.NaN;
                chartArea.AxisY.IsStartedFromZero = false;
            }

            // --- Code ẩn nhãn dữ liệu mỗi 3 giờ (giữ nguyên code cũ) ---
            try
            {
                if (targetChart.Series.Count > 0 && targetChart.Series[seriesName].Points.Count > 0)
                {
                    foreach (DataPoint point in targetChart.Series[seriesName].Points)
                    {
                        DateTime pointTime = DateTime.FromOADate(point.XValue);
                        if (pointTime.Hour % 3 != 0)
                        {
                            point.Label = "";
                        }
                    }
                }
            }
            catch (Exception) { /* ... Log lỗi ... */ }

        } // Kết thúc hàm PopulateRainfallChart


        // --- Hàm vẽ biểu đồ Gió ---
        private void PopulateWindChart(List<WeatherData> data)
        {
            if (chartWind == null || !chartWind.IsHandleCreated || chartWind.ChartAreas.Count == 0) return;

            var chartArea = chartWind.ChartAreas[0]; 
            string seriesName = "WindSeries";

            if (chartWind.Series.IndexOf(seriesName) != -1) { chartWind.Series.Remove(chartWind.Series[seriesName]); }

            // Lọc dữ liệu có Tốc độ gió
            var validData = data.Where(item => item.TocDoGio.HasValue).ToList();
            if (!validData.Any()) { chartWind.Series.Clear(); return; }

            var series = new Series(seriesName)
            {
                ChartType = SeriesChartType.Line, // Kiểu đường (hoặc Area)
                BorderWidth = 2,
                Color = Color.FromArgb(150, 173, 216, 230), // Màu xanh nhạt (LightBlue với alpha)
                BorderColor = Color.FromArgb(255, 173, 216, 230),
                XValueType = ChartValueType.DateTime,
                YValueType = ChartValueType.Double, 
                IsValueShownAsLabel = true,
                LabelForeColor = Color.White,
                Font = dataLabelFont,
                LabelFormat = "0.# 'km/h'", 
                MarkerStyle = MarkerStyle.Circle,
                MarkerSize = 4,
                MarkerColor = Color.LightBlue,
                SmartLabelStyle = new SmartLabelStyle() { /* ... Cấu hình tương tự ... */ }
            };

            try
            {
                series.Points.DataBindXY(validData.Select(d => d.NgayGio).ToList(),
                                         validData.Select(d => d.TocDoGio.Value).ToList());
            }
            catch (Exception) { /* ... xử lý lỗi DataBindXY ... */ return; }

            chartWind.Series.Add(series);

            // Điều chỉnh View trục X (giống các biểu đồ khác)
            try
            {
                DateTime minDate = validData.Min(d => d.NgayGio); DateTime maxDate = validData.Max(d => d.NgayGio); TimeSpan offset = TimeSpan.FromHours(1);
                if (maxDate > minDate) { DateTime viewStart = minDate - offset; DateTime viewEnd = maxDate + offset; chartArea.AxisX.Minimum = viewStart.ToOADate(); chartArea.AxisX.Maximum = viewEnd.ToOADate(); }
                else { chartArea.AxisX.ScaleView.ZoomReset(); chartArea.AxisX.Minimum = Double.NaN; chartArea.AxisX.Maximum = Double.NaN; }
                chartArea.AxisX.ScrollBar.Enabled = false;
            }
            catch (Exception ex) { Console.WriteLine("Lỗi khi đặt ScaleView/Min/Max X (Wind): " + ex.Message); /*...*/ }
        }
        // Bên trong lớp DuBaoThoiTiet
        private List<WeatherData> GetChartData(DateTime selectedDate)
        {
            List<WeatherData> chartData = new List<WeatherData>(); // Đổi tên biến để rõ ràng hơn
            Console.WriteLine($"DEBUG: GetChartData được gọi cho ngày: {selectedDate.ToShortDateString()}");
            using (SqlConnection connection = Connection.GetSqlConnection())
            {
                try
                {
                    string query = @"
                    SELECT NgayGio, NhietDo, TrangThai, IconKey, LuongMua, DoAm, TocDoGio, HuongGio, XacSuatMua
                    FROM DuBaoThoiTiet
                    WHERE CAST(NgayGio AS DATE) = @SelectedDate
                    ORDER BY NgayGio";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@SelectedDate", selectedDate.Date);
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var dataPoint = new WeatherData
                        {
                            NgayGio = reader["NgayGio"] != DBNull.Value ? Convert.ToDateTime(reader["NgayGio"]) : DateTime.MinValue,
                            NhietDo = reader["NhietDo"] != DBNull.Value ? Convert.ToInt32(reader["NhietDo"]) : (int?)null,
                            LuongMua = reader["LuongMua"] != DBNull.Value ? Convert.ToDouble(reader["LuongMua"]) : (double?)null, // tinyint -> int?
                            TocDoGio = reader["TocDoGio"] != DBNull.Value ? Convert.ToDouble(reader["TocDoGio"]) : (double?)null // float -> double?
                        };
                        if (dataPoint.NgayGio != DateTime.MinValue) 
                        {
                            chartData.Add(dataPoint);
                        }
                    }
                    reader.Close();
                }
                catch (Exception) { /* ... xử lý lỗi ... */ }
            }
            Console.WriteLine($"DEBUG: GetChartData trả về {chartData.Count} điểm dữ liệu.");
            return chartData;
        }

        // --- Populate Chart ---
        private void PopulateTemperatureChart(List<WeatherData> data)
        {
            if (chartTemperature == null || !chartTemperature.IsHandleCreated || chartTemperature.ChartAreas.Count == 0) return;
            var chartArea = chartTemperature.ChartAreas[0]; string seriesName = "NhietDoSeries";
            if (chartTemperature.Series.IndexOf(seriesName) != -1) { chartTemperature.Series.Remove(chartTemperature.Series[seriesName]); }
            var validChartData = data.Where(item => item.NhietDo.HasValue && item.NgayGio != DateTime.MinValue).ToList();
            if (!validChartData.Any()) { ClearChart(); return; }

            var series = new Series(seriesName) { ChartType = SeriesChartType.Area, BorderWidth = 2, Color = Color.FromArgb(150, 255, 192, 0), BorderColor = Color.FromArgb(255, 255, 192, 0), XValueType = ChartValueType.DateTime, YValueType = ChartValueType.Int32, IsValueShownAsLabel = true, LabelForeColor = Color.White, Font = this.dataLabelFont, LabelFormat = "0°", SmartLabelStyle = new SmartLabelStyle() { Enabled = true, AllowOutsidePlotArea = LabelOutsidePlotAreaStyle.No, CalloutLineColor = Color.Transparent, MovingDirection = LabelAlignmentStyles.Top, IsOverlappedHidden = false, MinMovingDistance = 5 } };
            try { series.Points.DataBindXY(validChartData.Select(d => d.NgayGio).ToList(), validChartData.Select(d => d.NhietDo.Value).ToList()); }
            catch (Exception ex) { MessageBox.Show($"Lỗi khi thực hiện DataBindXY cho biểu đồ: {ex.Message}", "Lỗi Data Binding", MessageBoxButtons.OK, MessageBoxIcon.Error); ClearChart(); return; }
            chartTemperature.Series.Add(series);
            try { int minTemp = validChartData.Min(d => d.NhietDo.Value); int maxTemp = validChartData.Max(d => d.NhietDo.Value); chartArea.AxisY.Minimum = minTemp - 2; chartArea.AxisY.Maximum = maxTemp + 3; } catch { /* Ignore */ }
            try { DateTime minDate = validChartData.Min(d => d.NgayGio); DateTime maxDate = validChartData.Max(d => d.NgayGio); TimeSpan offset = TimeSpan.FromHours(1); if (maxDate > minDate) { DateTime viewStart = minDate - offset; DateTime viewEnd = maxDate + offset; chartArea.AxisX.Minimum = viewStart.ToOADate(); chartArea.AxisX.Maximum = viewEnd.ToOADate(); } else { chartArea.AxisX.ScaleView.ZoomReset(); chartArea.AxisX.Minimum = Double.NaN; chartArea.AxisX.Maximum = Double.NaN; } chartArea.AxisX.ScrollBar.Enabled = false; }
            // Sửa: Bỏ comment Console.WriteLine để dùng biến ex
            catch (Exception ex) { Console.WriteLine("Lỗi khi đặt ScaleView/Min/Max X: " + ex.Message); chartArea.AxisX.ScaleView.ZoomReset(); chartArea.AxisX.Minimum = Double.NaN; chartArea.AxisX.Maximum = Double.NaN; }
        }

        // --- Clear Chart ---
        private void ClearChart()
        {
            if (chartTemperature != null && chartTemperature.IsHandleCreated) { while (chartTemperature.Series.Count > 0) { chartTemperature.Series.RemoveAt(0); } chartTemperature.Annotations.Clear(); if (chartTemperature.ChartAreas.Count > 0) { var chartArea = chartTemperature.ChartAreas[0]; chartArea.AxisX.CustomLabels.Clear(); chartArea.AxisY.Minimum = Double.NaN; chartArea.AxisY.Maximum = Double.NaN; chartArea.AxisX.Minimum = Double.NaN; chartArea.AxisX.Maximum = Double.NaN; chartArea.AxisX.ScaleView.ZoomReset(); } chartTemperature.Invalidate(); }
        }

        // Add the missing method ClearChartAndSeries to resolve the CS0103 error.  
        private void ClearChartAndSeries(Chart chartToClear)
        {
            if (chartToClear != null && chartToClear.IsHandleCreated)
            {
                // Remove all series from the chart  
                while (chartToClear.Series.Count > 0)
                {
                    chartToClear.Series.RemoveAt(0);
                }

                // Clear annotations  
                chartToClear.Annotations.Clear();

                // Reset chart area properties if any exist  
                if (chartToClear.ChartAreas.Count > 0)
                {
                    var chartArea = chartToClear.ChartAreas[0];
                    chartArea.AxisX.CustomLabels.Clear();
                    chartArea.AxisY.Minimum = Double.NaN;
                    chartArea.AxisY.Maximum = Double.NaN;
                    chartArea.AxisX.Minimum = Double.NaN;
                    chartArea.AxisX.Maximum = Double.NaN;
                    chartArea.AxisX.ScaleView.ZoomReset();
                }
                chartToClear.Invalidate();
            }
        }

        private class WeatherData
        {
            public DateTime NgayGio { get; set; }
            public int? NhietDo { get; set; }
            public int? XacSuatMua { get; set; } 
            public double? TocDoGio { get; set; }
            public double? LuongMua { get; set; } 
        }
    } 
} 