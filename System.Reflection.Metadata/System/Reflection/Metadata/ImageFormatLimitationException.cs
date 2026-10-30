namespace System.Reflection.Metadata;

public class ImageFormatLimitationException : Exception
{
	public ImageFormatLimitationException()
	{
	}

	public ImageFormatLimitationException(string message)
		: base(message)
	{
	}

	public ImageFormatLimitationException(string message, Exception innerException)
		: base(message, innerException)
	{
	}
}
