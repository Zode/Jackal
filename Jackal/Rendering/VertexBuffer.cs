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
public class VertexBuffer<T> : VertexBaseBuffer, IDisposable where T : struct
{
	private bool _disposed = false;
	/// <summary>
	/// OpenGL ID.
	/// </summary>
	public int ID => _ID;
	private static int _lastBoundID = 0;
	private int _size = 0;

	/// <summary>
	/// Initializes a new instance of VertexBuffer class.
	/// </summary>
	/// <param name="vertices">Vertex array.</param>
	/// <param name="indices">Indices.</param>
	/// <exception cref="VertexBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public VertexBuffer(T[] vertices, uint[] indices)
	{
		if(vertices.Length == 0)
		{
			throw new VertexBufferException("No vertices");
		}

		if(indices.Length == 0)
		{
			throw new VertexBufferException("No indices");
		}

		GL.CreateBuffers(1, out _ID);
		if(_ID == 0)
		{
			throw new VertexBufferException("Could not create vertex buffer object on OpenGL side");
		}

		_elementBufferType = ElementBufferType.UnsignedInt;
		_indicesCount = indices.Length;

		int sizeOfT = Marshal.SizeOf<T>();
		int closestAlignment = Math.Max(sizeOfT, sizeof(uint));

		int verticesSize = vertices.Length * sizeOfT;
		int indicesSize = indices.Length * sizeof(uint);

		int verticesAligned = Align(verticesSize, closestAlignment);
		int indicesAligned = Align(indicesSize, closestAlignment);
		_indicesOffset = verticesAligned;

		_size = verticesAligned + indicesAligned;
		GL.NamedBufferStorage(_ID, _size, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
		GCHandle handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
		try
		{
			GL.NamedBufferSubData(_ID, 0, verticesSize, handle.AddrOfPinnedObject());
		}
		finally
		{
			handle.Free();
		}

		handle = GCHandle.Alloc(indices, GCHandleType.Pinned);
		try
		{
			GL.NamedBufferSubData(_ID, _indicesOffset, indicesSize, handle.AddrOfPinnedObject());
		}
		finally
		{
			handle.Free();
		}
	}
	
	/// <summary>
	/// Align operand to closest next alignment.
	/// </summary>
	/// <param name="operand">Value to align.</param>
	/// <param name="alignment">Alignment.</param>
	/// <returns>Aligned value.</returns>
	private static int Align(int operand, int alignment)
	{
		return (operand + (alignment - 1)) & ~(alignment - 1);
	}

	/// <summary>
	/// Update the VertexBuffer contents. If buffer type or vertices size differs a new buffer is automatically allocated.
	/// </summary>
	/// <param name="vertices">Vertex array.</param>
	/// <param name="indices">Indices.</param>
	/// <exception cref="VertexBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public void Update(T[] vertices, uint[] indices)
	{
		if(vertices.Length == 0)
		{
			throw new VertexBufferException("No vertices");
		}

		if(indices.Length == 0)
		{
			throw new VertexBufferException("No indices");
		}
		
		if(_ID == 0)
		{
			throw new VertexBufferException("Can't update invalid buffer");
		}

		if(_elementBufferType != ElementBufferType.UnsignedInt)
		{
			throw new VertexBufferException($"Buffer type mismatch, element was unsigned int buffer had {_elementBufferType}");
		}

		int sizeOfT = Marshal.SizeOf<T>();
		int closestAlignment = Math.Max(sizeOfT, sizeof(uint));

		int verticesSize = vertices.Length * sizeOfT;
		int indicesSize = indices.Length * sizeof(uint);

		int verticesAligned = Align(verticesSize, closestAlignment);
		int indicesAligned = Align(indicesSize, closestAlignment);

		int size = verticesAligned + indicesAligned;

		if(size != _size)
		{
			Unbind();
			GL.DeleteBuffers(1, ref _ID);
			_ID = 0;
			GL.CreateBuffers(1, out _ID);
			if(_ID == 0)
			{
				throw new VertexBufferException("Could not create vertex buffer object on OpenGL side");
			}

			GL.NamedBufferStorage(_ID, size, IntPtr.Zero, BufferStorageFlags.DynamicStorageBit);
		}

		_size = size;
		_indicesCount = indices.Length;
		_indicesOffset = verticesAligned;

		GCHandle handle = GCHandle.Alloc(vertices, GCHandleType.Pinned);
		try
		{
			GL.NamedBufferSubData(_ID, 0, verticesSize, handle.AddrOfPinnedObject());
		}
		finally
		{
			handle.Free();
		}

		handle = GCHandle.Alloc(indices, GCHandleType.Pinned);
		try
		{
			GL.NamedBufferSubData(_ID, _indicesOffset, indicesSize, handle.AddrOfPinnedObject());
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