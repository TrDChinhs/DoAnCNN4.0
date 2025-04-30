namespace DoAn6._0
{
    partial class Admin
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.dgvRuong = new System.Windows.Forms.DataGridView();
            this.rIDRuongDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.cRIDChuRuongDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rDienTichDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rViTriDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rLoaiDatDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.qLRuongBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.quanLyNongTraiDataSet1 = new DoAn6._0.QuanLyNongTraiDataSet1();
            this.cmbChuRuong = new System.Windows.Forms.ComboBox();
            this.txtIDRuong = new System.Windows.Forms.TextBox();
            this.btnXoa = new System.Windows.Forms.Button();
            this.txtDienTich = new System.Windows.Forms.TextBox();
            this.txtViTri = new System.Windows.Forms.TextBox();
            this.txtLoaiDat = new System.Windows.Forms.TextBox();
            this.btnLuu = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnLamMoi = new System.Windows.Forms.Button();
            this.qLRuongTableAdapter = new DoAn6._0.QuanLyNongTraiDataSet1TableAdapters.QLRuongTableAdapter();
            this.btnThem = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRuong)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.qLRuongBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.quanLyNongTraiDataSet1)).BeginInit();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dgvRuong
            // 
            this.dgvRuong.AutoGenerateColumns = false;
            this.dgvRuong.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvRuong.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRuong.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.rIDRuongDataGridViewTextBoxColumn,
            this.cRIDChuRuongDataGridViewTextBoxColumn,
            this.rDienTichDataGridViewTextBoxColumn,
            this.rViTriDataGridViewTextBoxColumn,
            this.rLoaiDatDataGridViewTextBoxColumn});
            this.dgvRuong.DataSource = this.qLRuongBindingSource;
            this.dgvRuong.Dock = System.Windows.Forms.DockStyle.Top;
            this.dgvRuong.Location = new System.Drawing.Point(0, 0);
            this.dgvRuong.Name = "dgvRuong";
            this.dgvRuong.RowHeadersWidth = 51;
            this.dgvRuong.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.dgvRuong.Size = new System.Drawing.Size(864, 150);
            this.dgvRuong.TabIndex = 0;
            this.dgvRuong.SelectionChanged += new System.EventHandler(this.dgvRuong_SelectionChanged);
            // 
            // rIDRuongDataGridViewTextBoxColumn
            // 
            this.rIDRuongDataGridViewTextBoxColumn.DataPropertyName = "R_IDRuong";
            this.rIDRuongDataGridViewTextBoxColumn.HeaderText = "R_IDRuong";
            this.rIDRuongDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.rIDRuongDataGridViewTextBoxColumn.Name = "rIDRuongDataGridViewTextBoxColumn";
            // 
            // cRIDChuRuongDataGridViewTextBoxColumn
            // 
            this.cRIDChuRuongDataGridViewTextBoxColumn.DataPropertyName = "CR_IDChuRuong";
            this.cRIDChuRuongDataGridViewTextBoxColumn.HeaderText = "CR_IDChuRuong";
            this.cRIDChuRuongDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.cRIDChuRuongDataGridViewTextBoxColumn.Name = "cRIDChuRuongDataGridViewTextBoxColumn";
            // 
            // rDienTichDataGridViewTextBoxColumn
            // 
            this.rDienTichDataGridViewTextBoxColumn.DataPropertyName = "R_DienTich";
            this.rDienTichDataGridViewTextBoxColumn.HeaderText = "R_DienTich";
            this.rDienTichDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.rDienTichDataGridViewTextBoxColumn.Name = "rDienTichDataGridViewTextBoxColumn";
            // 
            // rViTriDataGridViewTextBoxColumn
            // 
            this.rViTriDataGridViewTextBoxColumn.DataPropertyName = "R_ViTri";
            this.rViTriDataGridViewTextBoxColumn.HeaderText = "R_ViTri";
            this.rViTriDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.rViTriDataGridViewTextBoxColumn.Name = "rViTriDataGridViewTextBoxColumn";
            // 
            // rLoaiDatDataGridViewTextBoxColumn
            // 
            this.rLoaiDatDataGridViewTextBoxColumn.DataPropertyName = "R_LoaiDat";
            this.rLoaiDatDataGridViewTextBoxColumn.HeaderText = "R_LoaiDat";
            this.rLoaiDatDataGridViewTextBoxColumn.MinimumWidth = 6;
            this.rLoaiDatDataGridViewTextBoxColumn.Name = "rLoaiDatDataGridViewTextBoxColumn";
            // 
            // qLRuongBindingSource
            // 
            this.qLRuongBindingSource.DataMember = "QLRuong";
            this.qLRuongBindingSource.DataSource = this.quanLyNongTraiDataSet1;
            // 
            // quanLyNongTraiDataSet1
            // 
            this.quanLyNongTraiDataSet1.DataSetName = "QuanLyNongTraiDataSet1";
            this.quanLyNongTraiDataSet1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // cmbChuRuong
            // 
            this.cmbChuRuong.FormattingEnabled = true;
            this.cmbChuRuong.Location = new System.Drawing.Point(660, 185);
            this.cmbChuRuong.Name = "cmbChuRuong";
            this.cmbChuRuong.Size = new System.Drawing.Size(176, 24);
            this.cmbChuRuong.TabIndex = 1;
            // 
            // txtIDRuong
            // 
            this.txtIDRuong.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIDRuong.Location = new System.Drawing.Point(155, 188);
            this.txtIDRuong.Name = "txtIDRuong";
            this.txtIDRuong.Size = new System.Drawing.Size(168, 22);
            this.txtIDRuong.TabIndex = 2;
            // 
            // btnXoa
            // 
            this.btnXoa.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(188)))), ((int)(((byte)(156)))));
            this.btnXoa.ForeColor = System.Drawing.Color.White;
            this.btnXoa.Location = new System.Drawing.Point(435, 294);
            this.btnXoa.Name = "btnXoa";
            this.btnXoa.Size = new System.Drawing.Size(110, 43);
            this.btnXoa.TabIndex = 3;
            this.btnXoa.Text = "Xóa";
            this.btnXoa.UseVisualStyleBackColor = false;
            this.btnXoa.Click += new System.EventHandler(this.btnXoa_Click_1);
            // 
            // txtDienTich
            // 
            this.txtDienTich.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDienTich.Location = new System.Drawing.Point(155, 228);
            this.txtDienTich.Name = "txtDienTich";
            this.txtDienTich.Size = new System.Drawing.Size(168, 22);
            this.txtDienTich.TabIndex = 2;
            // 
            // txtViTri
            // 
            this.txtViTri.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtViTri.Location = new System.Drawing.Point(470, 228);
            this.txtViTri.Multiline = true;
            this.txtViTri.Name = "txtViTri";
            this.txtViTri.Size = new System.Drawing.Size(168, 41);
            this.txtViTri.TabIndex = 2;
            // 
            // txtLoaiDat
            // 
            this.txtLoaiDat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLoaiDat.Location = new System.Drawing.Point(470, 186);
            this.txtLoaiDat.Name = "txtLoaiDat";
            this.txtLoaiDat.Size = new System.Drawing.Size(168, 22);
            this.txtLoaiDat.TabIndex = 2;
            // 
            // btnLuu
            // 
            this.btnLuu.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(188)))), ((int)(((byte)(156)))));
            this.btnLuu.ForeColor = System.Drawing.Color.White;
            this.btnLuu.Location = new System.Drawing.Point(551, 294);
            this.btnLuu.Name = "btnLuu";
            this.btnLuu.Size = new System.Drawing.Size(110, 43);
            this.btnLuu.TabIndex = 3;
            this.btnLuu.Text = "Lưu";
            this.btnLuu.UseVisualStyleBackColor = false;
            this.btnLuu.Click += new System.EventHandler(this.btnLuu_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(43, 188);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(66, 16);
            this.label1.TabIndex = 4;
            this.label1.Text = "ID Ruộng:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 230);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Diện tích:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(398, 230);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 16);
            this.label3.TabIndex = 4;
            this.label3.Text = "Vị trí:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(390, 190);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 16);
            this.label4.TabIndex = 4;
            this.label4.Text = "Loại đất:";
            // 
            // btnLamMoi
            // 
            this.btnLamMoi.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(188)))), ((int)(((byte)(156)))));
            this.btnLamMoi.ForeColor = System.Drawing.Color.White;
            this.btnLamMoi.Location = new System.Drawing.Point(203, 294);
            this.btnLamMoi.Name = "btnLamMoi";
            this.btnLamMoi.Size = new System.Drawing.Size(110, 43);
            this.btnLamMoi.TabIndex = 5;
            this.btnLamMoi.Text = "Làm Mới";
            this.btnLamMoi.UseVisualStyleBackColor = false;
            this.btnLamMoi.Click += new System.EventHandler(this.btnLamMoi_Click_1);
            // 
            // qLRuongTableAdapter
            // 
            this.qLRuongTableAdapter.ClearBeforeFill = true;
            // 
            // btnThem
            // 
            this.btnThem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(188)))), ((int)(((byte)(156)))));
            this.btnThem.ForeColor = System.Drawing.Color.White;
            this.btnThem.Location = new System.Drawing.Point(319, 294);
            this.btnThem.Name = "btnThem";
            this.btnThem.Size = new System.Drawing.Size(110, 43);
            this.btnThem.TabIndex = 6;
            this.btnThem.Text = "Thêm";
            this.btnThem.UseVisualStyleBackColor = false;
            this.btnThem.Click += new System.EventHandler(this.btnThem_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.dgvRuong);
            this.panel1.Controls.Add(this.btnThem);
            this.panel1.Controls.Add(this.cmbChuRuong);
            this.panel1.Controls.Add(this.btnLamMoi);
            this.panel1.Controls.Add(this.txtIDRuong);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.txtLoaiDat);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.txtDienTich);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.txtViTri);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.btnXoa);
            this.panel1.Controls.Add(this.btnLuu);
            this.panel1.Location = new System.Drawing.Point(152, 91);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(864, 363);
            this.panel1.TabIndex = 7;
            // 
            // Admin
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(25)))), ((int)(((byte)(188)))), ((int)(((byte)(156)))));
            this.ClientSize = new System.Drawing.Size(1168, 626);
            this.Controls.Add(this.panel1);
            this.Name = "Admin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Admin";
            this.Load += new System.EventHandler(this.Admin_Load_1);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRuong)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.qLRuongBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.quanLyNongTraiDataSet1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvRuong;
        private System.Windows.Forms.ComboBox cmbChuRuong;
        private System.Windows.Forms.TextBox txtIDRuong;
        private System.Windows.Forms.Button btnXoa;
        private System.Windows.Forms.TextBox txtDienTich;
        private System.Windows.Forms.TextBox txtViTri;
        private System.Windows.Forms.TextBox txtLoaiDat;
        private System.Windows.Forms.Button btnLuu;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnLamMoi;
        private QuanLyNongTraiDataSet1 quanLyNongTraiDataSet1;
        private System.Windows.Forms.BindingSource qLRuongBindingSource;
        private QuanLyNongTraiDataSet1TableAdapters.QLRuongTableAdapter qLRuongTableAdapter;
        private System.Windows.Forms.DataGridViewTextBoxColumn rIDRuongDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn cRIDChuRuongDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rDienTichDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rViTriDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn rLoaiDatDataGridViewTextBoxColumn;
        private System.Windows.Forms.Button btnThem;
        private System.Windows.Forms.Panel panel1;
    }
}