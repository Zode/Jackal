using SDL;
using Jackal.Rendering;
using Jackal.Input;

namespace Jackal;

/// <summary>
/// </summary>
public static class Engine
{
	private static ulong _tickTimeAccumulator = SDL3.SDL_GetPerformanceCounter();
	/// <summary>
	/// </summary>
	public static readonly ulong TickFrequency = SDL3.SDL_GetPerformanceFrequency();
	/// <summary>
	/// Determiners how many times in a second should the game logic execute.
	/// </summary>
	public static uint TicksPerSecond
	{
		get => _ticksPerSecond;
		set
		{
			_ticksPerSecond = value;
			TickDeltaTime = 1.0f / value;
			_tickTimeTarget = TickFrequency / (ulong)value;
		}
	}
	private static uint _ticksPerSecond = 60;
	/// <summary>
	/// The delta time for each tick.
	/// </summary>
	public static float TickDeltaTime {get; private set;} = 1.0f / _ticksPerSecond;
	private static ulong _tickTimeTarget = TickFrequency / (ulong)_ticksPerSecond;
	/// <summary>
	/// The time in milliseconds it took to process the game logic for previous logic tick.
	/// </summary>
	public static float TickTime {get; private set;} = 0.0f;
	/// <summary>
	/// How many game logic ticks were made previously.
	/// </summary>
	public static int Ticks {get; private set;} = 0;
	/// <summary>
	/// Time since engine start in milliseconds.
	/// </summary>
	public static ulong TimeMS => SDL3.SDL_GetTicks();
	/// <summary>
	/// Time since engine start in nanoseconds.
	/// </summary>
	public static ulong TimeNS => SDL3.SDL_GetTicksNS();
	/// <summary>
	/// Time since engine start in seconds.
	/// </summary>
	public static double TimeD => TimeMS / 1000.0f; 
	/// <summary>
	/// Time since engine start in seconds.
	/// </summary>
	public static float TimeF => TimeMS / 1000.0f; 
	/// <summary>
	/// Reference to the main game window.
	/// </summary>
	internal static GameWindow? GameWindow = null; 

	/// <summary>
	/// </summary>
	internal static void Process()
	{
		ulong frameStartTime = SDL3.SDL_GetPerformanceCounter();

		if(_tickTimeAccumulator - frameStartTime > _tickTimeTarget)
		{
			Inputs.Process();
			Ticks = 0;
			while(_tickTimeAccumulator - frameStartTime > _tickTimeTarget)
			{
				GameWindow?.OnTickUpdate();
				_tickTimeAccumulator += _tickTimeTarget;
				Ticks++;
			}

			Inputs.PostProcess();
			TickTime = (float)(SDL3.SDL_GetPerformanceCounter() - frameStartTime) * 1000.0f / TickFrequency;
		}

		Renderer.Process(frameStartTime);
	}

	/// <summary>
	/// Called when the engine is starting.
	/// </summary>
	internal static void Start()
	{
		Renderer.Start();
	}

	/// <summary>
	/// Called when the engine is shutting down.
	/// </summary>
	internal static void Shutdown()
	{
		Renderer.Shutdown();
	}
}