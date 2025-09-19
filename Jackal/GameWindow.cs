using System;
using System.Collections.Generic;
using SDL;
using Jackal.Rendering;
using Jackal.Input;
using Jackal.Exceptions;


#if DEBUG
using System.Diagnostics;
#endif

namespace Jackal;

/// <summary>
/// The game window class.
/// </summary>
public unsafe class GameWindow() : IDisposable
{
	/// <summary>
	/// The <see cref="Jackal.WindowSettings" /> for this window.
	/// </summary>
	public WindowSettings WindowSettings {get => _windowSettings; set  { _windowSettings = value; WindowSettingsUpdated(); }}
	/// <summary>
	/// List of <see cref="Jackal.Rendering.DisplayMode" />s.
	/// </summary>
	public List<DisplayMode> DisplayModes {get; private set;} = [];

	private WindowSettings _windowSettings = new()
	{
		Title = "Jackal Engine",
		Width = 320,
		Height = 240,
		WindowFlags = WindowFlags.None,
	};
	private bool _disposed = false;
	internal SDL_Window* Window = null;
	private SDL_GLContextState* _glContext = null;
	private bool _exitRequested = false;
	private bool _running = false;

	/// <summary>
	/// Start running and kick up the loop.
	/// </summary>
	public void Start()
	{
		SDL_WindowFlags flags = SDL_WindowFlags.SDL_WINDOW_OPENGL | WindowSettings.WindowFlags.ToSDL();
		
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_PROFILE_MASK, (int)SDL_GLProfile.SDL_GL_CONTEXT_PROFILE_CORE);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MAJOR_VERSION, 4);
		SDL3.SDL_GL_SetAttribute(SDL_GLAttr.SDL_GL_CONTEXT_MINOR_VERSION, 6);

		Window = SDL3.SDL_CreateWindow(WindowSettings.Title, WindowSettings.Width, WindowSettings.Height, flags);
		if(Window == null)
		{
			//throw new Exception($"Failed to make window: {SDL3.SDL_GetError()}");
			return;
		}

		Engine.GameWindow = this;
		_glContext = SDL3.SDL_GL_CreateContext(Window);
		if(_glContext == null)
		{
			//throw new Exception($"Failed to make OpenGL context: {SDL3.SDL_GetError()}");
			SDL3.SDL_DestroyWindow(Window);
			Window = null;
			return;
		}

		SDL3.SDL_GL_MakeCurrent(Window, _glContext);
		UpdateDisplayModes();
		Loop();
	}

	private void Loop()
	{
		Engine.Start();
		_running = true;
		OnStart();

		while(!_exitRequested)
		{
			SDL_Event @event;
			while(SDL3.SDL_PollEvent(&@event))
			{
				switch((SDL_EventType)@event.type)
				{
					case SDL_EventType.SDL_EVENT_WINDOW_CLOSE_REQUESTED:
					case SDL_EventType.SDL_EVENT_QUIT:
						RequestExit();
						break;

					case SDL_EventType.SDL_EVENT_WINDOW_DISPLAY_CHANGED:
						UpdateDisplayModes();
						break;

					case SDL_EventType.SDL_EVENT_WINDOW_RESIZED:
						Renderer.ResizeViewport(@event.window.data1, @event.window.data2);
						OnWindowResized(@event.window.data1, @event.window.data2);
						break;

					case SDL_EventType.SDL_EVENT_WINDOW_MOUSE_ENTER:
						OnMouseFocusChanged(true);
						break;

					case SDL_EventType.SDL_EVENT_WINDOW_MOUSE_LEAVE:
						OnMouseFocusChanged(false);
						break;

					case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_GAINED:
						OnKeyboardFocusChanged(true);
						break;

					case SDL_EventType.SDL_EVENT_WINDOW_FOCUS_LOST:
						OnKeyboardFocusChanged(false);
						break;

					case SDL_EventType.SDL_EVENT_MOUSE_WHEEL:
						switch(@event.wheel.direction)
						{
							case SDL_MouseWheelDirection.SDL_MOUSEWHEEL_NORMAL:
								Inputs.MouseWheel += @event.wheel.integer_y;
								break;
							
							case SDL_MouseWheelDirection.SDL_MOUSEWHEEL_FLIPPED:
								Inputs.MouseWheel -= @event.wheel.integer_y;
								break;
						}

						break;

					default:
						break;
				}
			}

			Engine.Process();
			SDL3.SDL_GL_SwapWindow(Window);
			Renderer.HandleFrameRateCap();
		}

		Engine.Shutdown();
	}

	/// <summary>
	/// Called when the window is resized.
	/// </summary>
	/// <param name="width">New width.</param>
	/// <param name="height">New height.</param>
	public virtual void OnWindowResized(int width, int height)
	{
	}

	/// <summary>
	/// Called when the mouse enters or leaves the window.
	/// </summary>
	/// <param name="mouseInWindow"><c>true</c> if mouse is inside window, <c>false</c> otherwise</param>
	public virtual void OnMouseFocusChanged(bool mouseInWindow)
	{
	}

	/// <summary>
	/// Called when the keyboard gains or loses focus of the window.
	/// </summary>
	/// <param name="focus"><c>true</c> if window has keyboard focus, <c>false</c> otherwise</param>
	public virtual void OnKeyboardFocusChanged(bool focus)
	{
	}

	/// <summary>
	/// Called when the engine has started, but before any logic is executed.
	/// </summary>
	public virtual void OnStart()
	{
	}

	/// <summary>
	/// Called when the engine runs its game logic tick update.
	/// </summary>
	public virtual void OnTickUpdate()
	{
	}

	/// <summary>
	/// Called when the engine renders a frame.
	/// </summary>
	public virtual void OnRenderFrame()
	{
	}

	/// <summary>
	/// Shuts down the application as soon as possible
	/// </summary>
	public void RequestExit()
	{
		_exitRequested = OnExitRequested();
	}

	/// <summary>
	/// Called when the application has requested an exit.
	/// </summary>
	/// <returns><c>true</c> if the game is allowed to close, <c>false</c> to keep it running</returns>
	public virtual bool OnExitRequested()
	{
		return true;
	}

	/// <summary>
	/// Called when the <see cref="WindowSettings" /> are updated.
	/// </summary>
	private void WindowSettingsUpdated()
	{
		if(!_running)
		{
			return;
		}

		SDL3.SDL_SetWindowTitle(Window, WindowSettings.Title);
		SDL3.SDL_SetWindowSize(Window, WindowSettings.Width, WindowSettings.Height);
		SDL3.SDL_SetWindowResizable(Window, (WindowSettings.WindowFlags & WindowFlags.Resizeable) == WindowFlags.Resizeable);
	}

	/// <summary>
	/// Refresh the stored display modes.
	/// </summary>
	private void UpdateDisplayModes()
	{
		DisplayModes.Clear();
		SDLPointerArray<SDL_DisplayMode>? displayModes = SDL3.SDL_GetFullscreenDisplayModes(SDL3.SDL_GetDisplayForWindow(Window));
		if(displayModes is null)
		{
			return;
		}

		foreach(SDL_DisplayMode mode in displayModes)
		{
			DisplayModes.Add(new()
			{
				Width = mode.w,
				Height = mode.h,
				RefreshRate = mode.refresh_rate,
			});
		}

		displayModes.Dispose();
	}

	
	/// <summary>
	/// Set the window to exclusive fullscreen mode.
	/// </summary>
	/// <param name="displayMode"><see cref="Jackal.Rendering.DisplayMode" /> to use.</param>
	/// <exception cref="Exception">Thrown if fails to set mode.</exception>
	public void SetExclusiveFullscreen(DisplayMode displayMode)
	{
		SDL_DisplayMode closestMode;
		if(!SDL3.SDL_GetClosestFullscreenDisplayMode(SDL3.SDL_GetDisplayForWindow(Window),
			displayMode.Width, displayMode.Height, displayMode.RefreshRate, true, &closestMode))
		{
			throw new GameWindowException("Failed to find suitable fullscreen mode.");
		}

		if(!SDL3.SDL_SetWindowFullscreenMode(Window, &closestMode))
		{
			
			throw new GameWindowException("Failed to set fullscreen mode.");
		}

		if(!SDL3.SDL_SetWindowFullscreen(Window, true))
		{
			throw new GameWindowException("Failed to go fullscreen.");
		}

		SDL3.SDL_SyncWindow(Window);

	}

	/// <summary>
	/// Set the window to borderless fullscreen mode.
	/// </summary>
	/// <exception cref="Exception">Thrown if fails to set mode.</exception>
	public void SetBorderlessFullscreen()
	{
		SDL_DisplayID display = SDL3.SDL_GetDisplayForWindow(Window);
		SDL_Rect bounds;
		if(!SDL3.SDL_GetDisplayBounds(display, &bounds))
		{
			throw new GameWindowException("Failed to get display bounds.");
		}

		if(!SDL3.SDL_SetWindowFullscreen(Window, false))
		{
			throw new GameWindowException("Failed to go windowed.");
		}

		SDL3.SDL_SetWindowSize(Window, bounds.w, bounds.h);
		SDL3.SDL_SetWindowPosition(Window, bounds.x, bounds.y);
		SDL3.SDL_SetWindowBordered(Window, false);

		SDL3.SDL_SyncWindow(Window);
	}

	/// <summary>
	/// Set the window to windowed mode.
	/// </summary>
	/// <exception cref="Exception">Thrown if fails to set mode.</exception>
	public void SetWindowed()
	{
		if(!SDL3.SDL_SetWindowFullscreen(Window, false))
		{
			throw new GameWindowException("Failed to go windowed.");
		}

		SDL3.SDL_SetWindowSize(Window, WindowSettings.Width, WindowSettings.Height);
		SDL3.SDL_SetWindowPosition(Window, (int)SDL3.SDL_WINDOWPOS_CENTERED, (int)SDL3.SDL_WINDOWPOS_CENTERED);
		SDL3.SDL_SetWindowBordered(Window, true);

		SDL3.SDL_SyncWindow(Window);
	}

	/// <summary>
	/// Dispose the game window.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Dispose the game window.
	/// </summary>
	protected virtual void Dispose(bool disposing)
	{
		if(_disposed || !disposing)
		{
			return;
		}

		SDL3.SDL_GL_DestroyContext(_glContext);
		SDL3.SDL_DestroyWindow(Window);
		_glContext = null;
		Window = null;

		SDL3.SDL_Quit();
		_disposed = true;
	}

	/// <summary>
	/// Destructor for GameWindow class.
	/// </summary>
	~GameWindow()
	{
		#if DEBUG
		Console.WriteLine("Leak! Did you forget to call dispose?");
		Debugger.Launch();
		#endif

		Dispose(false);
	}
}