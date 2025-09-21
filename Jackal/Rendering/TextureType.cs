using System;
using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum TextureType : byte
{
	/// <summary>
	/// 1D texture
	/// </summary>
	OneDimensional,
	/// <summary>
	/// 2D texture
	/// </summary>
	TwoDimensional,
	/// <summary>
	/// 3D texture 
	/// </summary>
	ThreeDimensional,
	/// <summary>
	/// Array of 1D textures
	/// </summary>
	OneDimensionalArray,
	/// <summary>
	/// Array of 2D textures
	/// </summary>
	TwoDimensionalArray,
	/// <summary>
	/// Cubemap texture, does not have mipmap supoprt.
	/// </summary>
	CubeMap,
}

/// <summary>
/// Extensons for <see cref="Jackal.Rendering.TextureType" />.
/// </summary>
public static class TextureTypeExtensions
{
	/// <summary>
	/// Convert <see cref="Jackal.Rendering.TextureType" /> to <see cref="OpenTK.Graphics.OpenGL4.TextureTarget" />.
	/// </summary>
	/// <param name="textureType"></param>
	/// <returns></returns>
	public static TextureTarget ToGLType(this TextureType textureType)
	{
		return textureType switch
		{
			TextureType.OneDimensional => TextureTarget.Texture1D,
			TextureType.TwoDimensional => TextureTarget.Texture2D,
			TextureType.ThreeDimensional => TextureTarget.Texture3D,
			TextureType.OneDimensionalArray => TextureTarget.Texture1DArray,
			TextureType.TwoDimensionalArray => TextureTarget.Texture2DArray,
			TextureType.CubeMap => TextureTarget.TextureCubeMap, /* note: this is actually not correct */
			_ => throw new NotImplementedException(),
		};
	}

	/// <summary>
	/// Convert <see cref="Jackal.Rendering.TextureType" /> to <see cref="OpenTK.Graphics.OpenGL4.GenerateMipmapTarget" />.
	/// </summary>
	/// <param name="textureType"></param>
	/// <returns></returns>
	public static GenerateMipmapTarget ToGLMipmapType(this TextureType textureType)
	{
		return textureType switch
		{
			TextureType.OneDimensional => GenerateMipmapTarget.Texture1D,
			TextureType.TwoDimensional => GenerateMipmapTarget.Texture2D,
			TextureType.ThreeDimensional => GenerateMipmapTarget.Texture3D,
			TextureType.OneDimensionalArray => GenerateMipmapTarget.Texture1DArray,
			TextureType.TwoDimensionalArray => GenerateMipmapTarget.Texture2DArray,
			TextureType.CubeMap => GenerateMipmapTarget.TextureCubeMap,
			_ => throw new NotImplementedException(),
		};
	}
}