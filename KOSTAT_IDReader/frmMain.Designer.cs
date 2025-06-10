
namespace KOSTAT_IDReader
{
    partial class frmMain
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing && (components != null))
        //    {
        //        components.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 기존 타이머 정리
                dateTimeTimer?.Stop();
                dateTimeTimer?.Dispose();

                // 자동 트리거 타이머 정리
                autoTriggerTimer?.Stop();
                autoTriggerTimer?.Dispose();

                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.LeftPanel = new System.Windows.Forms.TableLayoutPanel();
            this.LeftControlPanel = new System.Windows.Forms.TableLayoutPanel();
            this.bt_Trigger = new System.Windows.Forms.Button();
            this.nd_NRead = new System.Windows.Forms.NumericUpDown();
            this.SelectPathPanel = new System.Windows.Forms.TableLayoutPanel();
            this.bt_SelectPath = new System.Windows.Forms.Button();
            this.tb_Path = new System.Windows.Forms.TextBox();
            this.rtxLog = new System.Windows.Forms.RichTextBox();
            this.ConnectionPanel = new System.Windows.Forms.TableLayoutPanel();
            this.lb_bLaserCon = new System.Windows.Forms.Label();
            this.lb_bCamCon = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lb_DateTime = new System.Windows.Forms.Label();
            this.RightPanel = new System.Windows.Forms.TableLayoutPanel();
            this.bt_Exit = new System.Windows.Forms.Button();
            this.dgv_Data = new System.Windows.Forms.DataGridView();
            this._dateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MatchData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReadData = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Result = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lb_CamName = new System.Windows.Forms.Label();
            this.MainTablePanel = new System.Windows.Forms.TableLayoutPanel();
            this.MainDisplay = new System.Windows.Forms.PictureBox();
            this.LeftPanel.SuspendLayout();
            this.LeftControlPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nd_NRead)).BeginInit();
            this.SelectPathPanel.SuspendLayout();
            this.ConnectionPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.RightPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Data)).BeginInit();
            this.MainTablePanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MainDisplay)).BeginInit();
            this.SuspendLayout();
            // 
            // LeftPanel
            // 
            this.LeftPanel.ColumnCount = 1;
            this.LeftPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.LeftPanel.Controls.Add(this.LeftControlPanel, 0, 0);
            this.LeftPanel.Controls.Add(this.rtxLog, 0, 1);
            this.LeftPanel.Controls.Add(this.ConnectionPanel, 0, 2);
            this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftPanel.Location = new System.Drawing.Point(0, 88);
            this.LeftPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LeftPanel.Name = "LeftPanel";
            this.LeftPanel.RowCount = 3;
            this.LeftPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 21.29436F));
            this.LeftPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 78.70564F));
            this.LeftPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.LeftPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.LeftPanel.Size = new System.Drawing.Size(210, 872);
            this.LeftPanel.TabIndex = 1220;
            // 
            // LeftControlPanel
            // 
            this.LeftControlPanel.ColumnCount = 1;
            this.LeftControlPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.LeftControlPanel.Controls.Add(this.bt_Trigger, 0, 0);
            this.LeftControlPanel.Controls.Add(this.nd_NRead, 0, 2);
            this.LeftControlPanel.Controls.Add(this.SelectPathPanel, 0, 1);
            this.LeftControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftControlPanel.Location = new System.Drawing.Point(0, 0);
            this.LeftControlPanel.Margin = new System.Windows.Forms.Padding(0);
            this.LeftControlPanel.Name = "LeftControlPanel";
            this.LeftControlPanel.RowCount = 3;
            this.LeftControlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
            this.LeftControlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 35F));
            this.LeftControlPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.LeftControlPanel.Size = new System.Drawing.Size(210, 177);
            this.LeftControlPanel.TabIndex = 1;
            // 
            // bt_Trigger
            // 
            this.bt_Trigger.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bt_Trigger.Font = new System.Drawing.Font("Arial", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_Trigger.Location = new System.Drawing.Point(0, 0);
            this.bt_Trigger.Margin = new System.Windows.Forms.Padding(0);
            this.bt_Trigger.Name = "bt_Trigger";
            this.bt_Trigger.Size = new System.Drawing.Size(210, 79);
            this.bt_Trigger.TabIndex = 0;
            this.bt_Trigger.Text = "Trigger";
            this.bt_Trigger.UseVisualStyleBackColor = true;
            this.bt_Trigger.MouseDown += new System.Windows.Forms.MouseEventHandler(this.bt_Trigger_MouseDown);
            this.bt_Trigger.MouseUp += new System.Windows.Forms.MouseEventHandler(this.bt_Trigger_MouseUp);
            // 
            // nd_NRead
            // 
            this.nd_NRead.Dock = System.Windows.Forms.DockStyle.Fill;
            this.nd_NRead.Font = new System.Drawing.Font("굴림", 14F, System.Drawing.FontStyle.Bold);
            this.nd_NRead.Location = new System.Drawing.Point(0, 140);
            this.nd_NRead.Margin = new System.Windows.Forms.Padding(0);
            this.nd_NRead.Name = "nd_NRead";
            this.nd_NRead.Size = new System.Drawing.Size(210, 29);
            this.nd_NRead.TabIndex = 2;
            this.nd_NRead.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nd_NRead.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.nd_NRead.ValueChanged += new System.EventHandler(this.ng_NRead_ValueChanged);
            // 
            // SelectPathPanel
            // 
            this.SelectPathPanel.ColumnCount = 2;
            this.SelectPathPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80F));
            this.SelectPathPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 20F));
            this.SelectPathPanel.Controls.Add(this.bt_SelectPath, 1, 0);
            this.SelectPathPanel.Controls.Add(this.tb_Path, 0, 0);
            this.SelectPathPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SelectPathPanel.Location = new System.Drawing.Point(3, 82);
            this.SelectPathPanel.Name = "SelectPathPanel";
            this.SelectPathPanel.RowCount = 1;
            this.SelectPathPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.SelectPathPanel.Size = new System.Drawing.Size(204, 55);
            this.SelectPathPanel.TabIndex = 3;
            // 
            // bt_SelectPath
            // 
            this.bt_SelectPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bt_SelectPath.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_SelectPath.ForeColor = System.Drawing.Color.Black;
            this.bt_SelectPath.Location = new System.Drawing.Point(163, 0);
            this.bt_SelectPath.Margin = new System.Windows.Forms.Padding(0);
            this.bt_SelectPath.Name = "bt_SelectPath";
            this.bt_SelectPath.Size = new System.Drawing.Size(41, 55);
            this.bt_SelectPath.TabIndex = 2;
            this.bt_SelectPath.Text = "Path";
            this.bt_SelectPath.UseVisualStyleBackColor = true;
            this.bt_SelectPath.Click += new System.EventHandler(this.bt_SelectPath_Click);
            // 
            // tb_Path
            // 
            this.tb_Path.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tb_Path.Font = new System.Drawing.Font("굴림", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.tb_Path.Location = new System.Drawing.Point(3, 3);
            this.tb_Path.Multiline = true;
            this.tb_Path.Name = "tb_Path";
            this.tb_Path.Size = new System.Drawing.Size(157, 49);
            this.tb_Path.TabIndex = 0;
            // 
            // rtxLog
            // 
            this.rtxLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.rtxLog.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtxLog.ForeColor = System.Drawing.Color.Red;
            this.rtxLog.Location = new System.Drawing.Point(0, 177);
            this.rtxLog.Margin = new System.Windows.Forms.Padding(0);
            this.rtxLog.Name = "rtxLog";
            this.rtxLog.Size = new System.Drawing.Size(210, 656);
            this.rtxLog.TabIndex = 3;
            this.rtxLog.Text = "";
            // 
            // ConnectionPanel
            // 
            this.ConnectionPanel.ColumnCount = 2;
            this.ConnectionPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ConnectionPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ConnectionPanel.Controls.Add(this.lb_bLaserCon, 0, 0);
            this.ConnectionPanel.Controls.Add(this.lb_bCamCon, 0, 0);
            this.ConnectionPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConnectionPanel.Location = new System.Drawing.Point(3, 836);
            this.ConnectionPanel.Name = "ConnectionPanel";
            this.ConnectionPanel.RowCount = 1;
            this.ConnectionPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.ConnectionPanel.Size = new System.Drawing.Size(204, 33);
            this.ConnectionPanel.TabIndex = 2;
            // 
            // lb_bLaserCon
            // 
            this.lb_bLaserCon.BackColor = System.Drawing.Color.White;
            this.lb_bLaserCon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lb_bLaserCon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lb_bLaserCon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lb_bLaserCon.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold);
            this.lb_bLaserCon.ForeColor = System.Drawing.Color.Red;
            this.lb_bLaserCon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lb_bLaserCon.Location = new System.Drawing.Point(102, 0);
            this.lb_bLaserCon.Margin = new System.Windows.Forms.Padding(0);
            this.lb_bLaserCon.Name = "lb_bLaserCon";
            this.lb_bLaserCon.Size = new System.Drawing.Size(102, 33);
            this.lb_bLaserCon.TabIndex = 1221;
            this.lb_bLaserCon.Text = "Laser PC";
            this.lb_bLaserCon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lb_bCamCon
            // 
            this.lb_bCamCon.BackColor = System.Drawing.Color.White;
            this.lb_bCamCon.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lb_bCamCon.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lb_bCamCon.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lb_bCamCon.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold);
            this.lb_bCamCon.ForeColor = System.Drawing.Color.Red;
            this.lb_bCamCon.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lb_bCamCon.Location = new System.Drawing.Point(0, 0);
            this.lb_bCamCon.Margin = new System.Windows.Forms.Padding(0);
            this.lb_bCamCon.Name = "lb_bCamCon";
            this.lb_bCamCon.Size = new System.Drawing.Size(102, 33);
            this.lb_bCamCon.TabIndex = 1220;
            this.lb_bCamCon.Text = "IDReader";
            this.lb_bCamCon.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lb_bCamCon.Click += new System.EventHandler(this.lb_bCamCon_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox1.ErrorImage = null;
            this.pictureBox1.Image = global::KOSTAT_IDReader.Properties.Resources.qmea6t_8m7k_1ew7219_logo;
            this.pictureBox1.Location = new System.Drawing.Point(0, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(210, 88);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 1218;
            this.pictureBox1.TabStop = false;
            // 
            // lb_DateTime
            // 
            this.lb_DateTime.BackColor = System.Drawing.Color.White;
            this.lb_DateTime.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lb_DateTime.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lb_DateTime.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lb_DateTime.Font = new System.Drawing.Font("Arial", 12.75F, System.Drawing.FontStyle.Bold);
            this.lb_DateTime.ForeColor = System.Drawing.Color.Gray;
            this.lb_DateTime.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lb_DateTime.Location = new System.Drawing.Point(538, 0);
            this.lb_DateTime.Margin = new System.Windows.Forms.Padding(0);
            this.lb_DateTime.Name = "lb_DateTime";
            this.lb_DateTime.Size = new System.Drawing.Size(422, 88);
            this.lb_DateTime.TabIndex = 1217;
            this.lb_DateTime.Text = "0000/00/00 00:00:00";
            this.lb_DateTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // RightPanel
            // 
            this.RightPanel.ColumnCount = 1;
            this.RightPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.RightPanel.Controls.Add(this.bt_Exit, 0, 1);
            this.RightPanel.Controls.Add(this.dgv_Data, 0, 0);
            this.RightPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightPanel.Location = new System.Drawing.Point(538, 88);
            this.RightPanel.Margin = new System.Windows.Forms.Padding(0);
            this.RightPanel.Name = "RightPanel";
            this.RightPanel.RowCount = 2;
            this.RightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 89.52569F));
            this.RightPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10.47431F));
            this.RightPanel.Size = new System.Drawing.Size(422, 872);
            this.RightPanel.TabIndex = 0;
            // 
            // bt_Exit
            // 
            this.bt_Exit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bt_Exit.Font = new System.Drawing.Font("Arial", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.bt_Exit.ForeColor = System.Drawing.Color.Red;
            this.bt_Exit.Location = new System.Drawing.Point(0, 780);
            this.bt_Exit.Margin = new System.Windows.Forms.Padding(0);
            this.bt_Exit.Name = "bt_Exit";
            this.bt_Exit.Size = new System.Drawing.Size(422, 92);
            this.bt_Exit.TabIndex = 1;
            this.bt_Exit.Text = "Exit";
            this.bt_Exit.UseVisualStyleBackColor = true;
            this.bt_Exit.Click += new System.EventHandler(this.Exit_Click);
            // 
            // dgv_Data
            // 
            this.dgv_Data.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgv_Data.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this._dateTime,
            this.MatchData,
            this.ReadData,
            this.Result});
            this.dgv_Data.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgv_Data.Location = new System.Drawing.Point(0, 0);
            this.dgv_Data.Margin = new System.Windows.Forms.Padding(0);
            this.dgv_Data.Name = "dgv_Data";
            this.dgv_Data.RowHeadersVisible = false;
            this.dgv_Data.RowTemplate.Height = 23;
            this.dgv_Data.Size = new System.Drawing.Size(422, 780);
            this.dgv_Data.TabIndex = 2;
            // 
            // _dateTime
            // 
            this._dateTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this._dateTime.HeaderText = "DateTime";
            this._dateTime.Name = "_dateTime";
            this._dateTime.Width = 84;
            // 
            // MatchData
            // 
            this.MatchData.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.MatchData.HeaderText = "Match Data";
            this.MatchData.Name = "MatchData";
            this.MatchData.Width = 94;
            // 
            // ReadData
            // 
            this.ReadData.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.ReadData.HeaderText = "Read Data";
            this.ReadData.Name = "ReadData";
            this.ReadData.Width = 88;
            // 
            // Result
            // 
            this.Result.HeaderText = "Result";
            this.Result.Name = "Result";
            // 
            // lb_CamName
            // 
            this.lb_CamName.BackColor = System.Drawing.Color.White;
            this.lb_CamName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lb_CamName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lb_CamName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lb_CamName.Font = new System.Drawing.Font("Arial", 20F, System.Drawing.FontStyle.Bold);
            this.lb_CamName.ForeColor = System.Drawing.Color.Gray;
            this.lb_CamName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.lb_CamName.Location = new System.Drawing.Point(210, 0);
            this.lb_CamName.Margin = new System.Windows.Forms.Padding(0);
            this.lb_CamName.Name = "lb_CamName";
            this.lb_CamName.Size = new System.Drawing.Size(328, 88);
            this.lb_CamName.TabIndex = 1219;
            this.lb_CamName.Text = "ID Reader Name";
            this.lb_CamName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // MainTablePanel
            // 
            this.MainTablePanel.BackColor = System.Drawing.Color.White;
            this.MainTablePanel.ColumnCount = 3;
            this.MainTablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 39.0169F));
            this.MainTablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60.9831F));
            this.MainTablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 421F));
            this.MainTablePanel.Controls.Add(this.lb_CamName, 1, 0);
            this.MainTablePanel.Controls.Add(this.RightPanel, 2, 1);
            this.MainTablePanel.Controls.Add(this.lb_DateTime, 2, 0);
            this.MainTablePanel.Controls.Add(this.pictureBox1, 0, 0);
            this.MainTablePanel.Controls.Add(this.LeftPanel, 0, 1);
            this.MainTablePanel.Controls.Add(this.MainDisplay, 1, 1);
            this.MainTablePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainTablePanel.Location = new System.Drawing.Point(0, 0);
            this.MainTablePanel.Margin = new System.Windows.Forms.Padding(0);
            this.MainTablePanel.Name = "MainTablePanel";
            this.MainTablePanel.RowCount = 2;
            this.MainTablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.246575F));
            this.MainTablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.75343F));
            this.MainTablePanel.Size = new System.Drawing.Size(960, 960);
            this.MainTablePanel.TabIndex = 2;
            // 
            // MainDisplay
            // 
            this.MainDisplay.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.MainDisplay.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainDisplay.Location = new System.Drawing.Point(213, 91);
            this.MainDisplay.Name = "MainDisplay";
            this.MainDisplay.Size = new System.Drawing.Size(322, 866);
            this.MainDisplay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.MainDisplay.TabIndex = 1221;
            this.MainDisplay.TabStop = false;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 960);
            this.Controls.Add(this.MainTablePanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main Form";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmMain_FormClosing);
            this.Load += new System.EventHandler(this.frmMain_Load);
            this.LeftPanel.ResumeLayout(false);
            this.LeftControlPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.nd_NRead)).EndInit();
            this.SelectPathPanel.ResumeLayout(false);
            this.SelectPathPanel.PerformLayout();
            this.ConnectionPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.RightPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgv_Data)).EndInit();
            this.MainTablePanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MainDisplay)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel LeftPanel;
        private System.Windows.Forms.TableLayoutPanel LeftControlPanel;
        private System.Windows.Forms.Button bt_Trigger;
        private System.Windows.Forms.NumericUpDown nd_NRead;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lb_DateTime;
        private System.Windows.Forms.TableLayoutPanel RightPanel;
        private System.Windows.Forms.Button bt_Exit;
        private System.Windows.Forms.Label lb_CamName;
        private System.Windows.Forms.TableLayoutPanel MainTablePanel;
        private System.Windows.Forms.TableLayoutPanel ConnectionPanel;
        private System.Windows.Forms.Label lb_bLaserCon;
        private System.Windows.Forms.Label lb_bCamCon;
        private System.Windows.Forms.DataGridView dgv_Data;
        private System.Windows.Forms.PictureBox MainDisplay;
        private System.Windows.Forms.DataGridViewTextBoxColumn _dateTime;
        private System.Windows.Forms.DataGridViewTextBoxColumn MatchData;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReadData;
        private System.Windows.Forms.DataGridViewTextBoxColumn Result;
        private System.Windows.Forms.RichTextBox rtxLog;
        private System.Windows.Forms.TableLayoutPanel SelectPathPanel;
        private System.Windows.Forms.Button bt_SelectPath;
        private System.Windows.Forms.TextBox tb_Path;
    }
}

