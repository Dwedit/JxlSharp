using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
//using System.Linq;
using System.Text;
using System.Windows.Forms;

using JxlSharp;

namespace JxlExample
{
	public partial class Form1 : Form
	{
		string loadedFileName = "";

		public Form1()
		{
			InitializeComponent();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			OpenFile();
		}

		string PromptForFileName(bool saving)
		{
			using (FileDialog fileDialog = saving ? new SaveFileDialog() : (FileDialog)new OpenFileDialog())
			{
				fileDialog.Filter = "Image Files|*.jxl;*.jpg;*.png;*.bmp;*.gif;*.tif|All Files (*.*)|*.*";
				fileDialog.FileName = loadedFileName;
				if (fileDialog.ShowDialog() == DialogResult.OK)
				{
					return fileDialog.FileName;
				}
				else
				{
					return "";
				}
			}
		}

		private void OpenFile()
		{
			string fileName = PromptForFileName(false);
			if (!String.IsNullOrEmpty(fileName) && File.Exists(fileName))
			{
				OpenFile(fileName);
			}
		}

		private void OpenFile(string fileName)
		{
			Bitmap bitmap = LoadImage(fileName);
			if (bitmap == null)
			{
				MessageBox.Show("Failed to load image file.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			this.pictureBox1.Image?.Dispose();
			this.pictureBox1.Image = bitmap;
			this.loadedFileName = fileName;
		}

		/// <summary>
		/// Loads a standard bitmap, or a JXL file.
		/// </summary>
		/// <param name="fileName">Name of the file</param>
		/// <returns>Returns a bitmap, or null on failure</returns>
		private Bitmap LoadImage(string fileName)
		{
			Bitmap bitmap = null;
			byte[] fileData = null;
			try
			{
				fileData = File.ReadAllBytes(fileName);
			}
			catch
			{
				return null;
			}
			var memoryStream = new MemoryStream(fileData);
			string ext = Path.GetExtension(fileName).ToLowerInvariant();
			if (ext == ".jxl" && bitmap == null)
			{
				if (Debugger.IsAttached)
				{
					bitmap = JXL.LoadImage(fileData);
				}
				else
				{
					try
					{
						bitmap = JXL.LoadImage(fileData);
					}
					catch
					{

					}
				}
			}
			if (bitmap == null)
			{
				try
				{
					bitmap = (Bitmap)Bitmap.FromStream(memoryStream);
				}
				catch
				{

				}
				if (bitmap != null)
				{
					//prevent the source file from staying locked
					Bitmap oldbitmap = bitmap;
					bitmap = (Bitmap)bitmap.Clone();
					oldbitmap.Dispose();
				}
			}
			if (bitmap == null)
			{
				try
				{
					bitmap = JXL.LoadImage(fileData);
				}
				catch
				{

				}
			}
			return bitmap;
		}

		private void saveToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileAs();
		}

		private void SaveFileAs()
		{
			if (String.IsNullOrEmpty(this.loadedFileName))
			{
				return;
			}
			string fileName = PromptForFileName(true);
			if (!string.IsNullOrEmpty(fileName))
			{
				SaveFile(fileName);
			}
		}

		private void SaveFile(string fileName)
		{
			if (this.pictureBox1.Image != null)
			{
				Bitmap bitmap = (Bitmap)this.pictureBox1.Image;
				string ext = Path.GetExtension(fileName).ToLowerInvariant();
				if (ext == ".jxl")
				{
					MessageBox.Show("Writing JXL files is not yet implemented.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
					return;
					//byte[] jxlData = JXL.SaveImage(bitmap);
					//if (jxlData != null)
					//{
					//	File.WriteAllBytes(fileName, jxlData);
					//}
				}
				string sourceExt = Path.GetExtension(loadedFileName);
				if (sourceExt == ".jxl" && ext == ".jpg")
				{
					byte[] jxlBytes = File.ReadAllBytes(loadedFileName);
					byte[] jpegBytes = JXL.TranscodeToJpeg(jxlBytes);
					if (jpegBytes != null)
					{
						File.WriteAllBytes(fileName, jpegBytes);
						this.loadedFileName = fileName;
						MessageBox.Show("JPEG file saved using JPEG reconstruction data.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
						return;
					}
					else
					{
						MessageBox.Show("This JXL file does not contain JPEG reconstruction data.", "Note", MessageBoxButtons.OK, MessageBoxIcon.Information);
					}
				}
				bitmap.Save(fileName);
			}
		}

		private void copyToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CopyToClipboard();
		}

		private void CopyToClipboard()
		{
			Bitmap bitmap = (Bitmap)pictureBox1.Image;
			if (bitmap != null)
			{
				CopyToClipboard(bitmap);
			}
		}

		private void CopyToClipboard(Bitmap bitmap)
		{
			if (bitmap != null)
			{
				try
				{
					Clipboard.SetImage(bitmap);
				}
				catch
				{
					MessageBox.Show("Failed to set the contents of the clipboard.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
