using System;
using SDL;
using OpenTK.Graphics.OpenGL4;
using Jackal.FileFormats;

#if DEBUG
using System.Runtime.InteropServices;
#endif

namespace Jackal.Rendering;

/// <summary>
/// </summary>
public unsafe static class Renderer
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
	/// The time in milliseconds it took to render the previous render frame.
	/// </summary>
	public static float FrameTime {get; private set;} = 0;
	/// <summary>
	/// Get the FPS of the previous render frame.
	/// </summary>
	public static int FPS => FrameTime > 0.0f ? (int)MathF.Floor(1000.0f / FrameTime) : 0;
	private static ulong _frameStartTime = 0;
	internal static int MaxVertexAttributes {get; private set;} = 0;
	/// <summary>
	/// Maximum allowed texture anisotropy level for the OpenGL driver.
	/// </summary>
	public static TextureAnisotropy MaximumTextureAnisotropy {get; private set;} = TextureAnisotropy.Zero;
	/// <summary>
	/// The global texture anisotropy.
	/// </summary>
	public static TextureAnisotropy TextureAnisotropy {get; set;} = TextureAnisotropy.Sixteen;
	/// <summary>
	/// The global texture filter.
	/// </summary>
	public static TextureFilter TextureFilter {get; set;} = TextureFilter.LinearMipLinear;
	/// <summary>
	/// Maximum allowed texture size for the OpenGL driver.
	/// </summary>
	public static int MaxTextureSize {get; private set;} = 0;
	/// <summary>
	/// Maximum allowed 3d texture size for the OpenGL driver.
	/// </summary>
	public static int Max3DTextureSize {get; private set;} = 0;
	/// <summary>
	/// Maximum allowed cubemap texture size for the OpenGL driver.
	/// </summary>
	public static int MaxCubemapTextureSize {get; private set;} = 0;
	/// <summary>
	/// Maximum allowed array levels in the OpenGL driver.
	/// </summary>
	public static int MaxArrayTextureLevels {get; private set;} = 0;
	
	/// <summary>
	/// </summary>
	/// <param name="frameStartTime"></param>
	internal static void Process(ulong frameStartTime)
	{
		GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
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
		if(Engine.GameWindow is null)
		{
			return;
		}

		#if DEBUG
		GL.DebugMessageCallback(_GLDebugMessageDelegate, IntPtr.Zero);
		GL.Enable(EnableCap.DebugOutput);
		GL.Enable(EnableCap.DebugOutputSynchronous);
		#endif

		Texture.AddTextureLoader(".png", typeof(STBTextureLoader));
		Texture.AddTextureLoader(".tga", typeof(STBTextureLoader));
		Texture.AddTextureLoader(".bmp", typeof(STBTextureLoader));
		Texture.AddTextureLoader(".jpg", typeof(STBTextureLoader));
		Texture.AddTextureLoader(".jpeg", typeof(STBTextureLoader));

		int glint = 0;
		GL.GetInteger(GetPName.MaxVertexAttribs, &glint);
		MaxVertexAttributes = glint;

		float maxAnisotropy = 0;
		GL.GetFloat(GetPName.MaxTextureMaxAnisotropy, &maxAnisotropy);
		MaximumTextureAnisotropy = TextureAnisotropy.FromFloat(maxAnisotropy);

		GL.GetInteger(GetPName.MaxTextureSize, &glint);
		MaxTextureSize = glint;
		GL.GetInteger(GetPName.Max3DTextureSize, &glint);
		Max3DTextureSize = glint;
		GL.GetInteger(GetPName.MaxCubeMapTextureSize, &glint);
		MaxCubemapTextureSize = glint;
		GL.GetInteger(GetPName.MaxArrayTextureLayers, &glint);
		MaxArrayTextureLevels = glint;

		GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
		ResizeViewport(Engine.GameWindow.WindowSettings.Width, Engine.GameWindow.WindowSettings.Height);
	}

	/// <summary>
	/// Called when the engine is starting, after any game code.
	/// </summary>
	internal static void PostStart()
	{
		TextureAnisotropy = TextureAnisotropy.Clamp(MaximumTextureAnisotropy);
	}

	/// <summary>
	/// Resize the viewport.
	/// </summary>
	/// <param name="width">New width.</param>
	/// <param name="height">New height.</param>
	internal static void ResizeViewport(int width, int height)
	{
		GL.Viewport(0, 0, width, height);
	}

	#if DEBUG
	private static DebugProc _GLDebugMessageDelegate = OnGLDebugMessage;
	private static void OnGLDebugMessage(DebugSource source, DebugType type, int id, DebugSeverity severity, int length, IntPtr messagePtr, IntPtr userParamPtr)
	{
		string message = Marshal.PtrToStringAnsi(messagePtr, length);
		message = $"GL DEBUG severity:{severity}\tsource:{source}\ttype:{type}\tid:{id}\n{message}";
		Console.WriteLine(message);
		if(type == DebugType.DebugTypeError)
		{
			throw new Exception(message);
		}
	}
	#endif

	/// <summary>
	/// Called when the engine is shutting down, after any game code.
	/// </summary>
	internal static void Shutdown()
	{

	}
}