using System;
using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum ElementBufferType : byte
{
	/// <summary>
	/// </summary>
	UnsignedByte,
	/// <summary>
	/// </summary>
	UnsignedShort,
	/// <summary>
	/// </summary>
	UnsignedInt,
}

/// <summary>
/// Extensons for <see cref="Jackal.Rendering.ElementBufferType" />.
/// </summary>
public static class ElementBufferExtensions
{
	/// <summary>
	/// Convert <see cref="Jackal.Rendering.ElementBufferType" /> to <see cref="OpenTK.Graphics.OpenGL4.DrawElementsType" />.
	/// </summary>
	/// <param name="elementBufferType"></param>
	/// <returns></returns>
	public static DrawElementsType ToGL(this ElementBufferType elementBufferType)
	{
		return elementBufferType switch
		{
			ElementBufferType.UnsignedByte => DrawElementsType.UnsignedByte,
			ElementBufferType.UnsignedShort => DrawElementsType.UnsignedShort,
			ElementBufferType.UnsignedInt => DrawElementsType.UnsignedInt,
			_ => throw new NotImplementedException(),
		};
	}
}