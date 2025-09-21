using System;

namespace Jackal.Exceptions;

/// <summary>
/// Represents an error that occurs during texture handling.
/// </summary>
[Serializable]
public class TextureException : Exception
{
	/// <summary>
	/// Initializes a new instance of TextureException class.
	/// </summary>
	public TextureException()
	{
	}

	/// <summary>
	/// Initializes a new instance of TextureException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public TextureException(string message)
	: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of TextureException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference.</param>
	public TextureException(string message, Exception inner)
	: base(message, inner)
	{
	}
}