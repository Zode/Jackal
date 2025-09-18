using System;
using System.IO;
using Jackal.Exceptions;
using OpenTK.Graphics.OpenGL4;

#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public class Shader : IDisposable
{
	private bool _disposed = false;
	private int _ID = 0;

	/// <summary>
	/// Create a new shader from GLSL source files.
	/// </summary>
	/// <param name="vertexFilePath">File path to the vertex glsl.</param>
	/// <param name="fragmentFilePath">File path to the fragment glsl.</param>
	/// <returns>The new shader instance.</returns>
	/// <exception cref="ShaderException"></exception>
	private static Shader FromGLSLFile(string vertexFilePath, string fragmentFilePath)
	{
		string vertexSource = File.ReadAllText(vertexFilePath);
		string fragmentSource = File.ReadAllText(fragmentFilePath);
		
		int vertexID = GL.CreateShader(OpenTK.Graphics.OpenGL4.ShaderType.VertexShader);
		int fragmentID = GL.CreateShader(OpenTK.Graphics.OpenGL4.ShaderType.FragmentShader);
		if(vertexID == 0 || fragmentID == 0)
		{
			throw new ShaderException("Could not create shader object(s) on OpenGL side");
		}

		GL.ShaderSource(vertexID, vertexSource);
		GL.ShaderSource(fragmentID, fragmentSource);

		GL.CompileShader(vertexID);
		GL.GetShader(vertexID, ShaderParameter.CompileStatus, out int success);
		if(success == 0)
		{
			throw new ShaderException($"Could not compile vertex shader object: {GL.GetShaderInfoLog(vertexID)}");
		}

		GL.CompileShader(fragmentID);
		GL.GetShader(fragmentID, ShaderParameter.CompileStatus, out success);
		if(success == 0)
		{
			throw new ShaderException($"Could not compile fragment shader object: {GL.GetShaderInfoLog(fragmentID)}");
		}

		int programID = GL.CreateProgram();
		if(programID == 0)
		{
			throw new ShaderException("Could not create shader program on OpenGL side");
		}

		GL.AttachShader(programID, vertexID);
		GL.AttachShader(programID, fragmentID);

		GL.LinkProgram(programID);
		GL.GetProgram(programID, GetProgramParameterName.LinkStatus, out success);
		if(success == 0)
		{
			throw new ShaderException($"Could not link shader program: {GL.GetProgramInfoLog(programID)}");
		}

		GL.DetachShader(programID, vertexID);
		GL.DetachShader(programID, fragmentID);
		GL.DeleteShader(vertexID);
		GL.DeleteShader(fragmentID);

		return new()
		{
			_ID = programID,
		};
	}

	/// <summary>
	/// Bind the shader as currently active.
	/// </summary>
	public void Bind()
	{
		GL.UseProgram(_ID);
	}

	/// <summary>
	/// Dispose the shader.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose the shader.
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if(_disposed || !disposing)
		{
			return;
		}

		GL.DeleteProgram(_ID);
		_disposed = true;
	}

	/// <summary>
	/// Destructor for Shader class.
	/// </summary>
	~Shader()
	{
		#if DEBUG
		Console.WriteLine("Shader leak! Did you forget to call dispose?");
		Debugger.Launch();
		#endif

		Dispose(false);
	}
}