using System;
using System.IO;
using Jackal.Rendering;
using OpenTK.Mathematics;
using StbImageSharp;
using Jackal.Exceptions;


#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal.FileFormats;

/// <summary>
/// StbImageSharp based texture loader
/// </summary>
public unsafe class STBTextureLoader : ITextureLoader
{
	private bool _disposed = false;
	private FileStream? _fileStream = null;
	private ImageInfo _imageInfo;

	/// <inheritdoc />
	public void LoadFile(string filePath)
	{
		StbImage.stbi_set_flip_vertically_on_load(1);
		if(_fileStream is not null)
		{
			_fileStream.Dispose();
		}
		
		_fileStream = File.OpenRead(filePath);
		if(_fileStream.Length == 0 || !_fileStream.CanRead)
		{
			throw new TextureLoaderException("FileStream has no contents");
		}

		_imageInfo = ImageInfo.FromStream(_fileStream) ?? throw new TextureLoaderException("Failed to get ImageInfo");
	}

	/// <inheritdoc />
	public TextureFormat GetTextureFormat()
	{
		if(_fileStream is null)
		{
			throw new TextureLoaderException("FileStream is null");
		}

		return _imageInfo.ColorComponents switch
		{
			ColorComponents.Grey => TextureFormat.R,
			ColorComponents.GreyAlpha => TextureFormat.RG,
			ColorComponents.RedGreenBlue => TextureFormat.RGB,
			ColorComponents.RedGreenBlueAlpha => TextureFormat.RGBA,
			_ => throw new TextureLoaderException("Unsupported color component combination"),
		};
	}

	/// <inheritdoc />
	public Vector2i GetTextureResolution()
	{
		if(_fileStream is null)
		{
			throw new TextureLoaderException("FileStream is null");
		}

		return new Vector2i(_imageInfo.Width, _imageInfo.Height);
	}

	/// <inheritdoc />
	public bool Is16Bit()
	{
		if(_fileStream is null)
		{
			throw new TextureLoaderException("FileStream is null");
		}
		
		return _imageInfo.BitsPerChannel == 16;
	}

	/// <inheritdoc />
	public ushort[] GetTextureData16()
	{
		if(_fileStream is null)
		{
			throw new TextureLoaderException("FileStream is null");
		}

		if(_imageInfo.BitsPerChannel == 8)
		{
			throw new TextureLoaderException("Can't load 16 bit texture data from 8 bit source");
		}
		
		byte[] data8 = ImageResult.FromStream(_fileStream, _imageInfo.ColorComponents).Data; 
		ushort[] data16 = new ushort[data8.LongLength / 2];
		for(int i = 0; i < data8.LongLength; i += 2)
		{
			data16[i] = (ushort)((data8[i] << 8) + data8[i + 1]);
		}

		return data16;
	}

	/// <inheritdoc />
	public byte[] GetTextureData8()
	{
		if(_fileStream is null)
		{
			throw new TextureLoaderException("FileStream is null");
		}

		if(_imageInfo.BitsPerChannel == 16)
		{
			throw new TextureLoaderException("Can't load 8 bit texture data from 16 bit source");
		}

		return ImageResult.FromStream(_fileStream, _imageInfo.ColorComponents).Data;
	}

	/// <summary>
	/// Dispose the loader.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose the loader.
	/// </summary>
	/// <param name="disposing"></param>
	public void Dispose(bool disposing)
	{
		if(_disposed || !disposing)
		{
			return;
		}
		
		_fileStream?.Dispose();
		_disposed = true;
	}

	/// <summary>
	/// Destructor for STBTextureLoader class.
	/// </summary>
	~STBTextureLoader()
	{
		#if DEBUG
		Console.WriteLine("VertexBuffer leak! Did you forget to call dispose?");
		Debugger.Launch();
		#endif

		Dispose(false);
	}
}