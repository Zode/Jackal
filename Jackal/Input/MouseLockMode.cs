namespace Jackal.Input;

/// <summary>
/// </summary>
public enum MouseLockMode : byte
{
	/// <summary>
	/// Mouse cursor is free.
	/// </summary>
	None,
	/// <summary>
	/// Mouse cursor is constrained to <seealso cref="Jackal.Input.Inputs.MouseRect" /> inside the window.
	/// </summary>
	Constrained,
	/// <summary>
	/// Mouse cursor is locked to the window.
	/// </summary>
	Locked,
}