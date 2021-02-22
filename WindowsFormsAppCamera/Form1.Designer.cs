
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
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lblRed = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.lblHeight = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblWidth = new System.Windows.Forms.Label();
            this.lblRedCount = new System.Windows.Forms.Label();
            this.btnEraseCalibrate = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.lblRedAvg = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(21, 23);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(788, 447);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(21, 488);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(106, 51);
            this.button1.TabIndex = 1;
            this.button1.Text = "Calibrate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblRed
            // 
            this.lblRed.AutoSize = true;
            this.lblRed.Location = new System.Drawing.Point(17, 557);
            this.lblRed.Name = "lblRed";
            this.lblRed.Size = new System.Drawing.Size(87, 20);
            this.lblRed.TabIndex = 2;
            this.lblRed.Text = "Red count:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(838, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 20);
            this.label1.TabIndex = 4;
            this.label1.Text = "Height:";
            // 
            // lblHeight
            // 
            this.lblHeight.AutoSize = true;
            this.lblHeight.Location = new System.Drawing.Point(838, 55);
            this.lblHeight.Name = "lblHeight";
            this.lblHeight.Size = new System.Drawing.Size(18, 20);
            this.lblHeight.TabIndex = 5;
            this.lblHeight.Text = "0";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(838, 89);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(50, 20);
            this.label3.TabIndex = 6;
            this.label3.Text = "Width";
            // 
            // lblWidth
            // 
            this.lblWidth.AutoSize = true;
            this.lblWidth.Location = new System.Drawing.Point(838, 120);
            this.lblWidth.Name = "lblWidth";
            this.lblWidth.Size = new System.Drawing.Size(18, 20);
            this.lblWidth.TabIndex = 7;
            this.lblWidth.Text = "0";
            // 
            // lblRedCount
            // 
            this.lblRedCount.AutoSize = true;
            this.lblRedCount.Location = new System.Drawing.Point(110, 557);
            this.lblRedCount.Name = "lblRedCount";
            this.lblRedCount.Size = new System.Drawing.Size(18, 20);
            this.lblRedCount.TabIndex = 8;
            this.lblRedCount.Text = "0";
            // 
            // btnEraseCalibrate
            // 
            this.btnEraseCalibrate.Location = new System.Drawing.Point(133, 488);
            this.btnEraseCalibrate.Name = "btnEraseCalibrate";
            this.btnEraseCalibrate.Size = new System.Drawing.Size(159, 51);
            this.btnEraseCalibrate.TabIndex = 9;
            this.btnEraseCalibrate.Text = "Erase Calibration";
            this.btnEraseCalibrate.UseVisualStyleBackColor = true;
            this.btnEraseCalibrate.Click += new System.EventHandler(this.btnEraseCalibrate_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(17, 588);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(72, 20);
            this.label2.TabIndex = 10;
            this.label2.Text = "Red avg:";
            // 
            // lblRedAvg
            // 
            this.lblRedAvg.AutoSize = true;
            this.lblRedAvg.Location = new System.Drawing.Point(110, 588);
            this.lblRedAvg.Name = "lblRedAvg";
            this.lblRedAvg.Size = new System.Drawing.Size(18, 20);
            this.lblRedAvg.TabIndex = 11;
            this.lblRedAvg.Text = "0";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 675);
            this.Controls.Add(this.lblRedAvg);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnEraseCalibrate);
            this.Controls.Add(this.lblRedCount);
            this.Controls.Add(this.lblWidth);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblHeight);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblRed);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblRed;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblHeight;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblWidth;
        private System.Windows.Forms.Label lblRedCount;
        private System.Windows.Forms.Button btnEraseCalibrate;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblRedAvg;
    }
}

