namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum BufferType : byte
{
	/// <summary>
	/// Buffer data will most likely not change or very rarely. 
	/// </summary>
	Static,
	/// <summary>
	/// Buffer data is likely to change a lot.
	/// </summary>
	Dynamic,
	/// <summary>
	/// Buffer data will change every time it's drawn.
	/// </summary>
	Stream,
}