namespace Jackal;

/// <summary>
/// Container for <see cref="Jackal.GameWindow" /> settings.
/// </summary>
public struct WindowSettings
{
	/// <summary>
	/// The title of the game window.
	/// </summary>
	public string Title {get; set;}
	/// <summary>
	/// Width of the game window.
	/// </summary>
	public int Width {get; set;}
	/// <summary>
	/// Height of the game window.
	/// </summary>
	public int Height {get; set;}
	/// <summary>
	/// <see cref="Jackal.WindowFlags" /> of the game window.
	/// </summary>
	public WindowFlags WindowFlags {get; set;}
}