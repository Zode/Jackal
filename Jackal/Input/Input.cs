using System;
using System.Collections;
using SDL;
using OpenTK.Mathematics;

namespace Jackal.Input;

/// <summary>
/// Keyboard and mouse inputs.
/// </summary>
public unsafe static class Inputs
{
	private static readonly BitArray _keysCurrentlyDown = new((int)KeyboardKey.RightSuper + 1, false);
	private static readonly BitArray _keysPreviouslyDown = new((int)KeyboardKey.RightSuper + 1, false);
	private static readonly BitArray _mouseCurrentlyDown = new((int)MouseButton.ScrollWheelDown + 1, false);
	private static readonly BitArray _mousePreviouslyDown = new((int)MouseButton.ScrollWheelDown + 1, false);
	internal static int MouseWheel {get; set;} = 0;
	/// <summary>
	/// The mouse coordinates inside the window when the cursor is free or constrained.
	/// Delta values when cursor is locked. 
	/// </summary>
	public static Vector2 MousePosition {get; private set;} = Vector2.Zero;
	/// <summary>
	/// The box in which the mouse will be constrained to when cursor lock is set to constrain. 
	/// </summary>
	public static Box2 MouseRect
	{
		get => _mouseRect; 
		set
		{
			_mouseRect = value;
			if(MouseLock == MouseLockMode.Constrained && Engine.GameWindow is not null)
			{
				SDL_Rect rect = new()
				{
					x = (int)MathF.Floor(MouseRect.Min.X),
					y = (int)MathF.Floor(MouseRect.Min.Y),
					w = (int)MathF.Floor(MouseRect.Max.X - MouseRect.Min.X),
					h = (int)MathF.Floor(MouseRect.Max.Y - MouseRect.Min.Y),
				};

				SDL3.SDL_SetWindowMouseRect(Engine.GameWindow.Window, &rect);
			}
		}
	}
	private static Box2 _mouseRect = new(Vector2.Zero, Vector2.One);
	/// <summary>
	/// If <c>true</c>, mouse cursor is returned to the center of window when unlocking from <seealso cref="Jackal.Input.MouseLockMode" />.Locked
	/// or to the center of the <seealso cref="MouseRect" /> when unlocking from <seealso cref="Jackal.Input.MouseLockMode" />.Constrained.
	/// If <c>false</c>, <seealso cref="MouseReturnPosition" /> is used instead.
	/// </summary>
	public static bool MouseReturnToCenter {get; set;} = true;
	/// <summary>
	/// The position to return the mouse cursor to when the cursor is unlocked when <seealso cref="MouseReturnToCenter" /> is <c>true</c>. 
	/// </summary>
	public static Vector2 MouseReturnPosition {get; set;} = Vector2.Zero;
	/// <summary>
	/// The cursor lock mode.
	/// Set <seealso cref="MouseRect" /> before enabling <seealso cref="Jackal.Input.MouseLockMode" />.Constrained.
	/// Cursor is automatically hidden during locked or constrained modes.
	/// </summary>
	public static MouseLockMode MouseLock
	{
		get => _mouseLock;
		set
		{
			if(Engine.GameWindow is null)
			{
				return;
			}

			switch(value)
			{
				case MouseLockMode.None:
					SDL3.SDL_SetWindowMouseRect(Engine.GameWindow.Window, null);

					if(MouseReturnToCenter)
					{
						if(_mouseLock == MouseLockMode.Locked)
						{
							SDL3.SDL_WarpMouseInWindow(Engine.GameWindow.Window, Engine.GameWindow.WindowSettings.Width / 2.0f, Engine.GameWindow.WindowSettings.Height / 2.0f);
						}
						else if(_mouseLock == MouseLockMode.Constrained)
						{
							SDL3.SDL_WarpMouseInWindow(Engine.GameWindow.Window, (MouseRect.Max.X - MouseRect.Min.X) / 2.0f, (MouseRect.Max.Y - MouseRect.Min.Y) / 2.0f);
						}
					}
					else
					{
						if(_mouseLock != MouseLockMode.None)
						{
							SDL3.SDL_WarpMouseInWindow(Engine.GameWindow.Window, MouseReturnPosition.X, MouseReturnPosition.Y);
						}
					}

					SDL3.SDL_SetWindowRelativeMouseMode(Engine.GameWindow.Window, false);
					break;

				case MouseLockMode.Locked:
					SDL3.SDL_SetWindowMouseRect(Engine.GameWindow.Window, null);
					SDL3.SDL_SetWindowRelativeMouseMode(Engine.GameWindow.Window, true);
					break;

				case MouseLockMode.Constrained:
					SDL_Rect rect = new()
					{
						x = (int)MathF.Floor(MouseRect.Min.X),
						y = (int)MathF.Floor(MouseRect.Min.Y),
						w = (int)MathF.Floor(MouseRect.Max.X - MouseRect.Min.X),
						h = (int)MathF.Floor(MouseRect.Max.Y - MouseRect.Min.Y),
					};

					SDL3.SDL_SetWindowMouseRect(Engine.GameWindow.Window, &rect);
					SDL3.SDL_SetWindowRelativeMouseMode(Engine.GameWindow.Window, true);

					float mouseX;
					float mouseY;
					SDL3.SDL_GetMouseState(&mouseX, &mouseY);
					SDL3.SDL_WarpMouseInWindow(Engine.GameWindow.Window,
						MathF.Max(MouseRect.Min.X, MathF.Min(mouseX, MouseRect.Max.X - 1)),
						MathF.Max(MouseRect.Min.Y, MathF.Min(mouseY, MouseRect.Max.Y - 1)));

					break;
			}

			_mouseLock = value;
		}
	}
	private static MouseLockMode _mouseLock = MouseLockMode.None;
	/// <summary>
	/// If enabled, the mouse cursor is hidden.
	/// </summary>
	public static bool MouseHidden
	{
		get => _mouseHidden; 
		set
		{
			_mouseHidden = value;
			if(value)
			{
				SDL3.SDL_HideCursor();
			}
			else
			{
				SDL3.SDL_ShowCursor();
			}
		}
	}
	private static bool _mouseHidden = false;

