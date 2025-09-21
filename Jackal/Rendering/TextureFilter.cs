namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum TextureFilter : byte
{
	/// <summary>
	/// No filtering change, this is only used with the override setting in <seealso cref="Jackal.Rendering.Texture"/>.
	/// </summary>
	None,
	/// <summary> 
	/// Use nearest mipmap level (if applicable), then nearest texel to the pixel center.
	/// </summary>
	NearestMipNearest,
	/// <summary>
	/// Linearly interpolate two nearest mipmaps (if applicable), then nearest texel to the pixel center.
	/// </summary>
	NearestMipLinear,
	/// <summary>
	/// Use nearest mipmap level (if applicable), then sample four nearest texels to the pixel center.
	/// </summary>
	LinearMipNear,
	/// <summary>
	/// Linearly interpolate two nearest mipmaps (if applicable), then sample four nearest texels to the pixel center.
	/// </summary>
	LinearMipLinear,
}