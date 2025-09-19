using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// </summary>
/// <param name="type">Type of the attribute.</param>
/// <param name="count">Count of the attribute type.</param>
/// <param name="normalized">If <c>true</c> value is normalized to 0-1 range.</param>
internal struct VertexAttributeElement(VertexAttributeType type, int count, bool normalized)
{
	/// <summary>
	/// Type of the attribute.
	/// </summary>
	public VertexAttributeType Type {get; private set;} = type;
	/// <summary>
	/// Count of the attribute type.
	/// </summary>
	public int Count {get; private set;} = count;
	/// <summary>
	/// If <c>true</c> value is normalized to 0-1 range.
	/// </summary>
	public bool Normalized {get; private set;} = normalized;
}

/// <summary>
/// </summary>
internal class VertexAttributeLayout
{
	private readonly List<VertexAttributeElement> _vertexAttributes = [];
	/// <summary>
	/// Stride of the vertex layout.
	/// </summary>
	public int Stride {get; private set;} = 0;
	/// <summary>
	/// Count of VertexAttribute elements.
	/// </summary>
	public int Count => _vertexAttributes.Count;
	/// <summary>
	/// Get the attribute at specified index.
	/// </summary>
	/// <param name="index"></param>
	/// <returns></returns>
	public VertexAttributeElement this[int index] => _vertexAttributes[index];

	/// <summary>
	/// Add a vertex attribute of type.
	/// </summary>
	/// <param name="attributeType"><seealso cref="Jackal.Rendering.VertexAttributeType" /> to use.</param>
	/// <param name="count">Count of the attribute type.</param>
	/// <param name="normalized">If <c>true</c> value is normalized to 0-1 range.</param>
	public void Add(VertexAttributeType attributeType, int count, bool normalized)
	{
		_vertexAttributes.Add(new(attributeType, count, normalized));
		Stride += SizeOfType(attributeType) * count;
	}

	/// /// <summary>
	/// Get the size of the attribute type.
	/// </summary>
	/// <param name="attributeType"><seealso cref="Jackal.Rendering.VertexAttributeType" /> to check.</param>
	/// <returns></returns>
	/// <exception cref="NotImplementedException"></exception>
	public static int SizeOfType(VertexAttributeType attributeType)
	{
		return attributeType switch
		{
			VertexAttributeType.Byte => sizeof(sbyte),
			VertexAttributeType.UnsignedByte => sizeof(byte),
			VertexAttributeType.Short => sizeof(short),
			VertexAttributeType.UnsignedShort => sizeof(ushort),
			VertexAttributeType.Int => sizeof(int),
			VertexAttributeType.UnsignedInt => sizeof(uint),
			VertexAttributeType.HalfFloat => 16, //augh
			VertexAttributeType.Float => sizeof(float),
			VertexAttributeType.Double => sizeof(double),
			_ => throw new NotImplementedException(),
		};
	}

	public static VertexAttribPointerType ToGLType(VertexAttributeType attributeType)
	{
		return attributeType switch
		{
			VertexAttributeType.Byte => VertexAttribPointerType.Byte,
			VertexAttributeType.UnsignedByte => VertexAttribPointerType.UnsignedByte,
			VertexAttributeType.Short => VertexAttribPointerType.Short,
			VertexAttributeType.UnsignedShort => VertexAttribPointerType.UnsignedShort,
			VertexAttributeType.Int => VertexAttribPointerType.Int,
			VertexAttributeType.UnsignedInt => VertexAttribPointerType.UnsignedInt,
			VertexAttributeType.HalfFloat => VertexAttribPointerType.HalfFloat,
			VertexAttributeType.Float => VertexAttribPointerType.Float,
			VertexAttributeType.Double => VertexAttribPointerType.Double,
			_ => throw new NotImplementedException(),
		};
	}
}