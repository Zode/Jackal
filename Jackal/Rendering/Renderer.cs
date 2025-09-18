using System;
using SDL;

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public static class Renderer
{
	/// <summary>
	/// Should rendering have vsync enabled?
	/// </summary>
	public static VSyncMode VSync
	{
		get => _vSync;
		set
		{
			_vSync = value;
			switch(value)
			{
				case VSyncMode.Adaptive:
					if(!SDL3.SDL_GL_SetSwapInterval(-1))
					{
						_vSync = VSyncMode.Enabled;
						SDL3.SDL_GL_SetSwapInterval(1);
					}

					break;

				case VSyncMode.Enabled:
					SDL3.SDL_GL_SetSwapInterval(1);
					break;

				default:
				case VSyncMode.Disabled:
					SDL3.SDL_GL_SetSwapInterval(0);
					break;
			}
		}
	}
	private static VSyncMode _vSync = VSyncMode.Enabled;
	/// <summary>
	/// Rendering framerate cap. Set to 0 to disable.
	/// </summary>
	public static uint FrameRateCap {get; set;} = 120;
	/// <summary>
	/// The time in ms it took to render the previous render frame.
	/// </summary>
	public static float FrameTime {get; private set;} = 0;
	/// <summary>
	/// Get the FPS of the previous render frame.
	/// </summary>
	public static int FPS => FrameTime > 0.0f ? (int)MathF.Floor(1000.0f / FrameTime) : 0;
	private static ulong _frameStartTime = 0;

	/// <summary>
	/// </summary>
	/// <param name="frameStartTime"></param>
	internal static void Process(ulong frameStartTime)
	{
		Engine.GameWindow?.OnRenderFrame();
		_frameStartTime = frameStartTime;
	}

	/// <summary>
	/// Handles the frame rate cap if necessary.
	/// </summary>
	internal static void HandleFrameRateCap()
	{
		ulong time = SDL3.SDL_GetPerformanceCounter();
		FrameTime = (float)(time - _frameStartTime) * 1000.0f / Engine.TickFrequency;
		if(FrameRateCap == 0)
		{
			return;
		}

		ulong capTime = Engine.TickFrequency / (ulong)(FrameRateCap + 1);
		if(time - _frameStartTime < capTime)
		{
			SDL3.SDL_DelayNS(capTime - (time - _frameStartTime));
			FrameTime = (float)(SDL3.SDL_GetPerformanceCounter() - _frameStartTime) * 1000.0f / Engine.TickFrequency;
		}
	}

	/// <summary>
	/// Called when the engine is starting, before any game code.
	/// </summary>
	internal static void Start()
	{

	}

	/// <summary>
	/// Called when the engine is shutting down, after any game code.
	/// </summary>
	internal static void Shutdown()
	{

	}
}