	/// <summary>
	/// </summary>
	internal static void Process()
	{
		int numKeys = 0;
		SDLBool* scanCodes = SDL3.SDL_GetKeyboardState(&numKeys);
		for(int i = (int)KeyboardKey.A; i < Math.Min(numKeys, (int)KeyboardKey.RightSuper + 1); i++)
		{
			if((i >= (int)SDL_Scancode.SDL_SCANCODE_POWER && i <= (int)SDL_Scancode.SDL_SCANCODE_KP_EQUALS)
				|| (i >= (int)SDL_Scancode.SDL_SCANCODE_EXECUTE && i <= (int)SDL_Scancode.SDL_SCANCODE_KP_HEXADECIMAL)
				|| i == 222 || i == 223)
			{
				continue;
			}

			_keysCurrentlyDown[i] = scanCodes[i];
		}

		float mouseX = 0.0f;
		float mouseY = 0.0f;
		SDL_MouseButtonFlags mouseFlags;
		if(MouseLock == MouseLockMode.Locked)
		{
			mouseFlags = SDL3.SDL_GetRelativeMouseState(&mouseX, &mouseY);

		}
		else
		{
			mouseFlags = SDL3.SDL_GetMouseState(&mouseX, &mouseY);
		}	

		MousePosition = new(mouseX, mouseY);
		_mouseCurrentlyDown[(int)MouseButton.Left] = (mouseFlags & SDL_MouseButtonFlags.SDL_BUTTON_LMASK) == SDL_MouseButtonFlags.SDL_BUTTON_LMASK;
		_mouseCurrentlyDown[(int)MouseButton.Right] = (mouseFlags & SDL_MouseButtonFlags.SDL_BUTTON_RMASK) == SDL_MouseButtonFlags.SDL_BUTTON_RMASK;
		_mouseCurrentlyDown[(int)MouseButton.Middle] = (mouseFlags & SDL_MouseButtonFlags.SDL_BUTTON_MMASK) == SDL_MouseButtonFlags.SDL_BUTTON_MMASK;
		_mouseCurrentlyDown[(int)MouseButton.ScrollWheelUp] = MouseWheel > 0;
		_mouseCurrentlyDown[(int)MouseButton.ScrollWheelDown] = MouseWheel < 0;
	}

	/// <summary>
	/// </summary>
	internal static void PostProcess()
	{
		for(int i = 0; i <= (int)KeyboardKey.RightSuper; i++)
		{
			_keysPreviouslyDown[i] = _keysCurrentlyDown[i];
		}

		for(int i = 0; i <= (int)MouseButton.ScrollWheelDown; i++)
		{
			_mousePreviouslyDown[i] = _mouseCurrentlyDown[i];
		}

		MouseWheel = 0;
	}

	/// <summary>
	/// Check if the specified key is currently down.
	/// </summary>
	/// <param name="key"><see cref="Jackal.Input.KeyboardKey" /> to check.</param>
	/// <returns><c>true</c> if key is down, <c>false</c> otherwise.</returns>
	public static bool KeyDown(KeyboardKey key)
	{
		return _keysCurrentlyDown[(int)key];
	}

	/// <summary>
	/// Check if the specified key was pressed.
	/// </summary>
	/// <param name="key"><see cref="Jackal.Input.KeyboardKey" /> to check.</param>
	/// <returns><c>true</c> if key was presed, <c>false</c> otherwise.</returns>
	public static bool KeyPressed(KeyboardKey key)
	{
		return _keysCurrentlyDown[(int)key] && !_keysPreviouslyDown[(int)key];
	}

	/// <summary>
	/// Check if the specified key was released.
	/// </summary>
	/// <param name="key"><see cref="Jackal.Input.KeyboardKey" /> to check.</param>
	/// <returns><c>true</c> if key was released, <c>false</c> otherwise.</returns>
	public static bool KeyReleased(KeyboardKey key)
	{
		return !_keysCurrentlyDown[(int)key] && _keysPreviouslyDown[(int)key];
	}

	/// <summary>
	/// Check if the specified mouse button is currently down.
	/// </summary>
	/// <param name="button"><see cref="Jackal.Input.MouseButton" /> to check.</param>
	/// <returns><c>true</c> if button is down, <c>false</c> otherwise.</returns>
	public static bool MouseDown(MouseButton button)
	{
		return _mouseCurrentlyDown[(int)button];
	}

	/// <summary>
	/// Check if the specified mouse button is was pressed.
	/// </summary>
	/// <param name="button"><see cref="Jackal.Input.MouseButton" /> to check.</param>
	/// <returns><c>true</c> if button was pressed, <c>false</c> otherwise.</returns>
	public static bool MousePressed(MouseButton button)
	{
		return _mouseCurrentlyDown[(int)button] & !_mousePreviouslyDown[(int)button];
	}

	/// <summary>
	/// Check if the specified mouse button was released.
	/// </summary>
	/// <param name="button"><see cref="Jackal.Input.MouseButton" /> to check.</param>
	/// <returns><c>true</c> if button was released, <c>false</c> otherwise.</returns>
	public static bool MouseReleased(MouseButton button)
	{
		return !_mouseCurrentlyDown[(int)button] & _mousePreviouslyDown[(int)button];
	}
}