using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal abstract class VariableDesignationSyntax : CSharpSyntaxNode
{
	internal VariableDesignationSyntax(SyntaxKind kind, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
	}

	internal VariableDesignationSyntax(SyntaxKind kind)
		: base(kind)
	{
	}

	protected VariableDesignationSyntax(ObjectReader reader)
		: base(reader)
	{
	}
}
