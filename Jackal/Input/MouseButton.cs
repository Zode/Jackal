namespace Jackal.Input;

/// <summary>
/// </summary>
public enum MouseButton : byte
{
	#pragma warning disable CS1591
	[KeyString("Left Mouse")] Left,
	[KeyString("Right Mouse")] Right,
	[KeyString("Middle Mouse")] Middle,
	[KeyString("Scroll Up")] ScrollWheelUp,
	[KeyString("Scroll Down")] ScrollWheelDown,
	#pragma warning restore CS1591
}