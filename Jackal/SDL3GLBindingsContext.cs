using System;
using OpenTK;
using SDL;

namespace Jackal;

/// <summary>
/// Provides the necessary interface to bridge OpenTK bindings with SDL3 OpenGL context.
/// </summary>
public unsafe class SDL3GLBindingsContext : IBindingsContext
{
	IntPtr IBindingsContext.GetProcAddress(string procName)
	{
		return SDL3.SDL_GL_GetProcAddress(procName);
	}
}