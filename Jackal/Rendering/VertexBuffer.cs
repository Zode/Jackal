using System;
using OpenTK.Graphics.OpenGL4;
using Jackal.Exceptions;
using System.Runtime.InteropServices;

#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public class VertexBuffer<T> : IDisposable where T : struct
{
	private bool _disposed = false;
	private int _ID = 0;
	private static int _LastBoundID = 0;
	private int _size = 0;

	/// <summary>
	/// Initializes a new instance of VertexBufer class.
	/// </summary>
	/// <param name="bufferType"><see cref="Jackal.Rendering.BufferType" /> to use.</param>
	/// <param name="vertices">Vertex array.</param>
	/// <exception cref="VertexBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public VertexBuffer(BufferType bufferType, T[] vertices)
	{
		if(vertices.Length == 0)
		{
			throw new VertexBufferException("No vertices");
		}

		_ID = GL.GenBuffer();
		if(_ID == 0)
		{
			throw new VertexBufferException("Could not create vertex buffer object on OpenGL side");
		}

		Bind();
		BufferUsageHint bufferUsageHint = bufferType switch
		{
			BufferType.Static => BufferUsageHint.StaticDraw,
			BufferType.Dynamic => BufferUsageHint.DynamicDraw,
			BufferType.Stream => BufferUsageHint.StreamDraw,
			_ => throw new NotImplementedException(),
		};

		_size = System.Runtime.InteropServices.Marshal.SizeOf<T>();
		GCHandle handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
		try
		{
			GL.BufferData(BufferTarget.ArrayBuffer, _size, handle.AddrOfPinnedObject(), bufferUsageHint);
		}
		finally
		{
			handle.Free();
		}
	}

	/// <summary>
	/// Bind the vertex buffer as currently active.
	/// </summary>
	public void Bind()
	{
		if(_LastBoundID == _ID || _ID == 0)
		{
			return;
		}

		_LastBoundID = _ID;
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