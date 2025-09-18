using System;

namespace Jackal.Exceptions;

/// <summary>
/// Represents an error that occurs during shader handling.
/// </summary>
[Serializable]
public class ShaderException : Exception
{
	/// <summary>
	/// Initializes a new instance of ShaderException class.
	/// </summary>
	public ShaderException()
	{
	}

	/// <summary>
	/// Initializes a new instance of ShaderException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public ShaderException(string message)
	: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of ShaderException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference.</param>
	public ShaderException(string message, Exception inner)
	: base(message, inner)
	{
	}
}