using System;
using OpenTK.Graphics.OpenGL4;
using Jackal.Exceptions;
using System.IO;
using OpenTK.Mathematics;
using Jackal.FileFormats;
using System.Collections.Generic;

#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public unsafe class Texture : IDisposable
{
	private bool _disposed = false;
	private int _ID = 0;
	private static int _lastBoundID = 0;
	/// <summary>
	/// Type of the texture.
	/// </summary>
	public TextureType TextureType => _textureType;
	private readonly TextureType _textureType;
	/// <summary>
	/// Texture wrap mode.
	/// </summary>
	public TextureWrap TextureWrap => _textureWrap;
	private readonly TextureWrap _textureWrap;
	/// <summary>
	/// Texture filter override.
	/// </summary>
	public TextureFilter TextureFilterOverride => _textureFilterOverride;
	private readonly TextureFilter _textureFilterOverride;
	/// <summary>
	/// <c>true</c> if the texture has mipmaps, otherwise <c>false</c>.
	/// </summary>
	public bool HasMipmaps => _hasMipmaps; 
	private readonly bool _hasMipmaps;
	/// <summary>
	/// Texture anisotropy override.
	/// </summary>
	public TextureAnisotropy TextureAnisotropyOverride => _textureAnisotropyOverride;
	private readonly TextureAnisotropy _textureAnisotropyOverride;
	/// <summary>
	/// The texture size.
	/// </summary>
	public Vector2i TextureSize => _textureSize;
	private Vector2i _textureSize;
	private static Dictionary<string, Type> _textureLoaders = [];

	private Texture(Vector2i textureSize, TextureSettings textureSettings)
	{
		_textureSize = textureSize;
		_textureType = textureSettings.TextureType;
		_textureWrap = textureSettings.TextureWrap;
		_textureFilterOverride = textureSettings.TextureFilterOverride;
		_hasMipmaps = textureSettings.Mipmaps;
		_textureAnisotropyOverride = textureSettings.TextureAnisotropyOverride;
	}

	/// <summary>
	/// Add a texture loader for <seealso cref="FromFile" /> function.
	/// </summary>
	/// <param name="fileExtension">The file extension to register this to, with the dot included.</param>
	/// <param name="textureLoaderType">The type of the texture loader.</param>
	public static void AddTextureLoader(string fileExtension, Type textureLoaderType)
	{
		if(!typeof(ITextureLoader).IsAssignableFrom(textureLoaderType))
		{
			return;
		}

		_textureLoaders.TryAdd(fileExtension, textureLoaderType);
	}

	/// <summary>
	/// Load 1D, 2D, or 1D array 8 bit or 16 bit texture from a file.
	/// </summary>
	/// <param name="filePath">Path to texture file.</param>
	/// <param name="textureSettings">Texture settings to use.</param>
	/// <returns>Texture object if successful.</returns>
	/// <exception cref="TextureException"></exception>
	public static Texture FromFile(string filePath, TextureSettings textureSettings)
	{
		if(!File.Exists(filePath))
		{
			throw new TextureException($"Filepath \"{filePath}\" does not exist or is a directory.");
		}

		string fileExtension = Path.GetExtension(filePath);
		if(!_textureLoaders.TryGetValue(fileExtension, out Type? textureLoaderType))
		{
			throw new TextureException($"Unsupported texture file format: {fileExtension}");
		}

		ITextureLoader? textureLoader = (ITextureLoader?)Activator.CreateInstance(textureLoaderType);
		if(textureLoader is null)
		{
			throw new TextureException($"Failed to instantiate loader: {textureLoaderType}");
		}

		textureLoader.LoadFile(filePath);
		Vector2i textureSize = textureLoader.GetTextureResolution();
		DataCheck1D2D(textureSize, textureSettings);

		if(textureLoader.Is16Bit())
		{
			return FromData(textureLoader.GetTextureData16(), textureSize, textureLoader.GetTextureFormat(), textureSettings);
		}
		else
		{
			return FromData(textureLoader.GetTextureData8(), textureSize, textureLoader.GetTextureFormat(), textureSettings);
		}
	}

	/// <summary>
	/// Load 3D, 2D array, or Cubemap 8 bit or 16 bit texture from a file.
	/// </summary>
	/// <param name="filePaths">Paths to texture files.</param>
	/// <param name="textureSettings">Texture settings to use.</param>
	/// <returns>Texture object if successful.</returns>
	/// <exception cref="TextureException"></exception>
	public static Texture FromFiles(string[] filePaths, TextureSettings textureSettings)
	{
		if(filePaths.Length == 0)
		{
			throw new TextureException($"No files");
		}

		string fileExtension = string.Empty;
		foreach(string filePath in filePaths)
		{
			if(!File.Exists(filePath))
			{
				throw new TextureException($"Filepath \"{filePath}\" does not exist or is a directory.");
			}

			fileExtension = Path.GetExtension(filePath);
			if(!_textureLoaders.ContainsKey(fileExtension))
			{
				throw new TextureException($"Unsupported texture file format: {fileExtension}");
			}
		}

		//start constructing directly here, so we don't have to load all textures into memory at once.
		// <stares at the GC>

		fileExtension = Path.GetExtension(filePaths[0]);
		if(!_textureLoaders.TryGetValue(fileExtension, out Type? textureLoaderType))
		{
			throw new TextureException($"Unsupported texture file format: {fileExtension}");
		}

		ITextureLoader? textureLoader = (ITextureLoader?)Activator.CreateInstance(textureLoaderType);
		if(textureLoader is null)
		{
			throw new TextureException($"Failed to instantiate loader: {textureLoaderType}");
		}

		textureLoader.LoadFile(filePaths[0]);
		Vector2i textureSize = textureLoader.GetTextureResolution();
		//blind-ish check for the contentsLength, but its enough to determine if we have _something_ in there.
		DataCheck3DArraysCubemap(filePaths.Length, textureSize.X * textureSize.Y, textureSize, textureSettings);

		Texture texture = new(textureSize, textureSettings);
		texture.SetupGLTexture();
		TextureFormat textureFormat = textureLoader.GetTextureFormat();
		bool use16bit = textureLoader.Is16Bit();
		SetUnpackAlignment(textureLoader.GetTextureFormat(), use16bit);
		SizedInternalFormat sizedFormat = use16bit ? textureFormat.ToGL16BitInternal() : textureFormat.ToGL8BitInternal();
		MultidimDataTexStorage(filePaths.Length, use16bit, textureSize, textureFormat, textureSettings);
		for(int i = 0; i < filePaths.Length; i++)
		{
			if(i != 0)
			{
				if(!File.Exists(filePaths[i]))
				{
					throw new TextureException($"Filepath \"{filePaths[i]}\" does not exist or is a directory.");
				}

				string newExtension = Path.GetExtension(filePaths[i]);
				if(newExtension != fileExtension)
				{
					throw new TextureException($"Filetype ({newExtension}) mismatch, must be same as first texture ({fileExtension})");
				}

				textureLoader.LoadFile(filePaths[i]);
				Vector2i newSize = textureLoader.GetTextureResolution();
				if(textureSize != newSize)
				{
					throw new TextureException($"Texture size ({newSize.X} x {newSize.Y}) mismatch, must be same size as first texture ({textureSize.X} x {textureSize.Y})");
				}

				TextureFormat newFormat = textureLoader.GetTextureFormat();
				if(newFormat != textureFormat)
				{
					throw new TextureException($"Texture format ({newFormat}) mismatch, must be same size as first texture ({textureFormat})");
				}

				bool newIs16Bit = textureLoader.Is16Bit();
				if(use16bit != newIs16Bit)
				{
					throw new TextureException($"Texture must be same bit depth ({(newIs16Bit ? "16" : "8")}) as first texture ({(use16bit ? "16" : "8")})");
				}
			}

			if(use16bit)
			{
				fixed(ushort* dataPtr = textureLoader.GetTextureData16())
				{
					MultidimDataTexSubImage((IntPtr)dataPtr, i, use16bit, textureSize, textureFormat, PixelType.UnsignedShort, textureSettings);
				}
			}
			else
			{
				fixed(byte* dataPtr = textureLoader.GetTextureData8())
				{
					MultidimDataTexSubImage((IntPtr)dataPtr, i, use16bit, textureSize, textureFormat, PixelType.UnsignedByte, textureSettings);
				}
			}
		}

		texture.SetWraps(textureSettings.TextureType, textureSettings.TextureWrap);
		texture.SetFilters(textureSettings.Mipmaps, textureSettings.TextureFilterOverride);
		texture.SetAnisotropy(textureSettings.TextureAnisotropyOverride);
		if(textureSettings.Mipmaps)
		{
			texture.SetupMipmaps(textureSettings.TextureType);
		}

		return texture;
	}

	/// <summary>
	/// Construct 1D, 2D, or 1D array 8 bit texture from unsigned bytes or unsigned shorts.
	/// </summary>
	/// <param name="data">Texture data to use.</param>
	/// <param name="textureSize">Texture size to use.</param>
	/// <param name="textureFormat">Texture format to use.</param>
	/// <param name="textureSettings">Texture settings to use.</param>
	/// <returns>Texture object if successful.</returns>
	/// <exception cref="TextureException"></exception>
	public static Texture FromData<T>(T[] data, Vector2i textureSize, TextureFormat textureFormat, TextureSettings textureSettings)
	{
		bool use16bit = false;
		PixelType pixelType = PixelType.UnsignedByte;
		switch(Type.GetTypeCode(typeof(T)))
		{
			case TypeCode.Byte:
				break;

			case TypeCode.UInt16: //ushort
				use16bit = true;
				pixelType = PixelType.UnsignedShort;
				break;

			default:
				throw new TextureException($"Unsupported data type {Type.GetTypeCode(data.GetType())}");
		}

		DataCheck1D2D(data, textureSize, textureSettings);
		Texture texture = new(textureSize, textureSettings);
		texture.SetupGLTexture();

		SetUnpackAlignment(textureFormat, use16bit);
		SizedInternalFormat sizedFormat = use16bit ? textureFormat.ToGL16BitInternal() : textureFormat.ToGL8BitInternal();
		switch(textureSettings.TextureType)
		{
			case TextureType.OneDimensional:
				GL.TexStorage1D(TextureTarget1d.Texture1D, 1 + (int)MathF.Floor(MathF.Log2(textureSize.X)), sizedFormat, textureSize.X);
				if(use16bit)
				{
					GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, textureSize.X, textureFormat.ToGLPixel(), pixelType, data as ushort[]);
				}
				else
				{
					GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, textureSize.X, textureFormat.ToGLPixel(), pixelType, data as byte[]);
				}

				break;

			case TextureType.TwoDimensional:
				GL.TexStorage2D(TextureTarget2d.Texture2D, 1 + (int)MathF.Floor(MathF.Log2(MathF.Max(textureSize.X, textureSize.Y))), sizedFormat, textureSize.X, textureSize.Y);
				if(use16bit)
				{
					GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, textureSize.X, textureSize.Y, textureFormat.ToGLPixel(), pixelType, data as ushort[]);
				}
				else
				{
					GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, textureSize.X, textureSize.Y, textureFormat.ToGLPixel(), pixelType, data as byte[]);
				}

				break;

			case TextureType.OneDimensionalArray:
				GL.TexStorage2D(TextureTarget2d.Texture1DArray, 1 + (int)MathF.Floor(MathF.Log2(textureSize.X)), sizedFormat, textureSize.X, textureSize.Y);
				if(use16bit)
				{
					GL.TexSubImage2D(TextureTarget.Texture1DArray, 0, 0, 0, textureSize.X, textureSize.Y, textureFormat.ToGLPixel(), pixelType, data as ushort[]);
				}
				else
				{
					GL.TexSubImage2D(TextureTarget.Texture1DArray, 0, 0, 0, textureSize.X, textureSize.Y, textureFormat.ToGLPixel(), pixelType, data as byte[]);
				}

				break;
		}

		texture.SetWraps(textureSettings.TextureType, textureSettings.TextureWrap);
		texture.SetFilters(textureSettings.Mipmaps, textureSettings.TextureFilterOverride);
		texture.SetAnisotropy(textureSettings.TextureAnisotropyOverride);
		if(textureSettings.Mipmaps)
		{
			texture.SetupMipmaps(textureSettings.TextureType);
		}

		return texture;
	}

	/// <summary>
	/// Construct 3D, 2D array, or Cubemap 8 bit texture from bytes.
	/// </summary>
	/// <param name="data">Texture data to use. first dimension is depth/layer/image, second dimension is image data.</param>
	/// <param name="textureSize">Texture size to use.</param>
	/// <param name="textureFormat">Texture format to use.</param>
	/// <param name="textureSettings">Texture settings to use.</param>
	/// <returns>Texture object if successful.</returns>
	/// <exception cref="TextureException"></exception>
	public static Texture FromData<T>(T[,] data, Vector2i textureSize, TextureFormat textureFormat, TextureSettings textureSettings)
	{
		bool use16bit = false;
		PixelType pixelType = PixelType.UnsignedByte;
		switch(Type.GetTypeCode(data.GetType()))
		{
			case TypeCode.Byte:
				break;

			case TypeCode.UInt16: //ushort
				use16bit = true;
				pixelType = PixelType.UnsignedShort;
				break;

			default:
				throw new TextureException($"Unsupported data type {Type.GetTypeCode(data.GetType())}");
		}

		DataCheck3DArraysCubemap(data.GetLength(0), data.GetLength(1), textureSize, textureSettings);
		Texture texture = new(textureSize, textureSettings);
		texture.SetupGLTexture();

		SetUnpackAlignment(textureFormat, use16bit);
		MultidimDataTexStorage(data.GetLength(0), use16bit, textureSize, textureFormat, textureSettings);
		for(int i = 0; i < data.GetLength(0); i++)
		{
			if(use16bit)
			{
				ushort[,] ushortData = data as ushort[,] ?? throw new TextureException("ushort data was null");
				fixed(ushort* dataPtr = &ushortData[i,0])
				{
					MultidimDataTexSubImage((IntPtr)dataPtr, i, use16bit, textureSize, textureFormat, pixelType, textureSettings);
				}
			}
			else
			{
				byte[,] byteData = data as byte[,] ?? throw new TextureException("byte data was null");
				fixed(byte* dataPtr = &byteData[i,0])
				{
					MultidimDataTexSubImage((IntPtr)dataPtr, i, use16bit, textureSize, textureFormat, pixelType, textureSettings);
				}
			}
		}

		texture.SetWraps(textureSettings.TextureType, textureSettings.TextureWrap);
		texture.SetFilters(textureSettings.Mipmaps, textureSettings.TextureFilterOverride);
		texture.SetAnisotropy(textureSettings.TextureAnisotropyOverride);
		if(textureSettings.Mipmaps)
		{
			texture.SetupMipmaps(textureSettings.TextureType);
		}

		return texture;
	}

	private static void MultidimDataTexStorage(int depth, bool use16bit, Vector2i textureSize, TextureFormat textureFormat, TextureSettings textureSettings)
	{
		SizedInternalFormat sizedFormat = use16bit ? textureFormat.ToGL16BitInternal() : textureFormat.ToGL8BitInternal();
		switch(textureSettings.TextureType)
		{
			case TextureType.TwoDimensionalArray:
				GL.TexStorage3D(TextureTarget3d.Texture2DArray, 1 + (int)MathF.Floor(MathF.Log2(MathF.Max(textureSize.X, textureSize.Y))), sizedFormat, textureSize.X, textureSize.Y, depth);
				break;

			case TextureType.ThreeDimensional:
				GL.TexStorage3D(TextureTarget3d.Texture3D, 1 + (int)MathF.Floor(MathF.Log2(MathF.Max(MathF.Max(textureSize.X, textureSize.Y), depth))), sizedFormat, textureSize.X, textureSize.Y, depth);
				break;

			case TextureType.CubeMap:
				GL.TexStorage2D(TextureTarget2d.TextureCubeMap, 1 + (int)MathF.Floor(MathF.Log2(MathF.Max(textureSize.X, textureSize.Y))), sizedFormat, textureSize.X, textureSize.Y);
				break;
		}
	}

	private static void MultidimDataTexSubImage(IntPtr data, int depth,  bool use16bit, Vector2i textureSize, TextureFormat textureFormat, PixelType pixelType, TextureSettings textureSettings)
	{
		switch(textureSettings.TextureType)
		{
			case TextureType.TwoDimensionalArray:
				GL.TexSubImage3D(TextureTarget.Texture2DArray, 0, 0, 0, depth, textureSize.X, textureSize.Y, 1, textureFormat.ToGLPixel(), pixelType, data);

				break;

			case TextureType.ThreeDimensional:
				GL.TexSubImage3D(TextureTarget.Texture3D, 0, 0, 0, depth, textureSize.X, textureSize.Y, 1, textureFormat.ToGLPixel(), pixelType, data);

				break;

			case TextureType.CubeMap:
				void PushToGL(CubemapDirection cubemapDirection)
				{
					GL.TexSubImage2D(cubemapDirection.ToGL(), 0, 0, 0, textureSize.X, textureSize.Y, textureFormat.ToGLPixel(), pixelType, data);
				}

				PushToGL((CubemapDirection)depth);
				break;
		}
	}

	private static void DataCheckCommon(Vector2i textureSize)
	{
		if(textureSize.X == 0 || textureSize.Y == 0)
		{
			throw new TextureException($"Texture size ({textureSize.X} x {textureSize.Y}) can't be zero");
		}

		if(textureSize.X >= Renderer.MaxTextureSize || textureSize.Y >= Renderer.MaxTextureSize)
		{
			throw new TextureException($"Texture size ({textureSize.X} x {textureSize.Y}) bigger than OpenGL driver limit ({Renderer.MaxTextureSize} x {Renderer.MaxTextureSize})");
		}
	}

	private static void DataCheck1D2D(Vector2i textureSize, TextureSettings textureSettings)
	{
		switch(textureSettings.TextureType)
		{
			case TextureType.CubeMap:
				throw new TextureException("Cubemap not allowed");

			case TextureType.TwoDimensionalArray:
				throw new TextureException("Two dimensional texture array not allowed");

			case TextureType.ThreeDimensional:
				throw new TextureException("Three dimensional texture not allowed");
		}

		DataCheckCommon(textureSize);
		if(textureSettings.TextureType == TextureType.OneDimensionalArray && textureSize.Y >= Renderer.MaxArrayTextureLevels)
		{
			throw new TextureException($"Texture array depth ({textureSize.Y}) bigger than OpenGL driver limit ({Renderer.MaxArrayTextureLevels})");
		}
	}

	private static void DataCheck1D2D<T>(T[] data, Vector2i textureSize, TextureSettings textureSettings)
	{
		if(data.Length == 0)
		{
			throw new TextureException($"Data has no contents");
		}
			
		DataCheck1D2D(textureSize, textureSettings);
	}

	private static void DataCheck3DArraysCubemap(int depth, int contentsLength, Vector2i textureSize, TextureSettings textureSettings)
	{
		if(depth == 0)
		{
			throw new TextureException($"Data has no depth");
		}

		if(contentsLength == 0)
		{
			throw new TextureException($"Data has no contents");
		}

		switch(textureSettings.TextureType)
		{
			case TextureType.OneDimensional:
				throw new TextureException("Non array one dimensional not allowed");

			case TextureType.TwoDimensional:
				throw new TextureException("Non array two dimensional not allowed");

			case TextureType.OneDimensionalArray:
				throw new TextureException("One dimensional texture array not allowed");
		}

		DataCheckCommon(textureSize);

		switch(textureSettings.TextureType)
		{
			case TextureType.CubeMap:
				if(depth != 6)
				{
					throw new TextureException($"Incorrect amount of sides ({depth}) for a cubemap");
				}
				
				if(textureSize.X != textureSize.Y)
				{
					throw new TextureException($"Cubemap texture ({textureSize.X} x {textureSize.Y}) not square.");
				}

				if(textureSize.X >= Renderer.MaxCubemapTextureSize || textureSize.Y >= Renderer.MaxCubemapTextureSize)
				{
					throw new TextureException($"Cubemap texture size ({textureSize.X} x {textureSize.Y}) bigger than OpenGL driver limit ({Renderer.MaxCubemapTextureSize} x {Renderer.MaxCubemapTextureSize})");
				}

				break;
				
			case TextureType.ThreeDimensional:
				if(textureSize.X >= Renderer.Max3DTextureSize || textureSize.Y >= Renderer.Max3DTextureSize || depth >= Renderer.Max3DTextureSize)
				{
					throw new TextureException($"3D texture size ({textureSize.X} x {textureSize.Y} x {depth}) bigger than OpenGL driver limit ({Renderer.Max3DTextureSize} x {Renderer.Max3DTextureSize} x {Renderer.Max3DTextureSize})");
				}

				break;

			case TextureType.TwoDimensionalArray:
				if(textureSize.X >= Renderer.MaxTextureSize || textureSize.Y >= Renderer.MaxTextureSize)
				{
					throw new TextureException($"Texture size ({textureSize.X} x {textureSize.Y}) bigger than OpenGL driver limit ({Renderer.MaxTextureSize} x {Renderer.MaxTextureSize})");
				}

				if(depth >= Renderer.MaxArrayTextureLevels)
				{
					throw new TextureException($"Texture array depth ({depth}) bigger than OpenGL driver limit ({Renderer.MaxArrayTextureLevels})");
				}

				break;
		}
	}

	private void SetupGLTexture()
	{
		GL.GenTextures(1, out _ID);
		if(_ID == 0)
		{
			throw new TextureException("Could not create texture object on OpenGL side");
		}

		Bind();
	}

	private static void SetUnpackAlignment(TextureFormat textureFormat, bool is16bit)
	{
		if(is16bit)
		{
			if(textureFormat == TextureFormat.RGBA || textureFormat == TextureFormat.BGRA)
			{
				GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
			}
			else
			{
				GL.PixelStore(PixelStoreParameter.UnpackAlignment, 2);
			}

			return;
		}

		if(textureFormat == TextureFormat.RGBA || textureFormat == TextureFormat.BGRA)
		{
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
		}
		else
		{
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
		}
	}

	private void SetFilters()
	{
		SetFilters(HasMipmaps, TextureFilterOverride);
	}

	private void SetFilters(bool mipmaps, TextureFilter filter)
	{
		Bind();
		filter = filter != TextureFilter.None ? filter : Renderer.TextureFilter;
		switch(filter)
		{
			case TextureFilter.NearestMipNearest:
				GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMinFilter, mipmaps ? (int)TextureMinFilter.NearestMipmapNearest : (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				break;

			case TextureFilter.NearestMipLinear:
				GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMinFilter, mipmaps ? (int)TextureMinFilter.NearestMipmapLinear : (int)TextureMinFilter.Nearest);
				GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
				break;

			case TextureFilter.LinearMipNear:
				GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMinFilter, mipmaps ? (int)TextureMinFilter.LinearMipmapNearest : (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				break;

			case TextureFilter.LinearMipLinear:
				GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMinFilter, mipmaps ? (int)TextureMinFilter.LinearMipmapLinear : (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				break;

			default:
				throw new NotImplementedException();
		}
	}

	private void SetAnisotropy()
	{
		SetAnisotropy(TextureAnisotropyOverride);
	}

	private void SetAnisotropy(TextureAnisotropy anisotropy)
	{
		Bind();
		anisotropy = anisotropy != TextureAnisotropy.None ? anisotropy.Clamp(Renderer.MaximumTextureAnisotropy) : Renderer.TextureAnisotropy;
		GL.TexParameter(TextureType.ToGLType(), TextureParameterName.TextureMaxAnisotropy, (float)anisotropy);
	}

	private void SetWraps(TextureType textureType, TextureWrap textureWrap)
	{
		Bind();
		switch(textureType)
		{
			case TextureType.OneDimensional:
			case TextureType.OneDimensionalArray:
				GL.TexParameter(textureType.ToGLType(), TextureParameterName.TextureWrapS, (int)textureWrap.ToGL());
				break;

			case TextureType.TwoDimensional:
			case TextureType.TwoDimensionalArray:
			case TextureType.CubeMap:
				GL.TexParameter(textureType.ToGLType(), TextureParameterName.TextureWrapS, (int)textureWrap.ToGL());
				GL.TexParameter(textureType.ToGLType(), TextureParameterName.TextureWrapT, (int)textureWrap.ToGL());
				break;

			case TextureType.ThreeDimensional:
				GL.TexParameter(textureType.ToGLType(), TextureParameterName.TextureWrapS, (int)textureWrap.ToGL());
				GL.TexParameter(textureType.ToGLType(), TextureParameterName.TextureWrapT, (int)textureWrap.ToGL());
				GL.TexParameter(textureType.ToGLType(), TextureParameterName.TextureWrapR, (int)textureWrap.ToGL());
				break;
		}
	}

	private void SetupMipmaps(TextureType textureType)
	{
		if(textureType == TextureType.CubeMap)
		{
			return;
		}

		Bind();
		GL.GenerateMipmap(textureType.ToGLMipmapType());
	}

	/// <summary>
	/// Bind the vertex array as currently active.
	/// </summary>
	public void Bind()
	{
		if(_lastBoundID == _ID || _ID == 0)
		{
			return;
		}

		_lastBoundID = _ID;
		GL.BindTexture(TextureType.ToGLType(), _ID);
	}

	/// <summary>
	/// Unbind this texture from being active.
	/// </summary>
	public void Unbind()
	{
		GL.BindTexture(TextureType.ToGLType(), 0);
	}

	/// <summary>
	/// Dispose the vertex array.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose the vertex array.
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if(_disposed || !disposing)
		{
			return;
		}

		Unbind();
		GL.DeleteTextures(1, ref _ID);
		_ID = 0;
		_disposed = true;
	}

	/// <summary>
	/// Destructor for VertexArray class.
	/// </summary>
	~Texture()
	{
		#if DEBUG
		Console.WriteLine("Texture leak! Did you forget to call dispose?");
		Debugger.Launch();
		#endif

		Dispose(false);
	}
}