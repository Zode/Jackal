using System;
using System.Collections.Generic;
using SDL;

namespace Jackal;

/// <summary>
/// Window property flags.
/// </summary>
[Flags]
public enum WindowFlags : byte
{
	/// <summary>
	/// No flags.
	/// </summary>
	None = 0x00,
	/// <summary>
	/// Window is resizeable.
	/// </summary>
	Resizeable = 1 << 0,
}

/// <summary>
/// Extensions for <see cref="Jackal.WindowFlags" />.
/// </summary>
public static class WindowFlagsExtensions
{
	private static readonly Dictionary<WindowFlags, SDL_WindowFlags> _toSDLTable = new()
	{
		{WindowFlags.Resizeable, SDL_WindowFlags.SDL_WINDOW_RESIZABLE},
	};

	/// <summary>
	/// Convert <see cref="Jackal.WindowFlags" /> to <see cref="SDL.SDL_WindowFlags" />
	/// </summary>
	/// <param name="windowFlags"></param>
	/// <returns></returns>
	public static SDL_WindowFlags ToSDL(this WindowFlags windowFlags)
	{
		SDL_WindowFlags sdlFlags = 0x00;
		foreach(KeyValuePair<WindowFlags, SDL_WindowFlags> pair in _toSDLTable)
		{
			if((windowFlags & pair.Key) == pair.Key)
			{
				sdlFlags |= pair.Value;
			}
		}

		return sdlFlags;
	}
}