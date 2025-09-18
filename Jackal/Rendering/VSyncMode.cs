namespace Jackal.Rendering;

/// <summary>
/// </summary>
public enum VSyncMode : byte
{
	/// <summary>
	/// Immediate frame updates.
	/// </summary>
	Disabled,
	/// <summary>
	/// Synchronize every frame to vertical retrace.
	/// </summary>
	Enabled,
	/// <summary>
	/// Adaptive vsync
	/// </summary>
	Adaptive,
}