using System;
using Jackal;
using Jackal.Rendering;
using Jackal.Input;
using OpenTK.Graphics.OpenGL4;

namespace Workbench;

public unsafe class Window : GameWindow
{
	VertexArray vertArray;
	VertexBuffer vertBuffer;
	Shader shader;

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

		shader = Shader.FromString("""
		#version 460 core
		layout (location = 0) in vec3 Position;
		
		void main()
		{
			gl_Position = vec4(Position, 1.0f);
		}
		""","""
		#version 460 core
		out vec4 FragColor;

		void main()
		{
			FragColor = vec4(1.0f);
		}
		""");

		float[] vertices = {
			-0.5f, -0.5f, 0.0f,
			0.5f, -0.5f, 0.0f,
			0.0f, 0.5f, 0.0f,
		};

		vertArray = new();
		fixed(float* verts = vertices)
		{
			vertBuffer = new(VertexBufferType.Static, vertices.Length * sizeof(float), (IntPtr)verts);
		}

		VertexAttributeLayoutBuilder vertexLayoutBuilder = new();
		vertexLayoutBuilder.AddFloat(3).SetLayout();
	}

	public override bool OnExitRequested()
	{
		shader.Dispose();
		vertArray.Dispose();
		vertBuffer.Dispose();
		return true;
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
		shader.Bind();
		vertArray.Bind();
		GL.DrawArrays(PrimitiveType.Triangles, 0, 3);

		//Console.WriteLine($"Render frame time: {Renderer.FrameTime} ({Renderer.FPS} fps) (vsync: {Renderer.VSync}, framecap: {Renderer.FrameRateCap})");
	}
}