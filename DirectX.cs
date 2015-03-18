using System;
using System.Drawing;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using SlimDX;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using DirectX_Base.Properties;

using Device = SlimDX.Direct3D11.Device;
using Buffer = SlimDX.Direct3D11.Buffer;
using Resource = SlimDX.Direct3D11.Resource;
using Resources = DirectX_Base.Properties.Resources;
using MapFlags = SlimDX.Direct3D11.MapFlags;



namespace DirectX_Base
{
	class DirectX
	{
		private string mpShaderCode { get; set; }

		protected static Device sDevice = null;
		protected SwapChain mSwapChain = null;
		protected RenderTargetView mRenderTarget = null;

		protected static Texture2D sTextTexture;
		protected static Texture2D sSpriteTexture;
		protected static ShaderResourceView[] sTextureArray;

		public static Device spDevice { get { return sDevice; } private set { sDevice = value; } }
		protected static VertexShader sLinePolyVertShader;
		protected static VertexShader sTextureVertShader;
		protected static VertexShader sSpriteVertShader;
		protected static GeometryShader sSpriteGeoShader;
		protected static PixelShader sLinePolyPixShader;
		protected static PixelShader sTexturePixShader;
		protected static PixelShader sSpritePixShader;
		protected static InputLayout sLinePolyLayout;
		protected static InputLayout sTextureLayout;
		protected static InputLayout sSpriteLayout;

		protected Eye mEye;



		public Eye mpEye
		{
			get
			{
				return mEye;
			}
		}



		public static DeviceContext spContext
		{
			get
			{
				if (spDevice == null)
					return null;
				return spDevice.ImmediateContext;
			}
		}

		protected static Buffer spConstantBuffer { get; set; }



		public DirectX(RenderTargetParameters inParms)
		{
			mEye = new Eye();
			mEye.Initialize(0, 0, inParms.mpWidth, inParms.mpHeight);

			mpShaderCode = Encoding.Default.GetString(Resources.ShaderCode);

			CreateDevice(inParms.mpHandle);

			using (Resource resource = Resource.FromSwapChain<Texture2D>(mSwapChain, 0))
				mRenderTarget = new RenderTargetView(spDevice, resource);

			SetAspectRatio(inParms.mpWidth, inParms.mpHeight);

			BufferDescription matrixDescription = new BufferDescription()
			{
				SizeInBytes = Marshal.SizeOf(Matrix.Identity) * 2 + Marshal.SizeOf(new Vector3()) + sizeof(int),
				Usage = ResourceUsage.Dynamic,
				BindFlags = BindFlags.ConstantBuffer,
				CpuAccessFlags = CpuAccessFlags.Write,
				OptionFlags = ResourceOptionFlags.None,
				StructureByteStride = 0
			};

			if (spConstantBuffer == null)
				spConstantBuffer = new Buffer(spDevice, matrixDescription);
		}



		private void SetAspectRatio(int inWidth, int inHeight)
		{
			mpEye.mpWidth = inWidth;
			mpEye.mpHeight = inHeight;

			UpdateView(mEye);
		}



