using System;
using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum TextureWrap : byte
{
	/// <summary>
	/// Repeat the texture endlessly as it goes beyond the 0-1 range.
	/// </summary>
	Repeat,
	/// <summary>
	/// Repeat the texture endlessly, mirroring on each repeat as it goes beyond the 0-1 range. 
	/// </summary>
	MirroredRepeat,
	/// <summary>
	/// Clamp the texture to edges, stretching the edge pixels endlessly as it goes beyond the 0-1 range.
	/// </summary>
	Clamp,
}

/// <summary>
/// Extensons for <see cref="Jackal.Rendering.TextureWrap" />.
/// </summary>
public static class TextureWrapExtensions
{
	/// <summary>
	/// Convert <see cref="Jackal.Rendering.TextureWrap" /> to <see cref="OpenTK.Graphics.OpenGL4.TextureWrapMode" />.
	/// </summary>
	/// <param name="textureWrap"></param>
	/// <returns></returns>
	public static TextureWrapMode ToGL(this TextureWrap textureWrap)
	{
		return textureWrap switch
		{
			TextureWrap.Repeat => TextureWrapMode.Repeat,
			TextureWrap.MirroredRepeat => TextureWrapMode.MirroredRepeat,
			TextureWrap.Clamp => TextureWrapMode.ClampToEdge,
			_ => throw new NotImplementedException(),
		};
	}
}