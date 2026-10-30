using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class ExternAliasDirectiveSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken externKeyword;

	internal readonly SyntaxToken aliasKeyword;

	internal readonly SyntaxToken identifier;

	internal readonly SyntaxToken semicolonToken;

	public SyntaxToken ExternKeyword => externKeyword;

	public SyntaxToken AliasKeyword => aliasKeyword;

	public SyntaxToken Identifier => identifier;

	public SyntaxToken SemicolonToken => semicolonToken;

	internal ExternAliasDirectiveSyntax(SyntaxKind kind, SyntaxToken externKeyword, SyntaxToken aliasKeyword, SyntaxToken identifier, SyntaxToken semicolonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(externKeyword);
		this.externKeyword = externKeyword;
		AdjustFlagsAndWidth(aliasKeyword);
		this.aliasKeyword = aliasKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ExternAliasDirectiveSyntax(SyntaxKind kind, SyntaxToken externKeyword, SyntaxToken aliasKeyword, SyntaxToken identifier, SyntaxToken semicolonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(externKeyword);
		this.externKeyword = externKeyword;
		AdjustFlagsAndWidth(aliasKeyword);
		this.aliasKeyword = aliasKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal ExternAliasDirectiveSyntax(SyntaxKind kind, SyntaxToken externKeyword, SyntaxToken aliasKeyword, SyntaxToken identifier, SyntaxToken semicolonToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(externKeyword);
		this.externKeyword = externKeyword;
		AdjustFlagsAndWidth(aliasKeyword);
		this.aliasKeyword = aliasKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
		AdjustFlagsAndWidth(semicolonToken);
		this.semicolonToken = semicolonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)externKeyword, 
			1 => aliasKeyword, 
			2 => identifier, 
			3 => semicolonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.ExternAliasDirectiveSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitExternAliasDirective(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitExternAliasDirective(this);
	}

	public ExternAliasDirectiveSyntax Update(SyntaxToken externKeyword, SyntaxToken aliasKeyword, SyntaxToken identifier, SyntaxToken semicolonToken)
	{
		if (externKeyword != ExternKeyword || aliasKeyword != AliasKeyword || identifier != Identifier || semicolonToken != SemicolonToken)
		{
			ExternAliasDirectiveSyntax externAliasDirectiveSyntax = SyntaxFactory.ExternAliasDirective(externKeyword, aliasKeyword, identifier, semicolonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				externAliasDirectiveSyntax = externAliasDirectiveSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				externAliasDirectiveSyntax = externAliasDirectiveSyntax.WithAnnotationsGreen(annotations);
			}
			return externAliasDirectiveSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new ExternAliasDirectiveSyntax(Kind, externKeyword, aliasKeyword, identifier, semicolonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new ExternAliasDirectiveSyntax(Kind, externKeyword, aliasKeyword, identifier, semicolonToken, GetDiagnostics(), annotations);
	}

	internal ExternAliasDirectiveSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			externKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			aliasKeyword = syntaxToken2;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			identifier = syntaxToken3;
		}
		SyntaxToken syntaxToken4 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken4 != null)
		{
			AdjustFlagsAndWidth(syntaxToken4);
			semicolonToken = syntaxToken4;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(externKeyword);
		writer.WriteValue(aliasKeyword);
		writer.WriteValue(identifier);
		writer.WriteValue(semicolonToken);
	}

	static ExternAliasDirectiveSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(ExternAliasDirectiveSyntax), (ObjectReader r) => new ExternAliasDirectiveSyntax(r));
	}
}
