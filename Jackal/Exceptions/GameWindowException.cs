using System;

namespace Jackal.Exceptions;

/// <summary>
/// Represents an error that occurs during game window handling.
/// </summary>
[Serializable]
public class GameWindowException : Exception
{
	/// <summary>
	/// Initializes a new instance of GameWindowException class.
	/// </summary>
	public GameWindowException()
	{
	}

	/// <summary>
	/// Initializes a new instance of GameWindowException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public GameWindowException(string message)
	: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of GameWindowException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference.</param>
	public GameWindowException(string message, Exception inner)
	: base(message, inner)
	{
	}
}