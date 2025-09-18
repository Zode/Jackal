using System;
using Jackal;
using Jackal.Rendering;
using Jackal.Input;

namespace Workbench;

public class Window : GameWindow
{
	public Window()
	{
		WindowSettings = new()
		{
			Title = "Workbench",
			Width = 800,
			Height = 600,
			WindowFlags = WindowFlags.Resizeable,
		};
	}

	public override void OnStart()
	{
		//default in the engine, but modifiable to suit per game needs.
		Engine.TicksPerSecond = 60;
		//defaults in the engine, but these could be loaded from settings here:
		Renderer.FrameRateCap = 120;
		Renderer.VSync = VSyncMode.Enabled;
		//also similarly can be set here:
		SetWindowed();
		//SetExclusiveFullscreen(DisplayModes[0]);
		//SetBorderlessFullscreen();
	}

	public override void OnMouseFocusChanged(bool mouseInWindow)
	{
		Console.WriteLine($"Mouse in window: {mouseInWindow}");
	}

	public override void OnKeyboardFocusChanged(bool focus)
	{
		Console.WriteLine($"Keyboard focus: {focus}");
	}

	public override void OnWindowResized(int width, int height)
	{
		Console.WriteLine($"Resize: {width}x{height}");
	}

	public override void OnTickUpdate()
	{
		//Console.WriteLine($"Logic tick time: {Engine.TickTime} (loops: {Engine.Ticks}, fixed delta time: {Engine.FixedDeltaTime})");
	}

	public override void OnRenderFrame()
	{
		//Console.WriteLine($"Render frame time: {Renderer.FrameTime} ({Renderer.FPS} fps) (vsync: {Renderer.VSync}, framecap: {Renderer.FrameRateCap})");
	}
}