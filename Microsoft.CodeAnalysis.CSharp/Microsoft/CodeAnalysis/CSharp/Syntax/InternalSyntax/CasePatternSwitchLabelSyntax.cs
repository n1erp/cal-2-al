using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class CasePatternSwitchLabelSyntax : SwitchLabelSyntax
{
	internal readonly SyntaxToken keyword;

	internal readonly PatternSyntax pattern;

	internal readonly WhenClauseSyntax whenClause;

	internal readonly SyntaxToken colonToken;

	public override SyntaxToken Keyword => keyword;

	public PatternSyntax Pattern => pattern;

	public WhenClauseSyntax WhenClause => whenClause;

	public override SyntaxToken ColonToken => colonToken;

	internal CasePatternSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken colonToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal CasePatternSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken colonToken, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal CasePatternSwitchLabelSyntax(SyntaxKind kind, SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken colonToken)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(keyword);
		this.keyword = keyword;
		AdjustFlagsAndWidth(pattern);
		this.pattern = pattern;
		if (whenClause != null)
		{
			AdjustFlagsAndWidth(whenClause);
			this.whenClause = whenClause;
		}
		AdjustFlagsAndWidth(colonToken);
		this.colonToken = colonToken;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)keyword, 
			1 => pattern, 
			2 => whenClause, 
			3 => colonToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.CasePatternSwitchLabelSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitCasePatternSwitchLabel(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitCasePatternSwitchLabel(this);
	}

	public CasePatternSwitchLabelSyntax Update(SyntaxToken keyword, PatternSyntax pattern, WhenClauseSyntax whenClause, SyntaxToken colonToken)
	{
		if (keyword != Keyword || pattern != Pattern || whenClause != WhenClause || colonToken != ColonToken)
		{
			CasePatternSwitchLabelSyntax casePatternSwitchLabelSyntax = SyntaxFactory.CasePatternSwitchLabel(keyword, pattern, whenClause, colonToken);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				casePatternSwitchLabelSyntax = casePatternSwitchLabelSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				casePatternSwitchLabelSyntax = casePatternSwitchLabelSyntax.WithAnnotationsGreen(annotations);
			}
			return casePatternSwitchLabelSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new CasePatternSwitchLabelSyntax(Kind, keyword, pattern, whenClause, colonToken, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new CasePatternSwitchLabelSyntax(Kind, keyword, pattern, whenClause, colonToken, GetDiagnostics(), annotations);
	}

	internal CasePatternSwitchLabelSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
		SyntaxToken syntaxToken = (SyntaxToken)reader.ReadValue();
		if (syntaxToken != null)
		{
			AdjustFlagsAndWidth(syntaxToken);
			keyword = syntaxToken;
		}
		PatternSyntax patternSyntax = (PatternSyntax)reader.ReadValue();
		if (patternSyntax != null)
		{
			AdjustFlagsAndWidth(patternSyntax);
			pattern = patternSyntax;
		}
		WhenClauseSyntax whenClauseSyntax = (WhenClauseSyntax)reader.ReadValue();
		if (whenClauseSyntax != null)
		{
			AdjustFlagsAndWidth(whenClauseSyntax);
			whenClause = whenClauseSyntax;
		}
		SyntaxToken syntaxToken2 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken2 != null)
		{
			AdjustFlagsAndWidth(syntaxToken2);
			colonToken = syntaxToken2;
		}
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(keyword);
		writer.WriteValue(pattern);
		writer.WriteValue(whenClause);
		writer.WriteValue(colonToken);
	}

	static CasePatternSwitchLabelSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(CasePatternSwitchLabelSyntax), (ObjectReader r) => new CasePatternSwitchLabelSyntax(r));
	}
}