		protected void CreateDevice(IntPtr inHandle)
		{
			if (spDevice == null)
				spDevice = new Device(DriverType.Hardware, DeviceCreationFlags.None);

			if (sTextTexture == null)
			{
				sTextTexture = BitmapToTexture(Resources.Alphabet);
				sSpriteTexture = BitmapToTexture(Resources.Sprites);
				
				sTextureArray = new ShaderResourceView[3];
				sTextureArray[0] = new ShaderResourceView(spDevice, sTextTexture);
				sTextureArray[1] = new ShaderResourceView(spDevice, sSpriteTexture);
				sTextureArray[2] = null;

				using (ShaderBytecode bytecode = ShaderBytecode.Compile(mpShaderCode, "VS_Line_Polygon", "vs_4_0", ShaderFlags.EnableStrictness, EffectFlags.None))
				{
					InputElement[] elements = new[] { new InputElement("POSITION", 0, Format.R32G32_Float, 0),
																	new InputElement("COLOR", 0, Format.R32G32B32A32_Float, 0) };

					ShaderSignature inputSig = ShaderSignature.GetInputSignature(bytecode);
					sLinePolyVertShader = new VertexShader(spDevice, bytecode);
					sLinePolyLayout = new InputLayout(spDevice, inputSig, elements);

					inputSig.Dispose();
					bytecode.Dispose();
				}

				using (ShaderBytecode bytecode = ShaderBytecode.Compile(mpShaderCode, "PS_Line_Polygon", "ps_4_0", ShaderFlags.EnableStrictness, EffectFlags.None))
				{
					sLinePolyPixShader = new PixelShader(spDevice, bytecode);
					bytecode.Dispose();
				}

				using (ShaderBytecode bytecode = ShaderBytecode.Compile(mpShaderCode, "VS_Texture", "vs_4_0", ShaderFlags.EnableStrictness, EffectFlags.None))
				{
					InputElement[] elements = new[] { new InputElement("POSITION", 0, Format.R32G32_Float, 0), 
																		new InputElement("TEXCOORD", 0, Format.R32G32_Float, 0) };

					ShaderSignature inputSig = ShaderSignature.GetInputSignature(bytecode);
					sTextureVertShader = new VertexShader(spDevice, bytecode);
					sTextureLayout = new InputLayout(spDevice, inputSig, elements);

					inputSig.Dispose();
					bytecode.Dispose();
				}

				using (ShaderBytecode bytecode = ShaderBytecode.Compile(mpShaderCode, "PS_Texture", "ps_4_0", ShaderFlags.EnableStrictness, EffectFlags.None))
				{
					sTexturePixShader = new PixelShader(spDevice, bytecode);
					bytecode.Dispose();
				}

				using (ShaderBytecode bytecode = ShaderBytecode.Compile(mpShaderCode, "VS_Sprite", "vs_4_0", ShaderFlags.EnableStrictness, EffectFlags.None))
				{
					InputElement[] elements = new InputElement[] { new InputElement("POSITION", 0, Format.R32G32_Float, 0), 
																							new InputElement("SIZEPERCENT", 0, Format.R32_Float, 0),
																							new InputElement("IMAGEWIDTH", 0, Format.R32_Float, 0),
																							new InputElement("TEXID", 0, Format.R32_SInt, 0),
																							new InputElement("INDEX", 0, Format.R32_Float, 0),
																							new InputElement("ROTATION", 0, Format.R32_Float, 0),
																							new InputElement("XOFFSET", 0, Format.R32_Float, 0 )};

					ShaderSignature inputSig = ShaderSignature.GetInputSignature(bytecode);
					sSpriteVertShader = new VertexShader(spDevice, bytecode);
					sSpriteLayout = new InputLayout(spDevice, inputSig, elements);

					inputSig.Dispose();
					bytecode.Dispose();
				}

				using (ShaderBytecode bytecode = ShaderBytecode.Compile(mpShaderCode, "GS_Sprite", "gs_4_0", ShaderFlags.EnableStrictness, EffectFlags.None))
				{
					sSpriteGeoShader = new GeometryShader(spDevice, bytecode);
					bytecode.Dispose();
				}

				using (ShaderBytecode bytecode = ShaderBytecode.Compile(mpShaderCode, "PS_Sprite", "ps_4_0", ShaderFlags.EnableStrictness, EffectFlags.None))
				{
					sSpritePixShader = new PixelShader(spDevice, bytecode);
					bytecode.Dispose();
				}
			}

			int sampleCount = Device.MultisampleCountMaximum;

			while (spDevice.CheckMultisampleQualityLevels(Format.R8G8B8A8_UNorm, sampleCount) < 1)
				sampleCount--;

			RasterizerStateDescription desc = new RasterizerStateDescription()
			{
				CullMode = CullMode.None,
				DepthBias = 0,
				DepthBiasClamp = 0.0f,
				FillMode = FillMode.Solid,
				IsAntialiasedLineEnabled = true,
				IsDepthClipEnabled = false,
				IsFrontCounterclockwise = false,
				IsMultisampleEnabled = true,
				IsScissorEnabled = false,
				SlopeScaledDepthBias = 0.0f
			};

			spContext.Rasterizer.State = RasterizerState.FromDescription(spDevice, desc);

			SwapChainDescription chainDescription = new SwapChainDescription()
			{
				BufferCount = 2,
				Usage = Usage.RenderTargetOutput,
				OutputHandle = inHandle,
				IsWindowed = true,
				ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
				SampleDescription = new SampleDescription(sampleCount, 0),
				Flags = SwapChainFlags.AllowModeSwitch,
				SwapEffect = SwapEffect.Discard
			};

			mSwapChain = new SwapChain(spDevice.Factory, spDevice, chainDescription);
		}



