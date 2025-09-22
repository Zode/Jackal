using System;
using Jackal;
using Jackal.Rendering;
using Jackal.Input;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Workbench;

struct Vertex(float x, float y, float z, float u, float v)
{
	public float X {get;} = x;
	public float Y {get;} = y;
	public float Z {get;} = z;
	public float U {get;} = u;
	public float V {get;} = v;
};

public unsafe class Window : GameWindow
{
	VertexArray vertArray;
	VertexBuffer<Vertex> vertBuffer;
	ElementBuffer elementBuffer;
	Texture texture;
	Shader shader;
	Vector3 color = Vector3.One;

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

		Renderer.TextureFilter = TextureFilter.LinearMipLinear;
		Renderer.TextureAnisotropy = TextureAnisotropy.Sixteen;

		shader = Shader.FromString("""
		#version 460 core
		layout (location = 0) in vec3 aPosition;
		layout (location = 1) in vec2 aTexCoord;

		out vec2 TexCoord;
		
		void main()
		{
			TexCoord = aTexCoord;
			gl_Position = vec4(aPosition, 1.0f);
		}
		""","""
		#version 460 core
		out vec4 FragColor;

		in vec2 TexCoord;

		uniform sampler3D texture0;

		void main()
		{
			FragColor = texture(texture0, vec3(TexCoord.x, TexCoord.y, 0.0f));
		}
		""");

		Vertex[] vertices = [
			new(-0.5f, 0.5f, 0.0f, 0.0f, 1.0f),
			new(0.5f, 0.5f, 0.0f, 1.0f, 1.0f),
			new(-0.5f, -0.5f, 0.0f, 0.0f, 0.0f),
			new(0.5f, -0.5f, 0.0f, 1.0f, 0.0f),
		];

		uint[] indices = [
			0, 1, 3,
			0, 2, 3,
		];

		texture = Texture.FromFiles(["/home/zode/temp/test2.png", "/home/zode/temp/test2.png", "/home/zode/temp/test2.png"], new(){
			TextureType = TextureType.ThreeDimensional,
			TextureWrap = TextureWrap.Repeat,
			TextureFilterOverride = TextureFilter.None,
			TextureAnisotropyOverride = TextureAnisotropy.None,
			Mipmaps = true,
		});

		vertBuffer = new(BufferType.Static, vertices);
		elementBuffer = new(BufferType.Static, indices);
		vertArray = new();

		VertexAttributeLayoutBuilder vertexLayoutBuilder = new();
		vertexLayoutBuilder.AddFloat(3).AddFloat(2).SetLayout(vertArray, vertBuffer, elementBuffer);
	}

	public override bool OnExitRequested()
	{
		return true;
	}

	public override void OnShutdown()
	{
		texture.Dispose();
		shader.Dispose();
		vertArray.Dispose();
		vertBuffer.Dispose();
		elementBuffer.Dispose();
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
		texture.Bind(0);
		shader.Bind();
		color.X = MathF.Abs(MathF.Sin(Engine.TimeF));
		color.Y = MathF.Abs(MathF.Sin(Engine.TimeF / 1.22f));
		color.Y = MathF.Abs(MathF.Sin(Engine.TimeF / 3.460f));
		shader.SetUniform3(shader.GetUniformLocation("Color"), color);
		vertArray.Bind();
		elementBuffer.Draw(Jackal.Rendering.PrimitiveType.Triangles);

		//Console.WriteLine($"Render frame time: {Renderer.FrameTime} ({Renderer.FPS} fps) (vsync: {Renderer.VSync}, framecap: {Renderer.FrameRateCap})");
	}
}