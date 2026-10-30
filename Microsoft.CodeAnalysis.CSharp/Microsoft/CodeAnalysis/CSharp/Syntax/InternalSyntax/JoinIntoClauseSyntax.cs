using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class JoinIntoClauseSyntax : CSharpSyntaxNode
{
	internal readonly SyntaxToken intoKeyword;

	internal readonly SyntaxToken identifier;

	public SyntaxToken IntoKeyword => intoKeyword;

	public SyntaxToken Identifier => identifier;

	internal JoinIntoClauseSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal JoinIntoClauseSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 2;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal JoinIntoClauseSyntax(SyntaxKind kind, SyntaxToken intoKeyword, SyntaxToken identifier)
		: base(kind)
	{
		SlotCount = 2;
		AdjustFlagsAndWidth(intoKeyword);
		this.intoKeyword = intoKeyword;
		AdjustFlagsAndWidth(identifier);
		this.identifier = identifier;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)intoKeyword, 
			1 => identifier, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.JoinIntoClauseSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitJoinIntoClause(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitJoinIntoClause(this);
	}

	public JoinIntoClauseSyntax Update(SyntaxToken intoKeyword, SyntaxToken identifier)
	{
		if (intoKeyword != IntoKeyword || identifier != Identifier)
		{
			JoinIntoClauseSyntax joinIntoClauseSyntax = SyntaxFactory.JoinIntoClause(intoKeyword, identifier);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				joinIntoClauseSyntax = joinIntoClauseSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				joinIntoClauseSyntax = joinIntoClauseSyntax.WithAnnotationsGreen(annotations);
			}
			return joinIntoClauseSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new JoinIntoClauseSyntax(Kind, intoKeyword, identifier, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new JoinIntoClauseSyntax(Kind, intoKeyword, identifier, GetDiagnostics(), annotations);
	}

	internal JoinIntoClauseSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 2;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			intoKeyword = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			identifier = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(intoKeyword);
		writer.WriteValue(identifier);
	}

	static JoinIntoClauseSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(JoinIntoClauseSyntax), (ObjectReader r) => new JoinIntoClauseSyntax(r));
	}
}
