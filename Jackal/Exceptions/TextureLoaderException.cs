using System;

namespace Jackal.Exceptions;

/// <summary>
/// Represents an error that occurs during texture loading.
/// </summary>
[Serializable]
public class TextureLoaderException : Exception
{
	/// <summary>
	/// Initializes a new instance of TextureLoaderException class.
	/// </summary>
	public TextureLoaderException()
	{
	}

	/// <summary>
	/// Initializes a new instance of TextureLoaderException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	public TextureLoaderException(string message)
	: base(message)
	{
	}

	/// <summary>
	/// Initializes a new instance of TextureLoaderException class.
	/// </summary>
	/// <param name="message">The message that describes the error.</param>
	/// <param name="inner">The exception that is the cause of the current exception, or a null reference.</param>
	public TextureLoaderException(string message, Exception inner)
	: base(message, inner)
	{
	}
}