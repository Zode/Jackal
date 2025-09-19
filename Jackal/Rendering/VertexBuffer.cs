using System;
using OpenTK.Graphics.OpenGL4;
using Jackal.Exceptions;

#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public class VertexBuffer : IDisposable
{
	private bool _disposed = false;
	private int _ID = 0;

	/// <summary>
	/// Initializes a new instance of VertexBuffer class.
	/// </summary>
	/// <param name="vertexBufferType"><see cref="Jackal.Rendering.VertexBufferType" /> to use.</param>
	/// <param name="size">Size of the data (usually <c>data.length * typeof(data)</c>).</param>
	/// <param name="data">Pointer to data.</param>
	/// <exception cref="VertexBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public VertexBuffer(VertexBufferType vertexBufferType, int size, IntPtr data)
	{
		if(data == IntPtr.Zero)
		{
			throw new VertexBufferException("Data is empty");
		}

		_ID = GL.GenBuffer();
		if(_ID == 0)
		{
			throw new VertexBufferException("Could not create vertex buffer object on OpenGL side");
		}

		Bind();
		BufferUsageHint bufferUsageHint = vertexBufferType switch
		{
			VertexBufferType.Static => BufferUsageHint.StaticDraw,
			VertexBufferType.Dynamic => BufferUsageHint.DynamicDraw,
			VertexBufferType.Stream => BufferUsageHint.StreamDraw,
			_ => throw new NotImplementedException(),
		};

		GL.BufferData(BufferTarget.ArrayBuffer, size, data, bufferUsageHint);
	}

	/// <summary>
	/// Bind the vertex buffer as currently active.
	/// </summary>
	public void Bind()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, _ID);
	}

	/// <summary>
	/// Unbind any vertex buffer from being active.
	/// </summary>
	public static void Unbind()
	{
		GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
	}

	/// <summary>
	/// Dispose the vertex buffer.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose the vertex buffer.
	/// </summary>
	/// <param name="disposing"></param>
	protected virtual void Dispose(bool disposing)
	{
		if(_disposed || !disposing)
		{
			return;
		}

		Unbind();
		GL.DeleteBuffer(_ID);
		_disposed = true;
	}

	/// <summary>
	/// Destructor for VertexBuffer class.
	/// </summary>
	~VertexBuffer()
	{
		#if DEBUG
		Console.WriteLine("VertexBuffer leak! Did you forget to call dispose?");
		Debugger.Launch();
		#endif

		Dispose(false);
	}
}