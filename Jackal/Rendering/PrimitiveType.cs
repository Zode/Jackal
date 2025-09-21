using System;
using OpenTK.Graphics.OpenGL4;

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum PrimitiveType : byte
{
	/// <summary>
	/// Vertices are drawn as points.
	/// </summary>
	Points,
	/// <summary>
	/// Each group of 2 vertices is drawn as connected lines.
	/// </summary>
	Lines,
	/// <summary>
	/// Vertices are drawn as connected lines.
	/// </summary>
	LineStrip,
	/// <summary>
	/// Vertices are drawn as connected lines, with the last vertex connecting back to first vertex.
	/// </summary>
	LineLoop,
	/// <summary>
	/// Vertices are drawn as a triangle
	/// </summary>
	Triangles,
}

/// <summary>
/// Extensons for <see cref="Jackal.Rendering.PrimitiveType" />.
/// </summary>
public static class PrimitiveTypeExtensions
{
	/// <summary>
	/// Convert <see cref="Jackal.Rendering.PrimitiveType" /> to <see cref="OpenTK.Graphics.OpenGL4.PrimitiveType" />.
	/// </summary>
	/// <param name="primitiveType"></param>
	/// <returns></returns>
	public static OpenTK.Graphics.OpenGL4.PrimitiveType ToGL(this PrimitiveType primitiveType)
	{
		return primitiveType switch
		{
			PrimitiveType.Points => OpenTK.Graphics.OpenGL4.PrimitiveType.Points,
			PrimitiveType.Lines => OpenTK.Graphics.OpenGL4.PrimitiveType.Lines,
			PrimitiveType.LineStrip => OpenTK.Graphics.OpenGL4.PrimitiveType.LineStrip,
			PrimitiveType.LineLoop => OpenTK.Graphics.OpenGL4.PrimitiveType.LineLoop,
			PrimitiveType.Triangles => OpenTK.Graphics.OpenGL4.PrimitiveType.Triangles,
			_ => throw new NotImplementedException(),
		};
	}
}