using System;

namespace Jackal.Exceptions;

/// <summary>
/// Represents an error that occurs during vertex buffer handling.
/// </summary>
[Serializable]
public class VertexArrayException : Exception
{
	/// <summary>
	/// Initializes a new instance of VertexArrayException class.
	/// </summary>
	public VertexArrayException()
	{
	}

	/// <summary>
	/// Initializes a new instance of VertexArrayException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public VertexArrayException(string message)
	: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of VertexArrayException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference.</param>
	public VertexArrayException(string message, Exception inner)
	: base(message, inner)
	{
	}
}