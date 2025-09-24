using System;
using Jackal.Exceptions;

#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal.Rendering;

/// <summary>
/// Wraps <seealso cref="Jackal.Rendering.Shader" /> and <seealso cref="Jackal.Rendering.Texture" />s together.
/// </summary>
public class Material : IDisposable
{
	private bool _disposed = false;
	/// <summary>
	/// The shader for the material.
	/// </summary>
	public readonly Shader Shader;
	/// <summary>
	/// Textures for the material, if any.
	/// </summary>
	public readonly Texture[] Textures = new Texture[Renderer.MaxTextureFragmentImageUnits];
	private byte _texturesLength = 0;

	/// <summary>
	/// Constructor for Material class.
	/// </summary>
	/// <param name="shader">Shader to use for this material.</param>
	/// <param name="textures">Textures to use for this material, if any.</param>
	/// <exception cref="MaterialException"></exception>
	public Material(Shader shader, Texture[] textures)
	{
		if(textures.Length >= Renderer.MaxTextureFragmentImageUnits)
		{
			throw new MaterialException($"Can't construct material with more textures ({textures.Length}) than allowed maximum limit ({Renderer.MaxTextureFragmentImageUnits})");
		}

		Shader = shader;
		_texturesLength = (byte)textures.Length;
		textures.CopyTo(Textures, 0);
	}

	/// <summary>
	/// Bind the material's <seealso cref="Jackal.Rendering.Shader" /> and <seealso cref="Jackal.Rendering.Texture" />s as currently active.
	/// </summary>
	public void Bind()
	{
		Shader.Bind();
		for(byte i = 0; i < _texturesLength; i++)
		{
			Textures[i].Bind(i);
		}
	}

	/// <summary>
	/// Unbind the material's <seealso cref="Jackal.Rendering.Shader" /> and <seealso cref="Jackal.Rendering.Texture" />s from being currently active.
	/// </summary>
	public void Unbind()
	{
		Shader.Unbind();
		for(byte i = 0; i < _texturesLength; i++)
		{
			Textures[i].Unbind();
		}
	}

	/// <summary>
	/// Dispose the material.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose the material.
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if(_disposed || !disposing)
		{
			return;
		}

		Unbind();
		for(byte i = 0; i < _texturesLength; i++)
		{
			Textures[i].Dispose();
		}

		Shader.Dispose();
		_disposed = true;
	}

	/// <summary>
	/// Destructor for Material class.
	/// </summary>
	~Material()
	{
		#if DEBUG
		Console.WriteLine("Material leak! Did you forget to call dispose?");
		Debugger.Launch();
		#endif

		Dispose(false);
	}
}