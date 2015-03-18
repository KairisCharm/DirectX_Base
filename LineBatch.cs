using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using SlimDX;
using SlimDX.Direct3D11;

using Buffer = SlimDX.Direct3D11.Buffer;



namespace DirectX_Base
{
	class LineBatch : VertexBatch
	{
		private List<DirectX.Vertex> mVertList = new List<DirectX.Vertex>();



		public void AddLine(PointF inPoint1, PointF inPoint2, Color4 inColor)
		{
			AddVertex(inPoint1, inColor);
			AddVertex(inPoint2, inColor);
		}



		public void AddVertex(PointF inPoint, Color4 inColor)
		{
			mVertList.Add(new DirectX.Vertex() { mPosition = new Vector2(inPoint.X, inPoint.Y), mColor = inColor });
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

			inDirectX.RenderLines(ref mVerteces);
		}



		public void Clear()
		{
			mVertList.Clear();
			mVertList = new List<DirectX.Vertex>();

			if (mBuffer != null && !mBuffer.Disposed && mBuffer.Description != null && mBuffer.Description.SizeInBytes > 0)
				mBuffer.Dispose();
		}
	}
}
