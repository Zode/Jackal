namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum TextureAnisotropy : byte
{
	/// <summary>
	/// No filtering change, this is only used with the override setting in <seealso cref="Jackal.Rendering.Texture"/>.
	/// </summary>
	None = 1,
	/// <summary>
	/// No anisotropy
	/// </summary>
	Zero = 1,
	/// <summary>
	/// 2X anisotropy
	/// </summary>
	Two = 2,
	/// <summary>
	/// 4X anisotropy
	/// </summary>
	Four = 4,
	/// <summary>
	/// 8X anisotropy
	/// </summary>
	Eight = 8,
	/// <summary>
	/// 16X anisotropy
	/// </summary>
	Sixteen = 16,
}

/// <summary>
/// Extensions for <see cref="Jackal.Rendering.TextureAnisotropy" />.
/// </summary>
public static class TextureAnisotropyExtensions
{
	/// <summary>
	/// Convert float value to the corresponding <see cref="Jackal.Rendering.TextureAnisotropy" />.
	/// </summary>
	/// <param name="textureAnisotropy"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	public static TextureAnisotropy FromFloat(this TextureAnisotropy textureAnisotropy, float value)
	{
		return value switch
		{
			> 8.0f => TextureAnisotropy.Sixteen,
			> 4.0f and <= 8.0f => TextureAnisotropy.Eight,
			> 2.0f and <= 4.0f => TextureAnisotropy.Four,
			> 1.0f and <= 2.0f => TextureAnisotropy.Two,
			<= 1.0f => TextureAnisotropy.Zero, 
			_ => TextureAnisotropy.Zero,
		};
	}
	
	/// <summary>
	/// Clamp the <see cref="Jackal.Rendering.TextureAnisotropy" /> to the maximum given <see cref="Jackal.Rendering.TextureAnisotropy" />.
	/// </summary>
	/// <param name="textureAnisotropy"></param>
	/// <param name="maximum"></param>
	/// <returns></returns>
	public static TextureAnisotropy Clamp(this TextureAnisotropy textureAnisotropy, TextureAnisotropy maximum)
	{
		if(maximum > textureAnisotropy)
		{
			return maximum;
		}

		return textureAnisotropy;
	}
}