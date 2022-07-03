
namespace JxlExample
{
	partial class EncoderOptionsForm2
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EncoderOptionsForm2));
			this.cboEncodingSpeed = new System.Windows.Forms.ComboBox();
			this.lblEncodingSpeed = new System.Windows.Forms.Label();
			this.lblDecodingSpeed = new System.Windows.Forms.Label();
			this.cboDecodingSpeed = new System.Windows.Forms.ComboBox();
			this.lblEncodeSpeedHelp = new System.Windows.Forms.Label();
			this.lblDecodeSpeedHelp = new System.Windows.Forms.Label();
			this.boxCompressionMode = new System.Windows.Forms.GroupBox();
			this.optDrawing = new System.Windows.Forms.RadioButton();
			this.optPhoto = new System.Windows.Forms.RadioButton();
			this.optAuto = new System.Windows.Forms.RadioButton();
			this.optLossless = new System.Windows.Forms.RadioButton();
			this.txtQuality = new System.Windows.Forms.TextBox();
			this.lblQuality = new System.Windows.Forms.Label();
			this.lblQualityHelp = new System.Windows.Forms.Label();
			this.chkPreserveColor = new System.Windows.Forms.CheckBox();
			this.cmdOK = new System.Windows.Forms.Button();
			this.cmdCancel = new System.Windows.Forms.Button();
			this.boxCompressionMode.SuspendLayout();
			this.SuspendLayout();
			// 
			// cboEncodingSpeed
			// 
			this.cboEncodingSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboEncodingSpeed.FormattingEnabled = true;
			this.cboEncodingSpeed.Items.AddRange(new object[] {
            "1: Lightning",
            "2: Thunder",
            "3: Falcon",
            "4: Cheetah",
            "5: Hare",
            "6: Wombat",
            "7: Squirrel",
            "8: Kitten",
            "9: Tortoise"});
			this.cboEncodingSpeed.Location = new System.Drawing.Point(12, 25);
			this.cboEncodingSpeed.Name = "cboEncodingSpeed";
			this.cboEncodingSpeed.Size = new System.Drawing.Size(121, 21);
			this.cboEncodingSpeed.TabIndex = 2;
			// 
			// lblEncodingSpeed
			// 
			this.lblEncodingSpeed.AutoSize = true;
			this.lblEncodingSpeed.Location = new System.Drawing.Point(9, 9);
			this.lblEncodingSpeed.Name = "lblEncodingSpeed";
			this.lblEncodingSpeed.Size = new System.Drawing.Size(86, 13);
			this.lblEncodingSpeed.TabIndex = 1;
			this.lblEncodingSpeed.Text = "&Encoding Speed";
			// 
			// lblDecodingSpeed
			// 
			this.lblDecodingSpeed.AutoSize = true;
			this.lblDecodingSpeed.Location = new System.Drawing.Point(9, 49);
			this.lblDecodingSpeed.Name = "lblDecodingSpeed";
			this.lblDecodingSpeed.Size = new System.Drawing.Size(87, 13);
			this.lblDecodingSpeed.TabIndex = 4;
			this.lblDecodingSpeed.Text = "Decoding &Speed";
			// 
			// cboDecodingSpeed
			// 
			this.cboDecodingSpeed.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cboDecodingSpeed.FormattingEnabled = true;
			this.cboDecodingSpeed.Items.AddRange(new object[] {
            "0: Slowest decode",
            "1",
            "2",
            "3",
            "4: Fastest decode"});
			this.cboDecodingSpeed.Location = new System.Drawing.Point(12, 65);
			this.cboDecodingSpeed.Name = "cboDecodingSpeed";
			this.cboDecodingSpeed.Size = new System.Drawing.Size(121, 21);
			this.cboDecodingSpeed.TabIndex = 5;
			// 
			// lblEncodeSpeedHelp
			// 
			this.lblEncodeSpeedHelp.AutoSize = true;
			this.lblEncodeSpeedHelp.Location = new System.Drawing.Point(144, 12);
			this.lblEncodeSpeedHelp.Name = "lblEncodeSpeedHelp";
			this.lblEncodeSpeedHelp.Size = new System.Drawing.Size(319, 26);
			this.lblEncodeSpeedHelp.TabIndex = 0;
			this.lblEncodeSpeedHelp.Text = "Sets encoder effort/speed level without affecting decoding speed.\r\nDefault: 7: Sq" +
    "uirrel";
			// 
			// lblDecodeSpeedHelp
			// 
			this.lblDecodeSpeedHelp.AutoSize = true;
			this.lblDecodeSpeedHelp.Location = new System.Drawing.Point(144, 49);
			this.lblDecodeSpeedHelp.Name = "lblDecodeSpeedHelp";
			this.lblDecodeSpeedHelp.Size = new System.Drawing.Size(348, 39);
			this.lblDecodeSpeedHelp.TabIndex = 3;
			this.lblDecodeSpeedHelp.Text = "Sets the decoding speed tier for the provided options\r\nMinimum is 0  (slowest to " +
    "decode, best quality/density),\r\nand maximum is 4 (fastest to decode, at the cost" +
    " of some quality/density)";
			// 
			// boxCompressionMode
			// 
			this.boxCompressionMode.Controls.Add(this.optDrawing);
			this.boxCompressionMode.Controls.Add(this.optPhoto);
			this.boxCompressionMode.Controls.Add(this.optAuto);
			this.boxCompressionMode.Controls.Add(this.optLossless);
			this.boxCompressionMode.Location = new System.Drawing.Point(12, 92);
			this.boxCompressionMode.Name = "boxCompressionMode";
			this.boxCompressionMode.Size = new System.Drawing.Size(121, 94);
			this.boxCompressionMode.TabIndex = 6;
			this.boxCompressionMode.TabStop = false;
			this.boxCompressionMode.Text = "Compression Mode";
			// 
			// optDrawing
			// 
			this.optDrawing.AutoSize = true;
			this.optDrawing.Location = new System.Drawing.Point(5, 70);
			this.optDrawing.Name = "optDrawing";
			this.optDrawing.Size = new System.Drawing.Size(64, 17);
			this.optDrawing.TabIndex = 3;
			this.optDrawing.TabStop = true;
			this.optDrawing.Text = "&Drawing";
			this.optDrawing.UseVisualStyleBackColor = true;
			// 
			// optPhoto
			// 
			this.optPhoto.AutoSize = true;
			this.optPhoto.Location = new System.Drawing.Point(5, 52);
			this.optPhoto.Name = "optPhoto";
			this.optPhoto.Size = new System.Drawing.Size(53, 17);
			this.optPhoto.TabIndex = 2;
			this.optPhoto.TabStop = true;
			this.optPhoto.Text = "&Photo";
			this.optPhoto.UseVisualStyleBackColor = true;
			// 
			// optAuto
			// 
			this.optAuto.AutoSize = true;
			this.optAuto.Location = new System.Drawing.Point(5, 34);
			this.optAuto.Name = "optAuto";
			this.optAuto.Size = new System.Drawing.Size(47, 17);
			this.optAuto.TabIndex = 1;
			this.optAuto.TabStop = true;
			this.optAuto.Text = "&Auto";
			this.optAuto.UseVisualStyleBackColor = true;
			// 
			// optLossless
			// 
			this.optLossless.AutoSize = true;
			this.optLossless.Location = new System.Drawing.Point(5, 16);
			this.optLossless.Name = "optLossless";
			this.optLossless.Size = new System.Drawing.Size(65, 17);
			this.optLossless.TabIndex = 0;
			this.optLossless.TabStop = true;
			this.optLossless.Text = "&Lossless";
			this.optLossless.UseVisualStyleBackColor = true;
			// 
			// txtQuality
			// 
			this.txtQuality.Location = new System.Drawing.Point(12, 205);
			this.txtQuality.Name = "txtQuality";
			this.txtQuality.Size = new System.Drawing.Size(100, 20);
			this.txtQuality.TabIndex = 9;
			// 
			// lblQuality
			// 
			this.lblQuality.AutoSize = true;
			this.lblQuality.Location = new System.Drawing.Point(9, 189);
			this.lblQuality.Name = "lblQuality";
			this.lblQuality.Size = new System.Drawing.Size(90, 13);
			this.lblQuality.TabIndex = 8;
			this.lblQuality.Text = "&Quality (Distance)";
			// 
			// lblQualityHelp
			// 
			this.lblQualityHelp.AutoSize = true;
			this.lblQualityHelp.Location = new System.Drawing.Point(144, 161);
			this.lblQualityHelp.Name = "lblQualityHelp";
			this.lblQualityHelp.Size = new System.Drawing.Size(289, 65);
			this.lblQualityHelp.TabIndex = 7;
			this.lblQualityHelp.Text = resources.GetString("lblQualityHelp.Text");
			// 
			// chkPreserveColor
			// 
			this.chkPreserveColor.AutoSize = true;
			this.chkPreserveColor.Location = new System.Drawing.Point(12, 235);
			this.chkPreserveColor.Name = "chkPreserveColor";
			this.chkPreserveColor.Size = new System.Drawing.Size(212, 17);
			this.chkPreserveColor.TabIndex = 13;
			this.chkPreserveColor.Text = "Preserve color of fully &transparent pixels";
			this.chkPreserveColor.UseVisualStyleBackColor = true;
			// 
			// cmdOK
			// 
			this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.cmdOK.Location = new System.Drawing.Point(338, 257);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(75, 23);
			this.cmdOK.TabIndex = 14;
			this.cmdOK.Text = "OK";
			this.cmdOK.UseVisualStyleBackColor = true;
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// cmdCancel
			// 
			this.cmdCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdCancel.Location = new System.Drawing.Point(419, 257);
			this.cmdCancel.Name = "cmdCancel";
			this.cmdCancel.Size = new System.Drawing.Size(75, 23);
			this.cmdCancel.TabIndex = 15;
			this.cmdCancel.Text = "Cancel";
			this.cmdCancel.UseVisualStyleBackColor = true;
			// 
			// EncoderOptionsForm2
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cmdCancel;
			this.ClientSize = new System.Drawing.Size(506, 292);
			this.Controls.Add(this.cmdCancel);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.chkPreserveColor);
			this.Controls.Add(this.lblQualityHelp);
			this.Controls.Add(this.lblQuality);
			this.Controls.Add(this.txtQuality);
			this.Controls.Add(this.boxCompressionMode);
			this.Controls.Add(this.lblDecodeSpeedHelp);
			this.Controls.Add(this.lblEncodeSpeedHelp);
			this.Controls.Add(this.lblDecodingSpeed);
			this.Controls.Add(this.cboDecodingSpeed);
			this.Controls.Add(this.lblEncodingSpeed);
			this.Controls.Add(this.cboEncodingSpeed);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.Name = "EncoderOptionsForm2";
			this.Text = "Encoder Options";
			this.Load += new System.EventHandler(this.EncoderOptionsForm2_Load);
			this.boxCompressionMode.ResumeLayout(false);
			this.boxCompressionMode.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private System.Windows.Forms.Label lblEncodingSpeed;
		private System.Windows.Forms.ComboBox cboEncodingSpeed;
		private System.Windows.Forms.Label lblDecodingSpeed;
		private System.Windows.Forms.ComboBox cboDecodingSpeed;
		private System.Windows.Forms.Label lblEncodeSpeedHelp;
		private System.Windows.Forms.Label lblDecodeSpeedHelp;
		private System.Windows.Forms.GroupBox boxCompressionMode;
		private System.Windows.Forms.RadioButton optDrawing;
		private System.Windows.Forms.RadioButton optPhoto;
		private System.Windows.Forms.RadioButton optAuto;
		private System.Windows.Forms.RadioButton optLossless;
		private System.Windows.Forms.TextBox txtQuality;
		private System.Windows.Forms.Label lblQuality;
		private System.Windows.Forms.Label lblQualityHelp;
		private System.Windows.Forms.CheckBox chkPreserveColor;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.Button cmdCancel;
	}
}