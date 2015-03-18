using System.Collections.Generic;
using System.Runtime.InteropServices;
using SlimDX;
using SlimDX.Direct3D11;



namespace DirectX_Base
{
	abstract class VertexBatch
	{
		protected Buffer mBuffer;
		protected VertexBufferBinding mVerteces;



		public virtual void Dispose()
		{
			ClearBuffer();
		}



		private void ClearBuffer()
		{
			if (mBuffer != null)
				mBuffer.Dispose();
		}



		public abstract void Finish();



		protected void Finish<T>(List<T> inLocList)
		{
			DataStream stream = new DataStream(inLocList.ToArray(), true, true);
			stream.Position = 0;

			BufferDescription description = new BufferDescription()
			{
				SizeInBytes = (int)stream.Length,
				Usage = ResourceUsage.Dynamic,
				BindFlags = BindFlags.VertexBuffer,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = Marshal.SizeOf(typeof(T))
			};

			ClearBuffer();

			mBuffer = new Buffer(DirectX.spDevice, stream, description);
			mVerteces = new VertexBufferBinding(mBuffer, description.StructureByteStride, 0);

			stream.Close();
		}



		public abstract void Draw(ref DirectX inDirectX);
	}
}
