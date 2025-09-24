using System;

namespace Jackal.Exceptions;

/// <summary>
/// Represents an error that occurs during material handling.
/// </summary>
[Serializable]
public class MaterialException : Exception
{
	/// <summary>
	/// Initializes a new instance of MaterialException class.
	/// </summary>
	public MaterialException()
	{
	}

	/// <summary>
	/// Initializes a new instance of MaterialException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public MaterialException(string message)
	: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of MaterialException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference.</param>
	public MaterialException(string message, Exception inner)
	: base(message, inner)
	{
	}
}