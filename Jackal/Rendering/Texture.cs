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
public class Texture : IDisposable
{
	private bool _disposed = false;
	private int _ID = 0;
	private int _lastBoundID = 0;
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
	/// Load one or two dimensional 8 bit or 16 bit texture from a file.
	/// </summary>
	/// <param name="filePath">Path to texture file.</param>
	/// <param name="textureSettings">Texture settings to use.</param>
	/// <returns>Texture object if successful.</returns>
	/// <exception cref="TextureException"></exception>
	public static Texture FromFile(string filePath, TextureSettings textureSettings)
	{
		switch(textureSettings.TextureType)
		{
			case TextureType.CubeMap:
				throw new TextureException("Cubemap not allowed");

			case TextureType.OneDimensionalArray:
			case TextureType.TwoDimensionalArray:
				throw new TextureException("Texture array not allowed");

			case TextureType.ThreeDimensional:
				throw new TextureException("Three dimensional texture not allowed");
		}

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
			throw new TextureException($"Failed to instantiante loader: {textureLoaderType}");
		}

		textureLoader.LoadFile(filePath);
		Vector2i textureSize = textureLoader.GetTextureResolution();
		if(textureSize.X == 0 || textureSize.Y == 0)
		{
			throw new TextureException($"Texture size ({textureSize.X} x {textureSize.Y}) can't be zero");
		}

		if(textureSize.X >= Renderer.MaxTextureSize || textureSize.Y >= Renderer.MaxTextureSize)
		{
			throw new TextureException($"Texture size ({textureSize.X} x {textureSize.Y}) bigger than OpenGL driver limit ({Renderer.MaxTextureSize} x {Renderer.MaxTextureSize})");
		}

		if(textureLoader.Is16Bit())
		{
			return FromUShorts(textureLoader.GetTextureData16(), textureSize, textureLoader.GetTextureFormat(), textureSettings);
		}
		else
		{
			return FromBytes(textureLoader.GetTextureData8(), textureSize, textureLoader.GetTextureFormat(), textureSettings);
		}
	}

	/// <summary>
	/// Construct one or two dimensional 8 bit texture from unsigned bytes.
	/// </summary>
	/// <param name="data">Texture data to use.</param>
	/// <param name="textureSize">Texture size to use.</param>
	/// <param name="textureFormat">Texture format to use.</param>
	/// <param name="textureSettings">Texture settings to use.</param>
	/// <returns>Texture object if successful.</returns>
	/// <exception cref="TextureException"></exception>
	public static Texture FromBytes(byte[] data, Vector2i textureSize, TextureFormat textureFormat, TextureSettings textureSettings)
	{
		DataCheck1D2D(data, textureSize, textureSettings);
		Texture texture = new(textureSize, textureSettings);
		texture.SetupGLTexture();

		if(textureFormat == TextureFormat.RGBA || textureFormat == TextureFormat.BGRA)
		{
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 4);
		}
		else
		{
			GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
		}

		if(textureSettings.TextureType == TextureType.OneDimensional)
		{
			GL.TexStorage1D(TextureTarget1d.Texture1D, 1 + (int)MathF.Floor(MathF.Log2(textureSize.X)), textureFormat.ToGL8BitInternal(), textureSize.X);
			GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, textureSize.X, textureFormat.ToGLPixel(), PixelType.UnsignedByte, data);
		}
		else
		{
			GL.TexStorage2D(TextureTarget2d.Texture2D, 1 + (int)MathF.Floor(MathF.Log2(MathF.Max(textureSize.X, textureSize.Y))), textureFormat.ToGL8BitInternal(), textureSize.X, textureSize.Y);
			GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, textureSize.X, textureSize.Y, textureFormat.ToGLPixel(), PixelType.UnsignedByte, data);
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
	/// Construct one or two dimensional 16 bit texture from unsigned shorts.
	/// </summary>
	/// <param name="data">Texture data to use.</param>
	/// <param name="textureSize">Texture size to use.</param>
	/// <param name="textureFormat">Texture format to use.</param>
	/// <param name="textureSettings">Texture settings to use.</param>
	/// <returns>Texture object if successful.</returns>
	/// <exception cref="TextureException"></exception>
	public static Texture FromUShorts(ushort[] data, Vector2i textureSize, TextureFormat textureFormat, TextureSettings textureSettings)
	{
		DataCheck1D2D(data, textureSize, textureSettings);
		Texture texture = new(textureSize, textureSettings);
		texture.SetupGLTexture();

		if(textureSettings.TextureType == TextureType.OneDimensional)
		{
			GL.TexStorage1D(TextureTarget1d.Texture1D, 1 + (int)MathF.Floor(MathF.Log2(textureSize.X)), textureFormat.ToGL16BitInternal(), textureSize.X);
			GL.TexSubImage1D(TextureTarget.Texture1D, 0, 0, textureSize.X, textureFormat.ToGLPixel(), PixelType.UnsignedShort, data);
		}
		else
		{
			GL.TexStorage2D(TextureTarget2d.Texture2D, 1 + (int)MathF.Floor(MathF.Log2(MathF.Max(textureSize.X, textureSize.Y))), textureFormat.ToGL16BitInternal(), textureSize.X, textureSize.Y);
			GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, textureSize.X, textureSize.Y, textureFormat.ToGLPixel(), PixelType.UnsignedShort, data);
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

	private static void DataCheck1D2D<T>(T[] data, Vector2i textureSize, TextureSettings textureSettings)
	{
		switch(textureSettings.TextureType)
		{
			case TextureType.CubeMap:
				throw new TextureException("Cubemap not allowed");

			case TextureType.OneDimensionalArray:
			case TextureType.TwoDimensionalArray:
				throw new TextureException("Texture array not allowed");

			case TextureType.ThreeDimensional:
				throw new TextureException("Three dimensional texture not allowed");
		}

		if(data.Length == 0)
		{
			throw new TextureException($"Data has no contents");
		}
			
		if(textureSize.X == 0 || textureSize.Y == 0)
		{
			throw new TextureException($"Texture size ({textureSize.X} x {textureSize.Y}) can't be zero");
		}

		if(textureSize.X >= Renderer.MaxTextureSize || textureSize.Y >= Renderer.MaxTextureSize)
		{
			throw new TextureException($"Texture size ({textureSize.X} x {textureSize.Y}) bigger than OpenGL driver limit ({Renderer.MaxTextureSize} x {Renderer.MaxTextureSize})");
		}
	}

	private void SetupGLTexture()
	{
		_ID = GL.GenTexture();
		if(_ID == 0)
		{
			throw new TextureException("Could not create texture object on OpenGL side");
		}

		Bind();
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
		GL.DeleteTexture(_ID);
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