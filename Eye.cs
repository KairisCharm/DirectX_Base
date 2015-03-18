using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SlimDX;



namespace DirectX_Base
{
	class Eye
	{
		private const float kFov = 145;

		private float mAspectRatio;

		public float mpWidth { get; set; }
		public float mpHeight { get; set; }

		public Matrix mpView { get; private set; }
		public Matrix mpProjection { get; private set; }
		protected PointF mLocation = new PointF();

		private Timer mSlowPanTimer = new Timer();



		protected float mpAspectRatio
		{
			get
			{
				return mAspectRatio;
			}

			set
			{
				mAspectRatio = value;

				MakePerspective();
				Program.Render();
			}
		}



		public PointF mpLocation
		{
			get
			{
				return mLocation;
			}
			protected set
			{
				mLocation = value;

				Center();
				Program.Render();
			}
		}



		protected void Center()
		{
			mpView = Matrix.LookAtLH(new Vector3(mpLocation.X, mpLocation.Y, -1.0f), new Vector3(mpLocation.X, mpLocation.Y, 0.0f), Vector3.UnitY);
		}



		protected void MakePerspective()
		{
			mpProjection = Matrix.PerspectiveFovLH((float)Math.PI / 180.0f * kFov, mpAspectRatio, 0.001f, 1.0f);
		}



		public void Initialize(float inX, float inY, float inWidth, float inHeight)
		{
			mpLocation = new PointF(inY, inX);

			Center();
			SetAspectRatio(inWidth, inHeight);
		}



		public void SetAspectRatio(float inWidth, float inHeight)
		{
			mpAspectRatio = inWidth / inHeight;
		}



		public void Pan(int inX1, int inY1, int inX2, int inY2, int inWidth, int inHeight)
		{
			Vector3 unprojectedPoint1 = Vector3.Unproject(new Vector3(inX1, inY1, 0), 0, 0, inWidth, inHeight, -100, 0, mpView * mpProjection);
			Vector3 unprojectedPoint2 = Vector3.Unproject(new Vector3(inX2, inY2, 0), 0, 0, inWidth, inHeight, -100, 0, mpView * mpProjection);
			Pan(unprojectedPoint1, unprojectedPoint2, inWidth, inHeight);
		}



		public void Pan(Vector3 inPosition1, Vector3 inPosition2, int inWidth, int inHeight)
		{
			mpLocation = new PointF(mpLocation.X + (inPosition1.X - inPosition2.X), mpLocation.Y + (inPosition1.Y - inPosition2.Y));
		}
	}
}
