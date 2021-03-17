
namespace WindowsFormsAppCamera
{
    partial class Form1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.pictCamera = new System.Windows.Forms.PictureBox();
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.lblRed = new System.Windows.Forms.Label();
            this.lblRedCount = new System.Windows.Forms.Label();
            this.btnEraseCalibrate = new System.Windows.Forms.Button();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lblDrones = new System.Windows.Forms.Label();
            this.btnSaveBmp = new System.Windows.Forms.Button();
            this.btnToggleBlankOrLiveScreen = new System.Windows.Forms.Button();
            this.btnRecalLeftLess = new System.Windows.Forms.Button();
            this.btnRecalLeftMore = new System.Windows.Forms.Button();
            this.btnRecalRightMore = new System.Windows.Forms.Button();
            this.btnRecalRightLess = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.btnAllUp = new System.Windows.Forms.Button();
            this.cmbCamera = new System.Windows.Forms.ComboBox();
            this.cmbCameraFormat = new System.Windows.Forms.ComboBox();
            this.cmbComPorts = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnTestComPort = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnPressRB = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.lblGreenCount = new System.Windows.Forms.Label();
            this.lblBlueCount = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictCamera)).BeginInit();
            this.SuspendLayout();
            // 
            // pictCamera
            // 
            this.pictCamera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictCamera.Location = new System.Drawing.Point(25, 134);
            this.pictCamera.Name = "pictCamera";
            this.pictCamera.Size = new System.Drawing.Size(788, 419);
            this.pictCamera.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictCamera.TabIndex = 0;
            this.pictCamera.TabStop = false;
            this.pictCamera.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.Location = new System.Drawing.Point(25, 571);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(106, 61);
            this.btnCalibrate.TabIndex = 1;
            this.btnCalibrate.Text = "Calibrate";
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblRed
            // 
            this.lblRed.AutoSize = true;
            this.lblRed.Location = new System.Drawing.Point(28, 663);
            this.lblRed.Name = "lblRed";
            this.lblRed.Size = new System.Drawing.Size(25, 20);
            this.lblRed.TabIndex = 2;
            this.lblRed.Text = "R:";
            // 
            // lblRedCount
            // 
            this.lblRedCount.AutoSize = true;
            this.lblRedCount.Location = new System.Drawing.Point(59, 663);
            this.lblRedCount.Name = "lblRedCount";
            this.lblRedCount.Size = new System.Drawing.Size(18, 20);
            this.lblRedCount.TabIndex = 8;
            this.lblRedCount.Text = "0";
            // 
            // btnEraseCalibrate
            // 
            this.btnEraseCalibrate.Location = new System.Drawing.Point(137, 571);
            this.btnEraseCalibrate.Name = "btnEraseCalibrate";
            this.btnEraseCalibrate.Size = new System.Drawing.Size(159, 61);
            this.btnEraseCalibrate.TabIndex = 9;
            this.btnEraseCalibrate.Text = "Erase Calibration";
            this.btnEraseCalibrate.UseVisualStyleBackColor = true;
            this.btnEraseCalibrate.Click += new System.EventHandler(this.btnEraseCalibrate_Click);
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(707, 571);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(106, 61);
            this.btnStart.TabIndex = 13;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(595, 571);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(106, 61);
            this.btnStop.TabIndex = 14;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.button3_Click);
            // 
            // lblDrones
            // 
            this.lblDrones.AutoSize = true;
            this.lblDrones.Location = new System.Drawing.Point(838, 223);
            this.lblDrones.Name = "lblDrones";
            this.lblDrones.Size = new System.Drawing.Size(0, 20);
            this.lblDrones.TabIndex = 15;
            // 
            // btnSaveBmp
            // 
            this.btnSaveBmp.Location = new System.Drawing.Point(388, 571);
            this.btnSaveBmp.Name = "btnSaveBmp";
            this.btnSaveBmp.Size = new System.Drawing.Size(95, 61);
            this.btnSaveBmp.TabIndex = 16;
            this.btnSaveBmp.Text = "Save BMP";
            this.btnSaveBmp.UseVisualStyleBackColor = true;
            this.btnSaveBmp.Click += new System.EventHandler(this.btnSaveBmp_Click);
            // 
            // btnToggleBlankOrLiveScreen
            // 
            this.btnToggleBlankOrLiveScreen.Location = new System.Drawing.Point(489, 571);
            this.btnToggleBlankOrLiveScreen.Name = "btnToggleBlankOrLiveScreen";
            this.btnToggleBlankOrLiveScreen.Size = new System.Drawing.Size(95, 61);
            this.btnToggleBlankOrLiveScreen.TabIndex = 17;
            this.btnToggleBlankOrLiveScreen.Text = "Flip to Blank";
            this.btnToggleBlankOrLiveScreen.UseVisualStyleBackColor = true;
            this.btnToggleBlankOrLiveScreen.Click += new System.EventHandler(this.btnToggleBlankOrLiveScreen_Click);
            // 
            // btnRecalLeftLess
            // 
            this.btnRecalLeftLess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRecalLeftLess.Location = new System.Drawing.Point(444, 692);
            this.btnRecalLeftLess.Name = "btnRecalLeftLess";
            this.btnRecalLeftLess.Size = new System.Drawing.Size(67, 64);
            this.btnRecalLeftLess.TabIndex = 18;
            this.btnRecalLeftLess.Text = "Less";
            this.btnRecalLeftLess.UseVisualStyleBackColor = true;
            this.btnRecalLeftLess.Click += new System.EventHandler(this.btnRecalLeftLess_Click);
            // 
            // btnRecalLeftMore
            // 
            this.btnRecalLeftMore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRecalLeftMore.Location = new System.Drawing.Point(517, 692);
            this.btnRecalLeftMore.Name = "btnRecalLeftMore";
            this.btnRecalLeftMore.Size = new System.Drawing.Size(67, 64);
            this.btnRecalLeftMore.TabIndex = 19;
            this.btnRecalLeftMore.Text = "More";
            this.btnRecalLeftMore.UseMnemonic = false;
            this.btnRecalLeftMore.UseVisualStyleBackColor = true;
            this.btnRecalLeftMore.Click += new System.EventHandler(this.btnRecalLeftMore_Click);
            // 
            // btnRecalRightMore
            // 
            this.btnRecalRightMore.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRecalRightMore.Location = new System.Drawing.Point(746, 692);
            this.btnRecalRightMore.Name = "btnRecalRightMore";
            this.btnRecalRightMore.Size = new System.Drawing.Size(67, 64);
            this.btnRecalRightMore.TabIndex = 21;
            this.btnRecalRightMore.Text = "More";
            this.btnRecalRightMore.UseVisualStyleBackColor = true;
            this.btnRecalRightMore.Click += new System.EventHandler(this.btnRecalRightMore_Click);
            // 
            // btnRecalRightLess
            // 
            this.btnRecalRightLess.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnRecalRightLess.Location = new System.Drawing.Point(673, 692);
            this.btnRecalRightLess.Name = "btnRecalRightLess";
            this.btnRecalRightLess.Size = new System.Drawing.Size(67, 64);
            this.btnRecalRightLess.TabIndex = 20;
            this.btnRecalRightLess.Text = "Less";
            this.btnRecalRightLess.UseVisualStyleBackColor = true;
            this.btnRecalRightLess.Click += new System.EventHandler(this.btnRecalRightLess_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(367, 669);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 20);
            this.label4.TabIndex = 22;
            this.label4.Text = "LB";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(596, 669);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(34, 20);
            this.label5.TabIndex = 23;
            this.label5.Text = "RB";
            // 
            // btnAllUp
            // 
            this.btnAllUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAllUp.Location = new System.Drawing.Point(276, 692);
            this.btnAllUp.Name = "btnAllUp";
            this.btnAllUp.Size = new System.Drawing.Size(67, 64);
            this.btnAllUp.TabIndex = 24;
            this.btnAllUp.Text = "AllUp";
            this.btnAllUp.UseVisualStyleBackColor = true;
            this.btnAllUp.Click += new System.EventHandler(this.btnAllUp_Click);
            // 
            // cmbCamera
            // 
            this.cmbCamera.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCamera.FormattingEnabled = true;
            this.cmbCamera.Location = new System.Drawing.Point(138, 12);
            this.cmbCamera.Name = "cmbCamera";
            this.cmbCamera.Size = new System.Drawing.Size(537, 28);
            this.cmbCamera.TabIndex = 25;
            this.cmbCamera.SelectedIndexChanged += new System.EventHandler(this.cmbCamera_SelectedIndexChanged);
            // 
            // cmbCameraFormat
            // 
            this.cmbCameraFormat.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbCameraFormat.FormattingEnabled = true;
            this.cmbCameraFormat.Location = new System.Drawing.Point(138, 51);
            this.cmbCameraFormat.Name = "cmbCameraFormat";
            this.cmbCameraFormat.Size = new System.Drawing.Size(537, 28);
            this.cmbCameraFormat.TabIndex = 26;
            this.cmbCameraFormat.SelectedIndexChanged += new System.EventHandler(this.cmbCameraFormat_SelectedIndexChanged);
            // 
            // cmbComPorts
            // 
            this.cmbComPorts.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbComPorts.FormattingEnabled = true;
            this.cmbComPorts.Location = new System.Drawing.Point(138, 89);
            this.cmbComPorts.Name = "cmbComPorts";
            this.cmbComPorts.Size = new System.Drawing.Size(537, 28);
            this.cmbComPorts.TabIndex = 27;
            this.cmbComPorts.SelectedIndexChanged += new System.EventHandler(this.comboBox1_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 20);
            this.label1.TabIndex = 28;
            this.label1.Text = "Camera";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(21, 59);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(105, 20);
            this.label3.TabIndex = 29;
            this.label3.Text = "Video Format";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(21, 97);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(78, 20);
            this.label6.TabIndex = 30;
            this.label6.Text = "COM Port";
            // 
            // btnTestComPort
            // 
            this.btnTestComPort.Enabled = false;
            this.btnTestComPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTestComPort.Location = new System.Drawing.Point(697, 87);
            this.btnTestComPort.Name = "btnTestComPort";
            this.btnTestComPort.Size = new System.Drawing.Size(68, 31);
            this.btnTestComPort.TabIndex = 31;
            this.btnTestComPort.Text = "Test";
            this.btnTestComPort.UseVisualStyleBackColor = true;
            this.btnTestComPort.Click += new System.EventHandler(this.btnTestComPort_Click);
            // 
            // button2
            // 
            this.button2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.button2.Location = new System.Drawing.Point(371, 692);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(67, 64);
            this.button2.TabIndex = 32;
            this.button2.Text = "Press";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // btnPressRB
            // 
            this.btnPressRB.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.btnPressRB.Location = new System.Drawing.Point(600, 692);
            this.btnPressRB.Name = "btnPressRB";
            this.btnPressRB.Size = new System.Drawing.Size(67, 64);
            this.btnPressRB.TabIndex = 33;
            this.btnPressRB.Text = "Press";
            this.btnPressRB.UseVisualStyleBackColor = true;
            this.btnPressRB.Click += new System.EventHandler(this.button3_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 697);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 20);
            this.label2.TabIndex = 34;
            this.label2.Text = "G:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(28, 731);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(24, 20);
            this.label7.TabIndex = 35;
            this.label7.Text = "B:";
            // 
            // lblGreenCount
            // 
            this.lblGreenCount.AutoSize = true;
            this.lblGreenCount.Location = new System.Drawing.Point(60, 697);
            this.lblGreenCount.Name = "lblGreenCount";
            this.lblGreenCount.Size = new System.Drawing.Size(18, 20);
            this.lblGreenCount.TabIndex = 37;
            this.lblGreenCount.Text = "0";
            // 
            // lblBlueCount
            // 
            this.lblBlueCount.AutoSize = true;
            this.lblBlueCount.Location = new System.Drawing.Point(58, 731);
            this.lblBlueCount.Name = "lblBlueCount";
            this.lblBlueCount.Size = new System.Drawing.Size(18, 20);
            this.lblBlueCount.TabIndex = 38;
            this.lblBlueCount.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(844, 790);
            this.Controls.Add(this.lblBlueCount);
            this.Controls.Add(this.lblGreenCount);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnPressRB);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.btnTestComPort);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cmbComPorts);
            this.Controls.Add(this.cmbCameraFormat);
            this.Controls.Add(this.cmbCamera);
            this.Controls.Add(this.btnAllUp);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnRecalRightMore);
            this.Controls.Add(this.btnRecalRightLess);
            this.Controls.Add(this.btnRecalLeftMore);
            this.Controls.Add(this.btnRecalLeftLess);
            this.Controls.Add(this.btnToggleBlankOrLiveScreen);
            this.Controls.Add(this.btnSaveBmp);
            this.Controls.Add(this.lblDrones);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.btnEraseCalibrate);
            this.Controls.Add(this.lblRedCount);
            this.Controls.Add(this.lblRed);
            this.Controls.Add(this.btnCalibrate);
            this.Controls.Add(this.pictCamera);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "DivGrind Gen 2";
            this.TransparencyKey = System.Drawing.Color.Transparent;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_Closing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictCamera)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictCamera;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Label lblRed;
        private System.Windows.Forms.Label lblRedCount;
        private System.Windows.Forms.Button btnEraseCalibrate;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.Label lblDrones;
        private System.Windows.Forms.Button btnSaveBmp;
        private System.Windows.Forms.Button btnToggleBlankOrLiveScreen;
        private System.Windows.Forms.Button btnRecalLeftLess;
        private System.Windows.Forms.Button btnRecalLeftMore;
        private System.Windows.Forms.Button btnRecalRightMore;
        private System.Windows.Forms.Button btnRecalRightLess;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button btnAllUp;
        private System.Windows.Forms.ComboBox cmbCamera;
        private System.Windows.Forms.ComboBox cmbCameraFormat;
        private System.Windows.Forms.ComboBox cmbComPorts;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnTestComPort;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnPressRB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label lblGreenCount;
        private System.Windows.Forms.Label lblBlueCount;
    }
}