		public void Clear()
		{
			spContext.ClearRenderTargetView(mRenderTarget, new Color4(0,0,0));
		}



		public void Present()
		{
			mSwapChain.Present(0, PresentFlags.None);
		}



		public void Resize(int inWidth, int inHeight)
		{
			if (mRenderTarget != null)
				mRenderTarget.Dispose();

			mSwapChain.ResizeBuffers(2, inWidth, inHeight, Format.R8G8B8A8_UNorm, SwapChainFlags.AllowModeSwitch);

			using (Resource resource = Resource.FromSwapChain<Texture2D>(mSwapChain, 0))
			{
				if (mRenderTarget != null)
					mRenderTarget.Dispose();

				mRenderTarget = new RenderTargetView(spDevice, resource);
			}

			SetAspectRatio(inWidth, inHeight);
		}



		public void UpdateView()
		{
			UpdateView(mEye);
		}



		// this is static because all RenderTargets share the same set of constants
		public static void UpdateView(Eye inEye)
		{
			if (spContext != null && spConstantBuffer != null)
			{
				DataBox matrix = spContext.MapSubresource(spConstantBuffer, MapMode.WriteDiscard, MapFlags.None);
				matrix.Data.Write(inEye.mpView);
				matrix.Data.Write(inEye.mpProjection);
				matrix.Data.Write(new Vector2(inEye.mpWidth, inEye.mpHeight));
				spContext.UnmapSubresource(spConstantBuffer, 0);
			}
		}



		public static Texture2D BitmapToTexture(Bitmap inImage)
		{
			MemoryStream stream = new MemoryStream();
			inImage.Save(stream, ImageFormat.Png);
			byte[] bytes = new byte[stream.Length];
			stream.Position = 0;
			stream.Read(bytes, 0, bytes.Length);

			return Texture2D.FromMemory(spDevice, bytes);
		}



		private void PrepareContext(ref InputLayout inLayout, ref VertexShader inVertShader, ref PixelShader inPixShader, ref VertexBufferBinding inShape, PrimitiveTopology inTopology)
		{
			Viewport viewport = new Viewport(0.0f, 0.0f, mpEye.mpWidth, mpEye.mpHeight);
			spContext.OutputMerger.SetTargets(mRenderTarget);

			spContext.Rasterizer.SetViewports(viewport);
			spContext.InputAssembler.InputLayout = inLayout;
			spContext.InputAssembler.SetVertexBuffers(0, inShape);
			spContext.InputAssembler.PrimitiveTopology = inTopology;

			spContext.VertexShader.Set(inVertShader);
			spContext.GeometryShader.Set(null);
			spContext.PixelShader.Set(inPixShader);

			spContext.VertexShader.SetConstantBuffer(spConstantBuffer, 0);
			spContext.GeometryShader.SetConstantBuffer(spConstantBuffer, 0);
			spContext.PixelShader.SetConstantBuffer(spConstantBuffer, 0);
		}



		public void PrepareContext(ref InputLayout inLayout, ref VertexShader inVertShader, ref GeometryShader inGeoShader, ref PixelShader inPixShader, ref VertexBufferBinding inShape, PrimitiveTopology inTopology)
		{
			PrepareContext(ref inLayout, ref inVertShader, ref inPixShader, ref inShape, inTopology);
			spContext.GeometryShader.Set(inGeoShader);
		}



