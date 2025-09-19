using System;
using Jackal;
using Jackal.Rendering;
using Jackal.Input;
using OpenTK.Graphics.OpenGL4;

namespace Workbench;

struct Vertex(float x, float y, float z)
{
	public float X {get;} = x;
	public float Y {get;} = y;
	public float Z {get;} = z;
};

public unsafe class Window : GameWindow
{
	VertexArray vertArray;
	VertexBuffer<Vertex> vertBuffer;
	ElementBuffer elementBuffer;
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

		Vertex[] vertices = [
			new(-0.5f, 0.5f, 0.0f),
			new(0.5f, 0.5f, 0.0f),
			new(-0.5f, -0.5f, 0.0f),
			new(0.5f, -0.5f, 0.0f),
		];

		uint[] indices = [
			0, 1, 3,
			0, 2, 3,
		];

		vertArray = new();
		vertBuffer = new(BufferType.Static, vertices);
		elementBuffer = new(BufferType.Static, indices);

		VertexAttributeLayoutBuilder vertexLayoutBuilder = new();
		vertexLayoutBuilder.AddFloat(3).SetLayout();
	}

	public override bool OnExitRequested()
	{
		shader.Dispose();
		vertArray.Dispose();
		vertBuffer.Dispose();
		elementBuffer.Dispose();
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
		switch(elementBuffer.ElementBufferType)
		{
			case ElementBufferType.UnsignedByte:
				GL.DrawElements(PrimitiveType.Triangles, elementBuffer.Count, DrawElementsType.UnsignedByte, 0);
				break;
			case ElementBufferType.UnsignedShort:
				GL.DrawElements(PrimitiveType.Triangles, elementBuffer.Count, DrawElementsType.UnsignedShort, 0);
				break;
			case ElementBufferType.UnsignedInt:
				GL.DrawElements(PrimitiveType.Triangles, elementBuffer.Count, DrawElementsType.UnsignedInt, 0);
				break;
		}

		//Console.WriteLine($"Render frame time: {Renderer.FrameTime} ({Renderer.FPS} fps) (vsync: {Renderer.VSync}, framecap: {Renderer.FrameRateCap})");
	}
}