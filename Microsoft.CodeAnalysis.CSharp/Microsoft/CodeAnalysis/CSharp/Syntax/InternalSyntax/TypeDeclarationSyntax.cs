using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class TypeDeclarationSyntax : BaseTypeDeclarationSyntax
{
	public abstract SyntaxToken Keyword { get; }

	public abstract TypeParameterListSyntax TypeParameterList { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<TypeParameterConstraintClauseSyntax> ConstraintClauses { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<MemberDeclarationSyntax> Members { get; }

	internal TypeDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal TypeDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected TypeDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
