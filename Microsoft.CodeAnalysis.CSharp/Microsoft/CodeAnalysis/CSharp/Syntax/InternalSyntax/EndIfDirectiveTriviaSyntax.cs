using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class EndIfDirectiveTriviaSyntax : DirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken endIfKeyword;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken EndIfKeyword => endIfKeyword;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	internal EndIfDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken endIfKeyword, SyntaxToken endOfDirectiveToken, bool isActive, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(endIfKeyword);
		this.endIfKeyword = endIfKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal EndIfDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken endIfKeyword, SyntaxToken endOfDirectiveToken, bool isActive, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(endIfKeyword);
		this.endIfKeyword = endIfKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal EndIfDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken endIfKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
		: base(kind)
	{
		SlotCount = 3;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(endIfKeyword);
		this.endIfKeyword = endIfKeyword;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)hashToken, 
			1 => endIfKeyword, 
			2 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.EndIfDirectiveTriviaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitEndIfDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitEndIfDirectiveTrivia(this);
	}

	public EndIfDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken endIfKeyword, SyntaxToken endOfDirectiveToken, bool isActive)
	{
		if (hashToken != HashToken || endIfKeyword != EndIfKeyword || endOfDirectiveToken != EndOfDirectiveToken)
		{
			EndIfDirectiveTriviaSyntax endIfDirectiveTriviaSyntax = SyntaxFactory.EndIfDirectiveTrivia(hashToken, endIfKeyword, endOfDirectiveToken, isActive);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				endIfDirectiveTriviaSyntax = endIfDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				endIfDirectiveTriviaSyntax = endIfDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return endIfDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new EndIfDirectiveTriviaSyntax(Kind, hashToken, endIfKeyword, endOfDirectiveToken, isActive, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new EndIfDirectiveTriviaSyntax(Kind, hashToken, endIfKeyword, endOfDirectiveToken, isActive, GetDiagnostics(), annotations);
	}

	internal EndIfDirectiveTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 3;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			hashToken = syntaxToken;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			endIfKeyword = syntaxToken2;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			endOfDirectiveToken = syntaxToken3;
		}
		isActive = reader.ReadBoolean();
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(hashToken);
		writer.WriteValue(endIfKeyword);
		writer.WriteValue(endOfDirectiveToken);
		writer.WriteBoolean(isActive);
	}

	static EndIfDirectiveTriviaSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(EndIfDirectiveTriviaSyntax), (ObjectReader r) => new EndIfDirectiveTriviaSyntax(r));
	}
}