		public void Render(ref VertexBufferBinding inVerteces, PrimitiveTopology inTopology, int inLimit)
		{
			PrepareContext(ref sLinePolyLayout, ref sLinePolyVertShader, ref sLinePolyPixShader, ref inVerteces, inTopology);

			RenderTargetBlendDescription targetBlendDesc = new RenderTargetBlendDescription()
			{
				BlendEnable = false,
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.Zero,
				SourceBlendAlpha = BlendOption.One,
				DestinationBlendAlpha = BlendOption.Zero,
				BlendOperation = BlendOperation.Add,
				BlendOperationAlpha = BlendOperation.Add,
				RenderTargetWriteMask = ColorWriteMaskFlags.All
			};

			BlendStateDescription blendDesc = new BlendStateDescription()
			{
				AlphaToCoverageEnable = true,
				IndependentBlendEnable = true
			};

			blendDesc.RenderTargets[0] = targetBlendDesc;

			BlendState blendState = BlendState.FromDescription(spDevice, blendDesc);

			spContext.OutputMerger.BlendState = blendState;

			//the most optimal drawing is at about 1000 verteces per Draw() call
			int length = inVerteces.Buffer.Description.SizeInBytes / Marshal.SizeOf(typeof(Vertex));
			int count = length;

			int i = 0;
			int endIndex = 0;

			for (i = 0; i < (length - inLimit); i += inLimit)
			{
				spContext.Draw(inLimit, i);
				count -= inLimit;
				endIndex += inLimit;
			}

			spContext.Draw(count, endIndex);

			blendState.Dispose();
		}



		public void RenderTexture(ref VertexBufferBinding inVerteces, ref ShaderResourceView inTexture, PrimitiveTopology inTopology)
		{
			PrepareContext(ref sTextureLayout, ref sTextureVertShader, ref sTexturePixShader, ref inVerteces, inTopology);

			sTextureArray[2] = inTexture;
			spContext.PixelShader.SetShaderResources(sTextureArray, 0, sTextureArray.Length);

			spContext.Draw((int)inVerteces.Buffer.Description.SizeInBytes / Marshal.SizeOf(typeof(TextureVertex)), 0);
		}



		public void RenderLines(ref VertexBufferBinding inVerteces)
		{
			if (inVerteces == null || inVerteces.Buffer == null || inVerteces.Buffer.Disposed || inVerteces.Buffer.Description == null || inVerteces.Buffer.Description.SizeInBytes == 0)
				return;

			Render(ref inVerteces, PrimitiveTopology.LineList, 1000);
		}



		public void RenderPolygons(ref VertexBufferBinding inVerteces)
		{
			if (inVerteces == null || inVerteces.Buffer == null || inVerteces.Buffer.Disposed || inVerteces.Buffer.Description.SizeInBytes == 0)
				return;

			Render(ref inVerteces, PrimitiveTopology.TriangleList, 999);
		}



