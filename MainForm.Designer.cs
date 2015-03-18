namespace DirectX_Base
{
	partial class MainForm
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

			this.mDXPanel = new DirectXPanel();

			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Text = "DirectX_Base";

			this.mDXPanel.Location = new System.Drawing.Point(10, 10);
			this.mDXPanel.Size = new System.Drawing.Size(500, 500);

			this.Controls.Add(this.mDXPanel);
		}

		#endregion

		DirectXPanel mDXPanel;
	}
}

