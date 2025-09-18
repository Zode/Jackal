namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum VertexBufferType : byte
{
	/// <summary>
	/// Vertex buffer data will most likely not change or very rarely. 
	/// </summary>
	Static,
	/// <summary>
	/// Vertex buffer data is likely to change a lot.
	/// </summary>
	Dynamic,
	/// <summary>
	/// Vertex buffer data will change every time it's drawn.
	/// </summary>
	Stream,
}