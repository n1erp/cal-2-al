using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class TypeSyntax : ExpressionSyntax
{
	public bool IsVar
	{
		get
		{
			if (this is IdentifierNameSyntax identifierNameSyntax)
			{
				return identifierNameSyntax.Identifier.ToString() == "var";
			}
			return false;
		}
	}

	public bool IsUnmanaged
	{
		get
		{
			if (this is IdentifierNameSyntax identifierNameSyntax)
			{
				return identifierNameSyntax.Identifier.ToString() == "unmanaged";
			}
			return false;
		}
	}

	internal TypeSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal TypeSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected TypeSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
