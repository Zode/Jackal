using System;
using System.IO;
using Jackal.Rendering;
using OpenTK.Mathematics;

namespace Jackal.FileFormats;

/// <summary>
/// </summary>
public interface ITextureLoader : IDisposable
{
	/// <summary>
	/// Load texture from a file.
	/// </summary>
	/// <param name="filePath">Path to file.</param>
	public void LoadFile(string filePath);
	/// <summary>
	/// Get the texture resolution.
	/// </summary>
	/// <returns>Resolution of the texture.</returns>
	public Vector2i GetTextureResolution();
	/// <summary>
	/// Get the texture format from the provided stream.
	/// </summary>
	/// <returns>Format of the texture.</returns>
	public TextureFormat GetTextureFormat();
	/// <summary>
	/// Check if the texture is 16 bit.
	/// </summary>
	/// <returns><c>true</c> if 16 bit, <c>false</c> if 8 bit</returns>
	public bool Is16Bit();
	/// <summary>
	/// Get the texture data.
	/// </summary>
	/// <returns>8 bit texture data.</returns>
	public byte[] GetTextureData8();
	/// <summary>
	/// Get the texture data.
	/// </summary>
	/// <returns>16 bit texture data.</returns>
	public ushort[] GetTextureData16();
}