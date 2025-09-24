using System.Runtime.InteropServices;
using OpenTK.Mathematics;

namespace Jackal.Rendering;

/// <summary>
/// Holds a "view into memory" for any single type of value used for shader uniform.
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct MaterialUniform
{
	/// <summary>
	/// Type stored in the struct.
	/// </summary>
	[FieldOffset(0)]
	public MaterialUniformType Type;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public bool Bool;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public uint UInt;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public int Int;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public float Float;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector2i Vector2i;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector2 Vector2;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector2h Vector2h;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector3i Vector3i;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector3 Vector3;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector3h Vector3h;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector4i Vector4i;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector4 Vector4;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Vector4h Vector4h;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix2 Matrix2;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix2x3 Matrix2x3;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix2x4 Matrix2x4;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix3 Matrix3;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix3x2 Matrix3x2;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix3x4 Matrix3x4;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix4 Matrix4;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix4x2 Matrix4x2;
	/// <summary>
	/// </summary>
	[FieldOffset(1)]
	public Matrix4x3 Matrix4x3;
}