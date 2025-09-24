using System;
using Jackal.Exceptions;
using System.Collections.Generic;
using OpenTK.Mathematics;

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
	public readonly Texture[] Textures;
	/// <summary>
	/// Default shader uniforms to set when this material is bound.
	/// </summary>
	public readonly Dictionary<string, MaterialUniform> Uniforms;

	/// <summary>
	/// Constructor for Material class.
	/// </summary>
	/// <param name="shader">Shader to use for this material.</param>
	/// <param name="textures">Textures to use for this material, if any.</param>
	/// <param name="uniforms">Shader uniform values to set when binding the material.</param>
	/// <exception cref="MaterialException"></exception>
	public Material(Shader shader, Texture[] textures, Dictionary<string, MaterialUniform> uniforms)
	{
		if(textures.Length >= Renderer.MaxTextureFragmentImageUnits)
		{
			throw new MaterialException($"Can't construct material with more textures ({textures.Length}) than allowed maximum limit ({Renderer.MaxTextureFragmentImageUnits})");
		}

		Shader = shader;
		Textures = textures;
		Uniforms = uniforms;
	}

	/// <summary>
	/// Bind the material's <seealso cref="Jackal.Rendering.Shader" /> and <seealso cref="Jackal.Rendering.Texture" />s as currently active.
	/// </summary>
	public void Bind()
	{
		Shader.Bind();
		for(int i = 0; i < Textures.Length; i++)
		{
			Textures[i].Bind(i);
		}

		foreach(string key in Uniforms.Keys)
		{
			switch(Uniforms[key].Type)
			{
				case MaterialUniformType.Bool:
					Shader.SetUniform1(Shader.GetUniformLocation(key), Uniforms[key].Bool);
					break;

				case MaterialUniformType.UInt:
					Shader.SetUniform1(Shader.GetUniformLocation(key), Uniforms[key].UInt);
					break;

				case MaterialUniformType.Int:
					Shader.SetUniform1(Shader.GetUniformLocation(key), Uniforms[key].Int);
					break;

				case MaterialUniformType.Float:
					Shader.SetUniform1(Shader.GetUniformLocation(key), Uniforms[key].Float);
					break;

				case MaterialUniformType.Vector2i:
					Shader.SetUniform2(Shader.GetUniformLocation(key), Uniforms[key].Vector2i);
					break;

				case MaterialUniformType.Vector2:
					Shader.SetUniform2(Shader.GetUniformLocation(key), Uniforms[key].Vector2);
					break;

				case MaterialUniformType.Vector2h:
					Shader.SetUniform2(Shader.GetUniformLocation(key), Uniforms[key].Vector2h);
					break;

				case MaterialUniformType.Vector3i:
					Shader.SetUniform3(Shader.GetUniformLocation(key), Uniforms[key].Vector3i);
					break;

				case MaterialUniformType.Vector3:
					Shader.SetUniform3(Shader.GetUniformLocation(key), Uniforms[key].Vector3);
					break;

				case MaterialUniformType.Vector3h:
					Shader.SetUniform3(Shader.GetUniformLocation(key), Uniforms[key].Vector3h);
					break;

				case MaterialUniformType.Vector4i:
					Shader.SetUniform4(Shader.GetUniformLocation(key), Uniforms[key].Vector4i);
					break;

				case MaterialUniformType.Vector4:
					Shader.SetUniform4(Shader.GetUniformLocation(key), Uniforms[key].Vector4);
					break;

				case MaterialUniformType.Vector4h:
					Shader.SetUniform4(Shader.GetUniformLocation(key), Uniforms[key].Vector4h);
					break;

				case MaterialUniformType.Matrix2:
					Matrix2 m2 = Uniforms[key].Matrix2; 
					Shader.SetUniformMatrix2(Shader.GetUniformLocation(key), ref m2);
					break;

				case MaterialUniformType.Matrix2x3:
					Matrix2x3 m2x3 = Uniforms[key].Matrix2x3; 
					Shader.SetUniformMatrix2x3(Shader.GetUniformLocation(key), ref m2x3);
					break;

				case MaterialUniformType.Matrix2x4:
					Matrix2x4 m2x4 = Uniforms[key].Matrix2x4; 
					Shader.SetUniformMatrix2x4(Shader.GetUniformLocation(key), ref m2x4);
					break;

				case MaterialUniformType.Matrix3:
					Matrix3 m3 = Uniforms[key].Matrix3; 
					Shader.SetUniformMatrix3(Shader.GetUniformLocation(key), ref m3);
					break;

				case MaterialUniformType.Matrix3x2:
					Matrix3x2 m3x2 = Uniforms[key].Matrix3x2; 
					Shader.SetUniformMatrix3x2(Shader.GetUniformLocation(key), ref m3x2);
					break;

				case MaterialUniformType.Matrix3x4:
					Matrix3x4 m3x4 = Uniforms[key].Matrix3x4; 
					Shader.SetUniformMatrix3x4(Shader.GetUniformLocation(key), ref m3x4);
					break;

				case MaterialUniformType.Matrix4:
					Matrix4 m4 = Uniforms[key].Matrix4; 
					Shader.SetUniformMatrix4(Shader.GetUniformLocation(key), ref m4);
					break;

				case MaterialUniformType.Matrix4x2:
					Matrix4x2 m4x2 = Uniforms[key].Matrix4x2; 
					Shader.SetUniformMatrix4x2(Shader.GetUniformLocation(key), ref m4x2);
					break;

				case MaterialUniformType.Matrix4x3:
					Matrix4x3 m4x3 = Uniforms[key].Matrix4x3; 
					Shader.SetUniformMatrix4x3(Shader.GetUniformLocation(key), ref m4x3);
					break;

				default:
					throw new NotImplementedException();
			}
		}
	}

	/// <summary>
	/// Unbind the material's <seealso cref="Jackal.Rendering.Shader" /> and <seealso cref="Jackal.Rendering.Texture" />s from being currently active.
	/// </summary>
	public void Unbind()
	{
		Shader.Unbind();
		for(int i = 0; i < Textures.Length; i++)
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
		for(int i = 0; i < Textures.Length; i++)
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