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
	/// <summary>
	/// OpenGL ID.
	/// </summary>
	public int ID => _ID;
	private static int _lastBoundID = 0;
	private int _size = 0;
	private BufferType _bufferType;

	/// <summary>
	/// Initializes a new instance of VertexBuffer class.
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

		_bufferType = bufferType;
		GL.CreateBuffers(1, out _ID);
		if(_ID == 0)
		{
			throw new VertexBufferException("Could not create vertex buffer object on OpenGL side");
		}

		BufferUsageHint bufferUsageHint = bufferType switch
		{
			BufferType.Static => BufferUsageHint.StaticDraw,
			BufferType.Dynamic => BufferUsageHint.DynamicDraw,
			BufferType.Stream => BufferUsageHint.StreamDraw,
			_ => throw new NotImplementedException(),
		};

		_size = vertices.Length * Marshal.SizeOf<T>();
		GCHandle handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
		try
		{
			GL.NamedBufferData(_ID, _size, handle.AddrOfPinnedObject(), bufferUsageHint);
		}
		finally
		{
			handle.Free();
		}
	}

	/// <summary>
	/// Update the VertexBuffer contents. If buffer type or vertices size differs a new buffer is automatically allocated.
	/// </summary>
	/// <param name="bufferType"><see cref="Jackal.Rendering.BufferType" /> to potentially use.</param>
	/// <param name="vertices">Vertex array.</param>
	/// <exception cref="VertexBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public void Update(BufferType bufferType, T[] vertices)
	{
		if(vertices.Length == 0)
		{
			throw new VertexBufferException("No vertices");
		}
		
		if(_ID == 0)
		{
			throw new VertexBufferException("Can't update invalid buffer");
		}

		BufferUsageHint bufferUsageHint = bufferType switch
		{
			BufferType.Static => BufferUsageHint.StaticDraw,
			BufferType.Dynamic => BufferUsageHint.DynamicDraw,
			BufferType.Stream => BufferUsageHint.StreamDraw,
			_ => throw new NotImplementedException(),
		};

		int newSize = vertices.Length * System.Runtime.InteropServices.Marshal.SizeOf<T>();
		GCHandle handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
		try
		{
			if(bufferType != _bufferType || newSize != _size)
			{
				_bufferType = bufferType;
				_size = newSize;
				GL.NamedBufferData(_ID, _size, handle.AddrOfPinnedObject(), bufferUsageHint);
			}
			else
			{
				GL.NamedBufferSubData(_ID, 0, _size, handle.AddrOfPinnedObject());
			}
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
		if(_lastBoundID == _ID || _ID == 0)
		{
			return;
		}

		_lastBoundID = _ID;
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
		GL.DeleteBuffers(1, ref _ID);
		_ID = 0;
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