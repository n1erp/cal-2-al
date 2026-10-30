using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class RefKindExtensions
{
	public static bool IsManagedReference(this RefKind refKind)
	{
		return refKind != RefKind.None;
	}

	public static RefKind GetRefKind(this SyntaxKind syntaxKind)
	{
		return syntaxKind switch
		{
			SyntaxKind.RefKeyword => RefKind.Ref, 
			SyntaxKind.OutKeyword => RefKind.Out, 
			SyntaxKind.InKeyword => RefKind.In, 
			SyntaxKind.None => RefKind.None, 
			_ => throw ExceptionUtilities.UnexpectedValue(syntaxKind), 
		};
	}

	public static bool IsWritableReference(this RefKind refKind)
	{
		switch (refKind)
		{
		case RefKind.Ref:
		case RefKind.Out:
			return true;
		case RefKind.None:
		case RefKind.In:
			return false;
		default:
			throw ExceptionUtilities.UnexpectedValue(refKind);
		}
	}
}
