using System;
using System.IO;
using Jackal.Exceptions;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using System.Collections.Generic;

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
	private static int _lastBoundID = 0;
	private Dictionary<string, int> _uniformLocations = [];

	/// <summary>
	/// Create a new shader from GLSL source files.
	/// </summary>
	/// <param name="vertexFilePath">File path to the vertex glsl.</param>
	/// <param name="fragmentFilePath">File path to the fragment glsl.</param>
	/// <returns>The new shader instance.</returns>
	/// <exception cref="ShaderException"></exception>
	public static Shader FromGLSLFile(string vertexFilePath, string fragmentFilePath)
	{
		try
		{
			return InternalFromGLSLFile(vertexFilePath, fragmentFilePath);
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
			return FromDefault();
		}
	}

	private static Shader InternalFromGLSLFile(string vertexFilePath, string fragmentFilePath)
	{
		if(!File.Exists(vertexFilePath))
		{
			throw new ShaderException($"Filepath \"{vertexFilePath}\" for vertex shader does not exist or is a directory");
		}

		if(!File.Exists(fragmentFilePath))
		{
			throw new ShaderException($"Filepath \"{fragmentFilePath}\" for fragment shader does not exist or is a directory");
		}

		string vertexSource = File.ReadAllText(vertexFilePath);
		string fragmentSource = File.ReadAllText(fragmentFilePath);
		
		return FromString(vertexSource, fragmentSource);
	}

	/// <summary>
	/// Create a new shader from GLSL source strings.
	/// </summary>
	/// <param name="vertexSource">String containing the vertex source.</param>
	/// <param name="fragmentSource">String containing the fragment source.</param>
	/// <returns>The new shader instance.</returns>
	/// <exception cref="ShaderException"></exception>
	public static Shader FromString(string vertexSource, string fragmentSource)
	{
		try
		{
			return InternalFromString(vertexSource, fragmentSource);
		}
		catch(Exception e)
		{
			Console.WriteLine(e.Message);
			return FromDefault();
		}
	}

	private static Shader InternalFromString(string vertexSource, string fragmentSource)
	{
		if(string.IsNullOrWhiteSpace(vertexSource))
		{
			throw new ShaderException("Vertex source is null or whitespace");
		}

		if(string.IsNullOrWhiteSpace(fragmentSource))
		{
			throw new ShaderException("Fragment source is null or whitespace");
		}

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

	private static Shader FromDefault()
	{
		return FromString("""
		#version 460 core
		layout (location = 0) in vec3 aPosition;
		
		void main()
		{
			gl_Position = vec4(aPosition, 1.0f);
		}
		""","""
		#version 460 core
		out vec4 FragColor;

		void main()
		{
			FragColor = vec4(1, 0, 1, 1);
		}
		""");
	}

	/// <summary>
	/// Returns the location of a uniform variable.
	/// </summary>
	/// <param name="name">Name of the variable.</param>
	/// <returns>Location of the uniform variable.</returns>
	// <exception cref="ShaderException"></exception>
	public int GetUniformLocation(string name)
	{
		if(_uniformLocations.ContainsKey(name))
		{
			return _uniformLocations[name];
		}

		Bind();
		int location = GL.GetUniformLocation(_ID, name);
		if(location == -1)
		{
			throw new ShaderException($"No such uniform: {name}");
		}

		_uniformLocations[name] = location;
		return location;
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform1(int location, bool value)
	{
		Bind();
		GL.Uniform1(location, value ? 1 : 0);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform1(int location, uint value)
	{
		Bind();
		GL.Uniform1(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform1(int location, int value)
	{
		Bind();
		GL.Uniform1(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform1(int location, float value)
	{
		Bind();
		GL.Uniform1(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">First value to set on the uniform.</param>
	/// <param name="value2">Second value to set on the uniform.</param>
	public void SetUniform2(int location, uint value, uint value2)
	{
		Bind();
		GL.Uniform2(location, value, value2);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform2(int location, Vector2i value)
	{
		Bind();
		GL.Uniform2(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform2(int location, Vector2 value)
	{
		Bind();
		GL.Uniform2(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform2(int location, Vector2h value)
	{
		Bind();
		GL.Uniform2(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">First value to set on the uniform.</param>
	/// <param name="value2">Second value to set on the uniform.</param>
	/// <param name="value3">Third value to set on the uniform.</param>
	public void SetUniform3(int location, uint value, uint value2, uint value3)
	{
		Bind();
		GL.Uniform3(location, value, value2, value3);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform3(int location, Vector3i value)
	{
		Bind();
		GL.Uniform3(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform3(int location, Vector3 value)
	{
		Bind();
		GL.Uniform3(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform3(int location, Vector3h value)
	{
		Bind();
		GL.Uniform3(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">First value to set on the uniform.</param>
	/// <param name="value2">Second value to set on the uniform.</param>
	/// <param name="value3">Third value to set on the uniform.</param>
	/// <param name="value4">Fourth value to set on the uniform.</param>
	public void SetUniform4(int location, uint value, uint value2, uint value3, uint value4)
	{
		Bind();
		GL.Uniform4(location, value, value2, value3, value4);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform4(int location, Vector4i value)
	{
		Bind();
		GL.Uniform4(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform4(int location, Vector4 value)
	{
		Bind();
		GL.Uniform4(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniform4(int location, Vector4h value)
	{
		Bind();
		GL.Uniform4(location, value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix2(int location, ref Matrix2 value)
	{
		Bind();
		GL.UniformMatrix2(location, true, ref value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix2x3(int location, ref Matrix2x3 value)
	{
		Bind();
		GL.UniformMatrix2x3(location, true, ref value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix2x4(int location, ref Matrix2x4 value)
	{
		Bind();
		GL.UniformMatrix2x4(location, true, ref value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix3(int location, ref Matrix3 value)
	{
		Bind();
		GL.UniformMatrix3(location, true, ref value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix3x2(int location, ref Matrix3x2 value)
	{
		Bind();
		GL.UniformMatrix3x2(location, true, ref value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix3x4(int location, ref Matrix3x4 value)
	{
		Bind();
		GL.UniformMatrix3x4(location, true, ref value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix4(int location, ref Matrix4 value)
	{
		Bind();
		GL.UniformMatrix4(location, true, ref value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix4x2(int location, ref Matrix4x2 value)
	{
		Bind();
		GL.UniformMatrix4x2(location, true, ref value);
	}

	/// <summary>
	/// Set the value of a uniform variable.
	/// </summary>
	/// <param name="location">Location of the uniform.</param>
	/// <param name="value">Value to set on the uniform.</param>
	public void SetUniformMatrix4x3(int location, ref Matrix4x3 value)
	{
		Bind();
		GL.UniformMatrix4x3(location, true, ref value);
	}

	/// <summary>
	/// Bind the shader as currently active.
	/// </summary>
	public void Bind()
	{
		if(_lastBoundID == _ID || _ID == 0)
		{
			return;
		}

		_lastBoundID = _ID;
		GL.UseProgram(_ID);
	}

	/// <summary>
	/// Unbind any shader from being currenty active.
	/// </summary>
	public static void Unbind()
	{
		GL.UseProgram(0);
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

		Unbind();
		GL.DeleteProgram(_ID);
		_ID = 0;
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