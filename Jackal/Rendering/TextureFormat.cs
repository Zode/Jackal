using System;
using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// Texture channel format.
/// </summary>
public enum TextureFormat : byte
{
	/// <summary>
	/// 8 bit red channel only.
	/// </summary>
	R,
	/// <summary>
	/// 8 bit red, and green channels.
	/// </summary>
	RG,
	/// <summary>
	/// 8 bit red, green, and blue channels.
	/// </summary>
	RGB,
	/// <summary>
	/// 8 bit blue, green, red channels.
	/// </summary>
	BGR,
	/// <summary>
	/// 8 bit red, green, blue, and alpha channels.
	/// </summary>
	RGBA,
	/// <summary>
	/// 8 bit blue, green, red, and alpha channels.
	/// </summary>
	BGRA,
}

/// <summary>
/// Extensons for <see cref="Jackal.Rendering.TextureFormat" />.
/// </summary>
public static class TextureFormatExtensions
{
	/// <summary>
	/// Convert <see cref="Jackal.Rendering.TextureFormat" /> to 8 bit <see cref="OpenTK.Graphics.OpenGL4.SizedInternalFormat" />.
	/// </summary>
	/// <param name="textureFormat"></param>
	/// <returns></returns>
	public static SizedInternalFormat ToGL8BitInternal(this TextureFormat textureFormat)
	{
		return textureFormat switch
		{
			TextureFormat.R => SizedInternalFormat.R8,
			TextureFormat.RG => SizedInternalFormat.Rg8,
			TextureFormat.RGB => SizedInternalFormat.Rgb8,
			TextureFormat.BGR => SizedInternalFormat.Rgb8,
			TextureFormat.RGBA => SizedInternalFormat.Rgba8,
			TextureFormat.BGRA => SizedInternalFormat.Rgba8,
			_ => throw new NotImplementedException(),
		};
	}

	/// <summary>
	/// Convert <see cref="Jackal.Rendering.TextureFormat" /> to 16 bit <see cref="OpenTK.Graphics.OpenGL4.SizedInternalFormat" />.
	/// </summary>
	/// <param name="textureFormat"></param>
	/// <returns></returns>
	public static SizedInternalFormat ToGL16BitInternal(this TextureFormat textureFormat)
	{
		return textureFormat switch
		{
			TextureFormat.R => SizedInternalFormat.R16,
			TextureFormat.RG => SizedInternalFormat.Rg16,
			TextureFormat.RGB => SizedInternalFormat.Rgb16,
			TextureFormat.BGR => SizedInternalFormat.Rgb16,
			TextureFormat.RGBA => SizedInternalFormat.Rgba16,
			TextureFormat.BGRA => SizedInternalFormat.Rgba16,
			_ => throw new NotImplementedException(),
		};
	}

	/// <summary>
	/// Convert <see cref="Jackal.Rendering.TextureFormat" /> to <see cref="OpenTK.Graphics.OpenGL4.PixelFormat" />.
	/// </summary>
	/// <param name="textureFormat"></param>
	/// <returns></returns>
	public static PixelFormat ToGLPixel(this TextureFormat textureFormat)
	{
		return textureFormat switch
		{
			TextureFormat.R => PixelFormat.Red,
			TextureFormat.RG => PixelFormat.Rg,
			TextureFormat.RGB => PixelFormat.Rgb,
			TextureFormat.BGR => PixelFormat.Bgr,
			TextureFormat.RGBA => PixelFormat.Rgba,
			TextureFormat.BGRA => PixelFormat.Bgra,
			_ => throw new NotImplementedException(),
		};
	}
}