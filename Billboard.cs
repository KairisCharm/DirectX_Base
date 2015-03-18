using System.Collections.Generic;
using System.Drawing;
using SlimDX;
using SlimDX.Direct3D11;
using DirectX_Base.Properties;

using Resources = DirectX_Base.Properties.Resources;



namespace DirectX_Base
{
	class Billboard : VertexBatch
	{
		Texture2D mTexture;
		ShaderResourceView mResourceView;
		List<DirectX.TextureVertex> mLocList;
		


		public void SetTexture(Bitmap inImage)
		{
			mTexture = DirectX.BitmapToTexture(inImage);
			mResourceView = new ShaderResourceView(DirectX.spDevice, mTexture);
		}



		public void SetPosition(PointF inTopLeft, PointF inBottomRight)
		{
			if (mLocList != null)
				mLocList.Clear();
			mLocList = new List<DirectX.TextureVertex>();

			mLocList.Add(new DirectX.TextureVertex() { mPosition = new Vector2(inTopLeft.X, inTopLeft.Y), mTexCoord = new Vector2(0, 0) });
			mLocList.Add(new DirectX.TextureVertex() { mPosition = new Vector2(inTopLeft.X, inBottomRight.Y), mTexCoord = new Vector2(0, 1) });
			mLocList.Add(new DirectX.TextureVertex() { mPosition = new Vector2(inBottomRight.X, inTopLeft.Y), mTexCoord = new Vector2(1, 0) });
			mLocList.Add(new DirectX.TextureVertex() { mPosition = new Vector2(inBottomRight.X, inBottomRight.Y), mTexCoord = new Vector2(1, 1) });
		}



		public override void Draw(ref DirectX inDirectX)
		{
			if (mBuffer == null)
				return;

			inDirectX.RenderTexture(ref mVerteces, ref mResourceView, PrimitiveTopology.TriangleStrip);
		}



		public override void Finish()
		{
			Finish(mLocList);

			mLocList.Clear();
			mLocList = new List<DirectX.TextureVertex>();
		}



		public override void Dispose()
		{
			mTexture.Dispose();
			mResourceView.Dispose();
			base.Dispose();
		}
	}
}
