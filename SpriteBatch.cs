using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using SlimDX;
using SlimDX.Direct3D11;
using DirectX_Base.Properties;

using Resources = DirectX_Base.Properties.Resources;



namespace DirectX_Base
{
	class SpriteBatch : VertexBatch
	{
		private static string[][] sTextureInfo = null;

		public List<DirectX.SpriteVertex> mLocList = new List<DirectX.SpriteVertex>();
		public Dictionary<PointF, string> mLocationStrings = new Dictionary<PointF, string>();
		private static float sAlphabetHeight = 0;



		public SpriteBatch()
		{
			if (sTextureInfo == null)
			{
				string[] allTextures = Resources.TextureIndeces.Split('\n');

				sTextureInfo = new string[allTextures.Length][];

				for (int i = 0; i < allTextures.Length; i++)
					sTextureInfo[i] = allTextures[i].Split('\t');
			}

			if (sAlphabetHeight == 0)
				sAlphabetHeight = Resources.Alphabet.Height;
		}



		public void ClearSprites()
		{
			mLocList.Clear();
			mLocList = new List<DirectX.SpriteVertex>();

			Dispose();
		}



		public override void Finish()
		{
			if (mLocList.Count == 0)
				return;

			Finish(mLocList);

			mLocList.Clear();
			mLocList = new List<DirectX.SpriteVertex>();
		}



		public override void Draw(ref DirectX inDirectX)
		{
			if (mBuffer == null)
				return;

			inDirectX.RenderSprites(ref mVerteces);
		}



		public void ActivateSprite(string inTextureId, PointF inLocation)
		{
			ActivateSprite(inTextureId, inLocation, 1.0f, 0);
		}



		public void ActivateSprite(string inTextureId, PointF inLocation, float inScale, float inRotation)
		{
			float start, width;
			int index;
			FindSpriteInfo(inTextureId, out start, out width, out index);

			ActivateSprite(index, inScale, start, width, inRotation, 0, inLocation);
		}



		public void ActivateSprite(int inTextureId, float inSizePercent, float inTextureX, float inWidth, float inRotation, float inXOffset, PointF inLocation)
		{
			mLocList.Add(new DirectX.SpriteVertex() { mPosition = new Vector2(inLocation.X, inLocation.Y), mSizePercent = inSizePercent, mImageWidth = inWidth, mTextureId = inTextureId, mTextureStart = inTextureX, mRotation = (float)(Convert.kDegreesToRadians * inRotation), mXOffset = inXOffset });
		}



		public void ActivateTextSprite(string inString, float inSizePercent, PointF inLocation)
		{
			float totalWidth = 0;

			float offsetOffset = 0;

			float[] starts = new float[inString.Length];
			float[] widths = new float[inString.Length];
			int[] indeces = new int[inString.Length];

			int index = 0;
			foreach (char c in inString)
			{
				FindSpriteInfo(c + "Alph", out starts[index], out widths[index], out indeces[index]);

				totalWidth += widths[index] * inSizePercent;
				index++;
			}

			index = 0;
			foreach (char c in inString)
			{
				float width = widths[index] * inSizePercent;

				float offset = ((width / 2) - (totalWidth / 2)) + offsetOffset;
				offsetOffset += width;

				ActivateSprite(indeces[index], inSizePercent, starts[index], width / inSizePercent, 0, offset, inLocation);

				index++;
			}

			mLocationStrings[inLocation] = inString;
		}



		private static void FindSpriteInfo(string inId, out float outStart, out float outWidth, out int outIndex)
		{
			IEnumerator<string[]> infoEnum = (from string[] thisInfo in sTextureInfo where thisInfo.Contains(inId) select thisInfo).GetEnumerator();

			infoEnum.MoveNext();

			outStart = float.Parse(infoEnum.Current[1]);
			outWidth = float.Parse(infoEnum.Current[2]);
			outIndex = int.Parse(infoEnum.Current[3]);

			infoEnum.Dispose();
		}
	}
}
