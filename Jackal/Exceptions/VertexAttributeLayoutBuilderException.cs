using System;

namespace Jackal.Exceptions;

/// <summary>
/// Represents an error that occurs during vertex attribute layout handling.
/// </summary>
[Serializable]
public class VertexAttributeLayoutException : Exception
{
	/// <summary>
	/// Initializes a new instance of VertexAttributeLayoutException class.
	/// </summary>
	public VertexAttributeLayoutException()
	{
	}

	/// <summary>
	/// Initializes a new instance of VertexAttributeLayoutException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public VertexAttributeLayoutException(string message)
	: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of VertexAttributeLayoutException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference.</param>
	public VertexAttributeLayoutException(string message, Exception inner)
	: base(message, inner)
	{
	}
}