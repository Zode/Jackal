using Jackal.Exceptions;
using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public class VertexAttributeLayoutBuilder
{
	private VertexAttributeLayout _layout = new();

	/// <summary>
	/// Add attribute of byte type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddByte(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.Byte, count, normalized);
		return this;
	}

	/// <summary>
	/// Add attribute of unsigned byte type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddUnsignedByte(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.UnsignedByte, count, normalized);
		return this;
	}

	/// <summary>
	/// Add attribute of short type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddShort(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.Short, count, normalized);
		return this;
	}

	/// <summary>
	/// Add attribute of unsigned short type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddUnsignedShort(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.UnsignedShort, count, normalized);
		return this;
	}

	/// <summary>
	/// Add attribute of int type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddInt(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.Int, count, normalized);
		return this;
	}

	/// <summary>
	/// Add attribute of unsigned int type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddUnsignedInt(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.UnsignedInt, count, normalized);
		return this;
	}

	/// <summary>
	/// Add attribute of half float type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddHalfFloat(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.HalfFloat, count, normalized);
		return this;
	}

	/// <summary>
	/// Add attribute of float type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddFloat(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.Float, count, normalized);
		return this;
	}

	/// <summary>
	/// Add attribute of double type to the layout.
	/// </summary>
	/// <param name="count">Count of the type.</param>
	/// <param name="normalized">If <c>true</c> the value is normalized to 0-1 range.</param>
	/// <returns></returns>
	/// <exception cref="VertexAttributeLayoutException"></exception>
	public VertexAttributeLayoutBuilder AddDouble(int count, bool normalized = false)
	{
		if(count <= 0)
		{
			throw new VertexAttributeLayoutException("Count can't be less or equal to zero");
		}

		_layout.Add(VertexAttributeType.Double, count, normalized);
		return this;
	}

	/// <summary>
	/// Set the layout and attach the buffer
	/// </summary>
	public void SetLayoutAndAttach<T>(VertexArray vertexArray, VertexBuffer<T> vertexBuffer) where T : struct
	{
		vertexArray.Attach(vertexBuffer, _layout.Stride);
		int attribCount = 0;
		int offset = 0;
		for(int i = 0; i < _layout.Count; i++)
		{
			VertexAttributeElement attribute = _layout[i];
			GL.EnableVertexArrayAttrib(vertexArray.ID, i);
			GL.VertexArrayAttribFormat(vertexArray.ID, i, attribute.Count, VertexAttributeLayout.ToGLType(attribute.Type), attribute.Normalized, offset);
			GL.VertexArrayAttribBinding(vertexArray.ID, i, 0);
			offset += attribute.Count * VertexAttributeLayout.SizeOfType(attribute.Type);
			
			attribCount += attribute.Count;
			if(attribCount > Renderer.MaxVertexAttributes)
			{
				throw new VertexAttributeLayoutException($"Reached limit of maximum vertex attributes ({Renderer.MaxVertexAttributes})");
			}
		}
	}
}