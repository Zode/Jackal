using System;
using OpenTK.Graphics.OpenGL4;
using Jackal.Exceptions;

#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public class VertexArray : IDisposable
{
	private bool _disposed = false;
	private int _ID = 0;
	private int _lastBoundID = 0;

	/// <summary>
	/// Initializes a new instance of VertexArray class.
	/// </summary>
	/// <exception cref="VertexArrayException"></exception>
	public VertexArray()
	{
		_ID = GL.GenVertexArray();
		if(_ID == 0)
		{
			throw new VertexArrayException("Could not create vertex array object on OpenGL side");
		}

		Bind();
	}

	/// <summary>
	/// Bind the vertex array as currently active.
	/// </summary>
	public void Bind()
	{
		if(_lastBoundID == _ID || _ID == 0)
		{
			return;
		}

		_lastBoundID = _ID;
		GL.BindVertexArray(_ID);
	}

	/// <summary>
	/// Unbind any vertex array from being active.
	/// </summary>
	public static void Unbind()
	{
		GL.BindVertexArray(0);
	}

	/// <summary>
	/// Dispose the vertex array.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose the vertex array.
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if(_disposed || !disposing)
		{
			return;
		}

		Unbind();
		GL.DeleteVertexArray(_ID);
		_ID = 0;
		_disposed = true;
	}

	/// <summary>
	/// Destructor for VertexArray class.
	/// </summary>
	~VertexArray()
	{
		#if DEBUG
		Console.WriteLine("VertexArray leak! Did you forget to call dispose?");
		Debugger.Launch();
		#endif

		Dispose(false);
	}
}