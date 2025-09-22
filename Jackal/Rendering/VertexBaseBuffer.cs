using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// Base for <seealso cref="Jackal.Rendering.VertexBuffer{T}" />
/// </summary>
public abstract class VertexBaseBuffer
{
	/// <summary>
	/// OpenGL ID.
	/// </summary>
	protected int _ID = 0;
	/// <summary>
	/// Indice count in the buffer.
	/// </summary>
	protected int _indicesCount = 0;
	/// <summary>
	/// Element (indice) buffer type.
	/// </summary>
	protected ElementBufferType _elementBufferType = ElementBufferType.UnsignedInt;
	/// <summary>
	/// Indice offset in the buffer.
	/// </summary>
	protected int _indicesOffset = 0;
	
	/// <summary>
	/// Draw the vertex buffer.
	/// </summary>
	/// <param name="primitiveType">Primitive type to draw the buffer as.</param>
	public void Draw(PrimitiveType primitiveType)
	{
		if(_ID == 0)
		{
			return;
		}

		GL.DrawElements(primitiveType.ToGL(), _indicesCount, _elementBufferType.ToGL(), _indicesOffset);
	}
}