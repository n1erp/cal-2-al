namespace Microsoft.CodeAnalysis;

public enum RefKind : byte
{
	None = 0,
	Ref = 1,
	Out = 2,
	In = 3,
	RefReadOnly = In
}
