namespace Jackal.Input;

[System.AttributeUsage(System.AttributeTargets.Field)]
internal class KeyStringAttribute(string @string) : System.Attribute
{
	public string String {get; private set;} = @string;
}

/// <summary>
/// </summary>
public static class KeyStringExtensions
{
	/// <summary>
	/// Converts the enum to a human readable string
	/// </summary>
	/// <param name="key"></param>
	/// <returns></returns>
	public static string GetKeyString(this KeyboardKey key)
	{
		System.Reflection.FieldInfo? fieldInfo = key.GetType()?.GetField(key.ToString());
		if(fieldInfo is null)
		{
			return string.Empty;
		}

		KeyStringAttribute[] attributes = (KeyStringAttribute[])fieldInfo.GetCustomAttributes(typeof(KeyStringAttribute), false);
		return attributes.Length > 0 ? attributes[0].String : string.Empty; 
	}

	/// <summary>
	/// Converts the enum to a human readable string
	/// </summary>
	/// <param name="button"></param>
	/// <returns></returns>
	public static string GetKeyString(this MouseButton button)
	{
		System.Reflection.FieldInfo? fieldInfo = button.GetType()?.GetField(button.ToString());
		if(fieldInfo is null)
		{
			return string.Empty;
		}

		KeyStringAttribute[] attributes = (KeyStringAttribute[])fieldInfo.GetCustomAttributes(typeof(KeyStringAttribute), false);
		return attributes.Length > 0 ? attributes[0].String : string.Empty; 
	}
}