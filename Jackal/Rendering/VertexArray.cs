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
	/// <summary>
	/// OpenGL ID.
	/// </summary>
	public int ID => _ID;
	private static int _lastBoundID = 0;

	/// <summary>
	/// Initializes a new instance of VertexArray class.
	/// </summary>
	/// <exception cref="VertexArrayException"></exception>
	public VertexArray()
	{
		GL.CreateVertexArrays(1, out _ID);
		if(_ID == 0)
		{
			throw new VertexArrayException("Could not create vertex array object on OpenGL side");
		}
	}

	/// <summary>
	/// Attach a vertex buffer and element buffer.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="vertexBuffer"></param>
	/// <param name="elementBuffer"></param>
	/// <param name="stride"></param>
	/// <exception cref="VertexArrayException"></exception>
	public void Attach<T>(VertexBuffer<T> vertexBuffer, ElementBuffer elementBuffer, int stride) where T : struct
	{
		if(_ID == 0)
		{
			throw new VertexArrayException("Tried to attach vertex buffer and element buffer to non-existent vertex array");
		}

		if(vertexBuffer.ID == 0)
		{
			throw new VertexArrayException("Tried to attach non-existent vertex buffer to vertex array");
		}

		if(elementBuffer.ID == 0)
		{
			throw new VertexArrayException("Tried to attach non-existent element buffer to vertex array");
		}

		if(stride == 0)
		{
			throw new VertexArrayException("Stride can't be zero");
		}

		GL.VertexArrayVertexBuffer(_ID, 0, vertexBuffer.ID, 0, stride);
		GL.VertexArrayElementBuffer(_ID, elementBuffer.ID);
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
		GL.DeleteVertexArrays(1, ref _ID);
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