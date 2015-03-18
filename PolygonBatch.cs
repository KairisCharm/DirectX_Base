using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D11;

using Buffer = SlimDX.Direct3D11.Buffer;



namespace DirectX_Base
{
	class PolygonBatch : VertexBatch
	{
		private List<DirectX.Vertex> mVertList = new List<DirectX.Vertex>();

		public int mpCount { get { if (mBuffer == null) return 0; return mBuffer.Description.SizeInBytes; } }



		public void AddTriangle(PointF inPoint1, PointF inPoint2, PointF inPoint3, Color4 inColor1, Color4 inColor2, Color4 inColor3)
		{
			PointF[] points = new PointF[3];
			points[0] = inPoint1;
			points[1] = inPoint2;
			points[2] = inPoint3;

			Color4[] colors = new Color4[4];
			colors[0] = inColor1;
			colors[1] = inColor2;
			colors[2] = inColor3;

			ClockwiseCheck(ref points, ref colors);

			mVertList.Add(new DirectX.Vertex() { mPosition = new Vector2((float)points[0].X, (float)points[0].Y), mColor = colors[0] });
			mVertList.Add(new DirectX.Vertex() { mPosition = new Vector2((float)points[1].X, (float)points[1].Y), mColor = colors[1] });
			mVertList.Add(new DirectX.Vertex() { mPosition = new Vector2((float)points[2].X, (float)points[2].Y), mColor = colors[2] });
		}



		public void AddTriangle(PointF inPoint1, PointF inPoint2, PointF inPoint3, Color4 inColor)
		{
			PointF[] points = new PointF[3];
			points[0] = inPoint1;
			points[1] = inPoint2;
			points[2] = inPoint3;

			ClockwiseCheck(ref points);

			mVertList.Add(new DirectX.Vertex() { mPosition = new Vector2((float)points[0].X, (float)points[0].Y), mColor = inColor });
			mVertList.Add(new DirectX.Vertex() { mPosition = new Vector2((float)points[1].X, (float)points[1].Y), mColor = inColor });
			mVertList.Add(new DirectX.Vertex() { mPosition = new Vector2((float)points[2].X, (float)points[2].Y), mColor = inColor });
		}



		public void AddRect(PointF inPoint1, PointF inPoint2, PointF inPoint3, PointF inPoint4, Color4 inColor)
		{
			AddTriangle(inPoint1, inPoint2, inPoint3, inColor);
			AddTriangle(inPoint3, inPoint4, inPoint1, inColor);
		}



		public void AddRect(PointF inPoint1, PointF inPoint2, PointF inPoint3, PointF inPoint4, Color4 inColor1, Color4 inColor2, Color4 inColor3, Color4 inColor4)
		{
			AddTriangle(inPoint1, inPoint2, inPoint3, inColor1, inColor2, inColor3);
			AddTriangle(inPoint3, inPoint4, inPoint1, inColor3, inColor4, inColor1);
		}



		public override void Finish()
		{
			Finish(mVertList);

			mVertList.Clear();
			mVertList = new List<DirectX.Vertex>();
		}



		public override void Draw(ref DirectX inDirectX)
		{
			if (mBuffer == null)
				return;

			inDirectX.RenderPolygons(ref mVerteces);
		}



		public void Clear()
		{
			mVertList.Clear();
			mVertList = new List<DirectX.Vertex>();

			if (mBuffer != null && !mBuffer.Disposed && mBuffer.Description.SizeInBytes > 0)
				mBuffer.Dispose();
		}



		private static void ClockwiseCheck(ref PointF[] inPoints)
		{
			Color4[] colors = null;
			ClockwiseCheck(ref inPoints, ref colors);
		}



		private static void ClockwiseCheck(ref PointF[] inPoints, ref Color4[] inColors)
		{
			if (ClockwiseTest(inPoints))
			{
				PointF buf = inPoints[0];
				inPoints[0] = inPoints[1];
				inPoints[1] = buf;

				if (inColors != null)
				{
					Color4 colBuf = inColors[0];
					inColors[0] = inColors[1];
					inColors[1] = colBuf;
				}
			}
		}



		public static bool ClockwiseTest(PointF[] inPoints)
		{
			return ((inPoints[1].X - inPoints[0].X) * (inPoints[1].Y + inPoints[0].Y) +
												(inPoints[2].X - inPoints[1].X) * (inPoints[2].Y + inPoints[1].Y) +
												(inPoints[0].X - inPoints[2].X) * (inPoints[0].Y + inPoints[2].Y)) < 0;
		}
	}
}
