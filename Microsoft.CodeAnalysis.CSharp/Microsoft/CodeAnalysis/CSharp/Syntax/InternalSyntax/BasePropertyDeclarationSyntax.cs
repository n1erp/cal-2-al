using Microsoft.CodeAnalysis.Syntax.InternalSyntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class BasePropertyDeclarationSyntax : MemberDeclarationSyntax
{
	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<AttributeListSyntax> AttributeLists { get; }

	public abstract Microsoft.CodeAnalysis.Syntax.InternalSyntax.SyntaxList<SyntaxToken> Modifiers { get; }

	public abstract TypeSyntax Type { get; }

	public abstract ExplicitInterfaceSpecifierSyntax ExplicitInterfaceSpecifier { get; }

	public abstract AccessorListSyntax AccessorList { get; }

	internal BasePropertyDeclarationSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal BasePropertyDeclarationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected BasePropertyDeclarationSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
