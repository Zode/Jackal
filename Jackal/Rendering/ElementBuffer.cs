using System;
using OpenTK.Graphics.OpenGL4;
using Jackal.Exceptions;

#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public unsafe class ElementBuffer : IDisposable
{
	private bool _disposed = false;
	private int _ID = 0;
	/// <summary>
	/// OpenGL ID.
	/// </summary>
	public int ID => _ID;
	private static int _lastBoundID = 0;
	/// <summary>
	/// Type of the indices.
	/// </summary>
	public ElementBufferType ElementBufferType {get; private set;} = ElementBufferType.UnsignedByte;
	/// <summary>
	/// Count of indices.
	/// </summary>
	public int Count {get; private set;} = 0;

	/// <summary>
	/// Initializes a new instance of ElementBuffer class.
	/// </summary>
	/// <param name="bufferType"><see cref="Jackal.Rendering.BufferType" /> to use.</param>
	/// <param name="indices"><see cref="Jackal.Rendering.VertexBuffer{T}" /> indices.</param>
	/// <exception cref="ElementBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public ElementBuffer(BufferType bufferType, uint[] indices)
	{
		if(indices.Length == 0)
		{
			throw new ElementBufferException("No indices");
		}

		ElementBufferType = ElementBufferType.UnsignedInt;
		Count = indices.Length;
		fixed(uint* indicesPtr = indices)
		{
			Initialize(bufferType, indices.Length * sizeof(uint), (IntPtr)indicesPtr);
		}
	}

	/// <summary>
	/// Initializes a new instance of ElementBuffer class.
	/// </summary>
	/// <param name="bufferType"><see cref="Jackal.Rendering.BufferType" /> to use.</param>
	/// <param name="indices"><see cref="Jackal.Rendering.VertexBuffer{T}" /> indices.</param>
	/// <exception cref="ElementBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public ElementBuffer(BufferType bufferType, ushort[] indices)
	{
		if(indices.Length == 0)
		{
			throw new ElementBufferException("No indices");
		}

		ElementBufferType = ElementBufferType.UnsignedShort;
		Count = indices.Length;
		fixed(ushort* indicesPtr = indices)
		{
			Initialize(bufferType, indices.Length * sizeof(ushort), (IntPtr)indicesPtr);
		}
	}

	/// <summary>
	/// Initializes a new instance of ElementBuffer class.
	/// </summary>
	/// <param name="bufferType"><see cref="Jackal.Rendering.BufferType" /> to use.</param>
	/// <param name="indices"><see cref="Jackal.Rendering.VertexBuffer{T}" /> indices.</param>
	/// <exception cref="ElementBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	public ElementBuffer(BufferType bufferType, byte[] indices)
	{
		if(indices.Length == 0)
		{
			throw new ElementBufferException("No indices");
		}

		ElementBufferType = ElementBufferType.UnsignedByte;
		Count = indices.Length;
		fixed(byte* indicesPtr = indices)
		{
			Initialize(bufferType, indices.Length * sizeof(byte), (IntPtr)indicesPtr);
		}
	}
	
	/// <summary>
	/// Initializes a new instance of ElementBuffer class.
	/// </summary>
	/// <param name="bufferType"><see cref="Jackal.Rendering.BufferType" /> to use.</param>
	/// <param name="size">Size of the indices (usually <c>indices.length * typeof(indices)</c>).</param>
	/// <param name="indices">Pointer to indices.</param>
	/// <exception cref="ElementBufferException"></exception>
	/// <exception cref="NotImplementedException"></exception>
	private void Initialize(BufferType bufferType, int size, IntPtr indices)
	{
		if(indices == IntPtr.Zero)
		{
			throw new ElementBufferException("Indices is null");
		}

		GL.CreateBuffers(1, out _ID);
		if(_ID == 0)
		{
			throw new ElementBufferException("Could not create element buffer object on OpenGL side");
		}

		BufferUsageHint bufferUsageHint = bufferType switch
		{
			BufferType.Static => BufferUsageHint.StaticDraw,
			BufferType.Dynamic => BufferUsageHint.DynamicDraw,
			BufferType.Stream => BufferUsageHint.StreamDraw,
			_ => throw new NotImplementedException(),
		};

		GL.NamedBufferData(_ID, size, indices, bufferUsageHint);
	}

	/// <summary>
	/// Draw the element buffer.
	/// </summary>
	/// <param name="primitiveType">Primitive type to draw the buffer as.</param>
	public void Draw(PrimitiveType primitiveType)
	{
		GL.DrawElements(primitiveType.ToGL(), Count, ElementBufferType.ToGL(), 0);
	}

	/// <summary>
	/// Bind the element buffer as currently active.
	/// </summary>
	public void Bind()
	{
		if(_lastBoundID == _ID || _ID == 0)
		{
			return;
		}

		_lastBoundID = _ID;
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ID);
	}

	/// <summary>
	/// Unbind any element buffer from being active.
	/// </summary>
	public static void Unbind()
	{
		GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
	}

	/// <summary>
	/// Dispose the element buffer.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose the element buffer.
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
	/// Destructor for ElementBuffer class.
	/// </summary>
	~ElementBuffer()
	{
		#if DEBUG
		Console.WriteLine("ElementBuffer leak! Did you forget to call dispose?");
		Debugger.Launch();
		#endif

		Dispose(false);
	}
}