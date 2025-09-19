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
	/// <param name="indices"><see cref="Jackal.Rendering.VertexBuffer" /> indices.</param>
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
		fixed(uint* pIndices = indices)
		{
			Initialize(bufferType, indices.Length * sizeof(uint), (IntPtr)pIndices);
		}
	}

	/// <summary>
	/// Initializes a new instance of ElementBuffer class.
	/// </summary>
	/// <param name="bufferType"><see cref="Jackal.Rendering.BufferType" /> to use.</param>
	/// <param name="indices"><see cref="Jackal.Rendering.VertexBuffer" /> indices.</param>
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
		fixed(ushort* pIndices = indices)
		{
			Initialize(bufferType, indices.Length * sizeof(ushort), (IntPtr)pIndices);
		}
	}

	/// <summary>
	/// Initializes a new instance of ElementBuffer class.
	/// </summary>
	/// <param name="bufferType"><see cref="Jackal.Rendering.BufferType" /> to use.</param>
	/// <param name="indices"><see cref="Jackal.Rendering.VertexBuffer" /> indices.</param>
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
		fixed(byte* pIndices = indices)
		{
			Initialize(bufferType, indices.Length * sizeof(byte), (IntPtr)pIndices);
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

		_ID = GL.GenBuffer();
		if(_ID == 0)
		{
			throw new ElementBufferException("Could not create element buffer object on OpenGL side");
		}

		Bind();
		BufferUsageHint bufferUsageHint = bufferType switch
		{
			BufferType.Static => BufferUsageHint.StaticDraw,
			BufferType.Dynamic => BufferUsageHint.DynamicDraw,
			BufferType.Stream => BufferUsageHint.StreamDraw,
			_ => throw new NotImplementedException(),
		};

		GL.BufferData(BufferTarget.ElementArrayBuffer, size, indices, bufferUsageHint);
	}

	/// <summary>
	/// Bind the element buffer as currently active.
	/// </summary>
	public void Bind()
	{
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
		GL.DeleteBuffer(_ID);
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