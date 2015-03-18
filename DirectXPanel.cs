using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;

using Resources = DirectX_Base.Properties.Resources;



namespace DirectX_Base
{
	public partial class DirectXPanel : Panel
	{
		DirectX mDirectX = null;



		public DirectXPanel()
		{
			InitializeComponent();
		}



		public void Init()
		{
			DirectX.RenderTargetParameters parms = new DirectX.RenderTargetParameters(Handle, Width, Height);

			mDirectX = new DirectX(parms);

			// this is a good place to set up the initial state of verteces

			Program.Render();
		}



		public void Draw()
		{
			mDirectX.Clear();
			mDirectX.UpdateView();

			// here, call Draw on any vertex classes

			mDirectX.Present();
		}



		protected override void OnPaint(PaintEventArgs inArgs)
		{
			// disable Windows drawing so DX has the floor
		}



		protected override void OnResize(EventArgs inArgs)
		{
			if(mDirectX != null)
				mDirectX.Resize(Width, Height);

			base.OnResize(inArgs);
		}



		public void CleanUp()
		{
			mDirectX.Dispose();

			// here, call Dispose() on any vertex classes
		}
	}
}
