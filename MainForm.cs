using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX.Windows;



namespace DirectX_Base
{
	public partial class MainForm : RenderForm
	{
		public MainForm()
		{
			InitializeComponent();
		}



		public void Draw()
		{
			mDXPanel.Draw();
		}



		protected override void OnLoad(EventArgs inArgs)
		{
			base.OnLoad(inArgs);

			mDXPanel.Init();
		}



		protected override void OnClosing(CancelEventArgs inArgs)
		{
			mDXPanel.CleanUp();
			DirectX.FinalDispose();
			base.OnClosing(inArgs);
		}
	}
}
