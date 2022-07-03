using JxlSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace JxlExample
{
	public partial class EncoderOptionsForm2 : Form
	{
		public EncoderOptions EncoderOptions = new EncoderOptions();
		public EncoderOptionsForm2()
		{
			InitializeComponent();
		}
		public EncoderOptionsForm2(EncoderOptions encoderOptions) : this()
		{
			this.EncoderOptions = encoderOptions;
			ReadOptions();
		}

		private void EncoderOptionsForm2_Load(object sender, EventArgs e)
		{
			ReadOptions();
		}

		private void cmdOK_Click(object sender, EventArgs e)
		{
			SetOptions();
		}

		private void SetOptions()
		{
			int effort = cboEncodingSpeed.SelectedIndex + 1;
			int decodeSpeed = cboDecodingSpeed.SelectedIndex;
			JxlLossyMode lossyMode = JxlLossyMode.Lossless;
			if (optLossless.Checked) lossyMode = JxlLossyMode.Lossless;
			if (optAuto.Checked) lossyMode = JxlLossyMode.Default;
			if (optPhoto.Checked) lossyMode = JxlLossyMode.Photo;
			if (optDrawing.Checked) lossyMode = JxlLossyMode.Drawing;
			float quality;
			if (!float.TryParse(txtQuality.Text, out quality))
			{
				quality = 1;
			}
			int keepInvisible = chkPreserveColor.Checked ? 1: 0;

			this.EncoderOptions.LossyMode = lossyMode;
			this.EncoderOptions.Quality = quality;
			this.EncoderOptions.Settings[JxlEncoderFrameSettingId.Effort] = effort;
			this.EncoderOptions.Settings[JxlEncoderFrameSettingId.DecodingSpeed] = decodeSpeed;
			this.EncoderOptions.Settings[JxlEncoderFrameSettingId.KeepInvisible] = keepInvisible;
		}

		private void ReadOptions()
		{
			int effort = 7;
			if (this.EncoderOptions.Settings.TryGetValue(JxlEncoderFrameSettingId.Effort, out effort) && effort != -1)
			{

			}
			else
			{
				effort = 7;
			}

			int decodingSpeed = 0;
			if (this.EncoderOptions.Settings.TryGetValue(JxlEncoderFrameSettingId.DecodingSpeed, out decodingSpeed) && decodingSpeed != -1)
			{

			}
			else
			{
				decodingSpeed = 0;
			}

			int keepInvisible = 0;
			if (this.EncoderOptions.Settings.TryGetValue(JxlEncoderFrameSettingId.KeepInvisible, out keepInvisible) && keepInvisible != -1)
			{

			}
			else
			{
				keepInvisible = 0;
			}

			float quality = this.EncoderOptions.Quality;
			if (quality < 0) quality = 0;
			if (quality > 15) quality = 15;

			JxlLossyMode lossyMode = this.EncoderOptions.LossyMode;

			if (effort >= 1 && effort <= 9)
			{
				cboEncodingSpeed.SelectedIndex = effort - 1;
			}
			else
			{
				cboEncodingSpeed.SelectedIndex = 7 - 1;
			}
			if (decodingSpeed >= 0 && decodingSpeed <= 4)
			{
				cboDecodingSpeed.SelectedIndex = decodingSpeed;
			}
			optLossless.Checked = false;
			optAuto.Checked = false;
			optPhoto.Checked = false;
			optDrawing.Checked = false;

			switch (lossyMode)
			{
				case JxlLossyMode.Lossless:
					optLossless.Checked = true;
					break;
				case JxlLossyMode.Default:
					optAuto.Checked = true;
					break;
				case JxlLossyMode.Photo:
					optPhoto.Checked = true;
					break;
				case JxlLossyMode.Drawing:
					optDrawing.Checked = true;
					break;
			}

			txtQuality.Text = Math.Round(quality, 4).ToString();
			chkPreserveColor.Checked = keepInvisible == 1;
		}
	}

	public class EncoderOptions
	{
		public Dictionary<JxlSharp.JxlEncoderFrameSettingId, int> Settings = new Dictionary<JxlSharp.JxlEncoderFrameSettingId, int>();
		public JxlLossyMode LossyMode;
		public float Quality;
		public EncoderOptions()
		{
			this.LossyMode = JxlLossyMode.Default;
			this.Settings[JxlEncoderFrameSettingId.Effort] = 7;
			this.Settings[JxlEncoderFrameSettingId.DecodingSpeed] = 0;
			this.Settings[JxlEncoderFrameSettingId.KeepInvisible] = 1;
			this.Quality = 1.0f;
		}
	}
}
