namespace Jackal.Rendering;

/// <summary>
/// </summary>
public struct TextureSettings
{
	/// <summary>
	/// Type of the texture.
	/// </summary>
	public TextureType TextureType;
	/// <summary>
	/// Texture wrap mode.
	/// </summary>
	public TextureWrap TextureWrap;
	/// <summary>
	/// Texture filter override.
	/// </summary>
	public TextureFilter TextureFilterOverride;
	/// <summary>
	/// Set to <c>true</c> if the texture should have mipmaps.
	/// </summary>
	public bool Mipmaps;
	/// <summary>
	/// Texture anisotropy override.
	/// </summary>
	public TextureAnisotropy TextureAnisotropyOverride;
}