		public void RenderSprites(ref VertexBufferBinding inVerteces)
		{
			if (inVerteces == null || inVerteces.Buffer == null || inVerteces.Buffer.Disposed || inVerteces.Buffer.Description.SizeInBytes == 0)
				return;

			SamplerDescription sampleDescr = new SamplerDescription()
			{
				Filter = Filter.MinMagMipLinear,
				AddressU = TextureAddressMode.Wrap,
				AddressV = TextureAddressMode.Wrap,
				AddressW = TextureAddressMode.Wrap,
				MipLodBias = 0.0f,
				MaximumAnisotropy = 1,
				ComparisonFunction = Comparison.Always,
				BorderColor = Color.Transparent,
				MinimumLod = 0,
				MaximumLod = float.MaxValue
			};

			SamplerState samplerState = SamplerState.FromDescription(spDevice, sampleDescr);

			RenderTargetBlendDescription targetBlendDesc = new RenderTargetBlendDescription()
			{
				BlendEnable = false,
				SourceBlend = BlendOption.One,
				DestinationBlend = BlendOption.One,
				SourceBlendAlpha = BlendOption.One,
				DestinationBlendAlpha = BlendOption.One,
				BlendOperation = BlendOperation.Add,
				BlendOperationAlpha = BlendOperation.Add,
				RenderTargetWriteMask = ColorWriteMaskFlags.All
			};

			BlendStateDescription blendDesc = new BlendStateDescription()
			{
				AlphaToCoverageEnable = true,
				IndependentBlendEnable = true
			};

			blendDesc.RenderTargets[0] = targetBlendDesc;

			BlendState blendState = BlendState.FromDescription(spDevice, blendDesc);

			spContext.OutputMerger.BlendState = blendState;

			spContext.PixelShader.SetSampler(samplerState, 0);

			PrepareContext(ref sSpriteLayout, ref sSpriteVertShader, ref sSpriteGeoShader, ref sSpritePixShader, ref inVerteces, PrimitiveTopology.PointList);

			spContext.GeometryShader.SetShaderResources(sTextureArray, 0, sTextureArray.Length);
			spContext.PixelShader.SetShaderResources(sTextureArray, 0, sTextureArray.Length);

			//the most optimal drawing is at about 1000 verteces per Draw() call
			int length = inVerteces.Buffer.Description.SizeInBytes / Marshal.SizeOf(typeof(SpriteVertex));
			int count = length;
			int i = 0;
			int endIndex = 0;
			for (i = 0; i < (length - 1000); i += 1000)
			{
				spContext.Draw(1000, i);
				count -= 1000;
				endIndex += 1000;
			}

			spContext.Draw(count, endIndex);

			samplerState.Dispose();
			blendState.Dispose();
		}



		public virtual void Dispose()
		{
			mRenderTarget.Dispose();
			mSwapChain.Dispose();
		}



		public static void FinalDispose()
		{
			spContext.Rasterizer.State.Dispose();
			spContext.Dispose();
			spDevice.Dispose();
			sLinePolyVertShader.Dispose();
			sLinePolyPixShader.Dispose();
			sTextureVertShader.Dispose();
			sTexturePixShader.Dispose();
			sSpriteVertShader.Dispose();
			sSpriteGeoShader.Dispose();
			sSpritePixShader.Dispose();
			sLinePolyLayout.Dispose();
			sTextTexture.Dispose();
			sSpriteTexture.Dispose();
			sTextureArray[0].Dispose();
			sTextureArray[1].Dispose();

			if(sTextureArray[2] != null)
				sTextureArray[2].Dispose();

			sTextureLayout.Dispose();
			sSpriteLayout.Dispose();
			spConstantBuffer.Dispose();
		}



		public class RenderTargetParameters
		{
			public IntPtr mpHandle { get; private set; }
			public int mpWidth { get; private set; }
			public int mpHeight { get; private set; }


			public RenderTargetParameters(IntPtr inHandle, int inWidth, int inHeight)
			{
				mpWidth = inWidth;
				mpHeight = inHeight;
				mpHandle = inHandle;
			}
		}



		[StructLayout(LayoutKind.Explicit, Size = 24)]
		public struct Vertex
		{
			[FieldOffset(0)]
			public Vector2 mPosition;

			[FieldOffset(8)]
			public Color4 mColor;
		}



		[StructLayout(LayoutKind.Explicit, Size = 20)]
		public struct TextureVertex
		{
			[FieldOffset(0)]
			public Vector2 mPosition;

			[FieldOffset(8)]
			public Vector2 mTexCoord;
		}



		[StructLayout(LayoutKind.Explicit, Size = 32)]
		public struct SpriteVertex
		{
			[FieldOffset(0)]
			public Vector2 mPosition;

			[FieldOffset(8)]
			public float mSizePercent;

			[FieldOffset(12)]
			public float mImageWidth;

			[FieldOffset(16)]
			public int mTextureId;

			[FieldOffset(20)]
			public float mTextureStart;

			[FieldOffset(24)]
			public float mRotation;

			[FieldOffset(28)]
			public float mXOffset;
		}
	}
}
