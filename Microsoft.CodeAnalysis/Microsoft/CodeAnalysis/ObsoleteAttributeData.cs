namespace Microsoft.CodeAnalysis;

internal sealed class ObsoleteAttributeData
{
	public static readonly ObsoleteAttributeData Uninitialized = new ObsoleteAttributeData(ObsoleteAttributeKind.Uninitialized, null, isError: false);

	public static readonly ObsoleteAttributeData Experimental = new ObsoleteAttributeData(ObsoleteAttributeKind.Experimental, null, isError: false);

	public readonly ObsoleteAttributeKind Kind;

	public readonly bool IsError;

	public readonly string Message;

	internal bool IsUninitialized => this == Uninitialized;

	public ObsoleteAttributeData(ObsoleteAttributeKind kind, string message, bool isError)
	{
		Kind = kind;
		Message = message;
		IsError = isError;
	}
}
