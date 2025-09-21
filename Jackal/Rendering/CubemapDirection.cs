using System;
using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum CubemapDirection : byte
{
	/// <summary>
	/// </summary>
	PositiveX,
	/// <summary>
	/// </summary>
	NegativeX,
	/// <summary>
	/// </summary>
	PositiveY,
	/// <summary>
	/// </summary>
	NegativeY,
	/// <summary>
	/// </summary>
	PositiveZ,
	/// <summary>
	/// </summary>
	NegativeZ,
}

/// <summary>
/// Extensons for <see cref="Jackal.Rendering.CubemapDirection" />.
/// </summary>
public static class CubemapDirectionExtensions
{
	/// <summary>
	/// Convert <see cref="Jackal.Rendering.CubemapDirection" /> to <see cref="OpenTK.Graphics.OpenGL4.TextureTarget" />.
	/// </summary>
	/// <param name="cubemapDirection"></param>
	/// <returns></returns>
	public static TextureTarget ToGL(this CubemapDirection cubemapDirection)
	{
		return cubemapDirection switch
		{
			CubemapDirection.PositiveX => TextureTarget.TextureCubeMapPositiveX,
			CubemapDirection.NegativeX => TextureTarget.TextureCubeMapNegativeX,
			CubemapDirection.PositiveY => TextureTarget.TextureCubeMapPositiveY,
			CubemapDirection.NegativeY => TextureTarget.TextureCubeMapNegativeY,
			CubemapDirection.PositiveZ => TextureTarget.TextureCubeMapPositiveZ,
			CubemapDirection.NegativeZ => TextureTarget.TextureCubeMapNegativeZ,
			_ => throw new NotImplementedException(),
		};
	}
}