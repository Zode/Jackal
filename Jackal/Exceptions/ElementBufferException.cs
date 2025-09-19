using System;

namespace Jackal.Exceptions;

/// <summary>
/// Represents an error that occurs during element buffer handling.
/// </summary>
[Serializable]
public class ElementBufferException : Exception
{
	/// <summary>
	/// Initializes a new instance of ElementBufferException class.
	/// </summary>
	public ElementBufferException()
	{
	}

	/// <summary>
	/// Initializes a new instance of ElementBufferException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public ElementBufferException(string message)
	: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of ElementBufferException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference.</param>
	public ElementBufferException(string message, Exception inner)
	: base(message, inner)
	{
	}
}