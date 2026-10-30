using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

internal sealed class IfDirectiveTriviaSyntax : ConditionalDirectiveTriviaSyntax
{
	internal readonly SyntaxToken hashToken;

	internal readonly SyntaxToken ifKeyword;

	internal readonly ExpressionSyntax condition;

	internal readonly SyntaxToken endOfDirectiveToken;

	internal readonly bool isActive;

	internal readonly bool branchTaken;

	internal readonly bool conditionValue;

	public override SyntaxToken HashToken => hashToken;

	public SyntaxToken IfKeyword => ifKeyword;

	public override ExpressionSyntax Condition => condition;

	public override SyntaxToken EndOfDirectiveToken => endOfDirectiveToken;

	public override bool IsActive => isActive;

	public override bool BranchTaken => branchTaken;

	public override bool ConditionValue => conditionValue;

	internal IfDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
		this.conditionValue = conditionValue;
	}

	internal IfDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue, SyntaxFactoryContext context)
		: base(kind)
	{
		SetFactoryContext(context);
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
		this.conditionValue = conditionValue;
	}

	internal IfDirectiveTriviaSyntax(SyntaxKind kind, SyntaxToken hashToken, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
		: base(kind)
	{
		SlotCount = 4;
		AdjustFlagsAndWidth(hashToken);
		this.hashToken = hashToken;
		AdjustFlagsAndWidth(ifKeyword);
		this.ifKeyword = ifKeyword;
		AdjustFlagsAndWidth(condition);
		this.condition = condition;
		AdjustFlagsAndWidth(endOfDirectiveToken);
		this.endOfDirectiveToken = endOfDirectiveToken;
		this.isActive = isActive;
		this.branchTaken = branchTaken;
		this.conditionValue = conditionValue;
	}

	internal override GreenNode GetSlot(int index)
	{
		return index switch
		{
			0 => (GreenNode)hashToken, 
			1 => ifKeyword, 
			2 => condition, 
			3 => endOfDirectiveToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new Microsoft.CodeAnalysis.CSharp.Syntax.IfDirectiveTriviaSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(CSharpSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitIfDirectiveTrivia(this);
	}

	public override void Accept(CSharpSyntaxVisitor visitor)
	{
		visitor.VisitIfDirectiveTrivia(this);
	}

	public IfDirectiveTriviaSyntax Update(SyntaxToken hashToken, SyntaxToken ifKeyword, ExpressionSyntax condition, SyntaxToken endOfDirectiveToken, bool isActive, bool branchTaken, bool conditionValue)
	{
		if (hashToken != HashToken || ifKeyword != IfKeyword || condition != Condition || endOfDirectiveToken != EndOfDirectiveToken)
		{
			IfDirectiveTriviaSyntax ifDirectiveTriviaSyntax = SyntaxFactory.IfDirectiveTrivia(hashToken, ifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue);
			DiagnosticInfo[] diagnostics = GetDiagnostics();
			if (diagnostics != null && diagnostics.Length != 0)
			{
				ifDirectiveTriviaSyntax = ifDirectiveTriviaSyntax.WithDiagnosticsGreen(diagnostics);
			}
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				ifDirectiveTriviaSyntax = ifDirectiveTriviaSyntax.WithAnnotationsGreen(annotations);
			}
			return ifDirectiveTriviaSyntax;
		}
		return this;
	}

	internal override GreenNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new IfDirectiveTriviaSyntax(Kind, hashToken, ifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue, diagnostics, GetAnnotations());
	}

	internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new IfDirectiveTriviaSyntax(Kind, hashToken, ifKeyword, condition, endOfDirectiveToken, isActive, branchTaken, conditionValue, GetDiagnostics(), annotations);
	}

	internal IfDirectiveTriviaSyntax(ObjectReader reader)
		: base(reader)
	{
		SlotCount = 4;
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
			ifKeyword = syntaxToken2;
		}
		ExpressionSyntax expressionSyntax = (ExpressionSyntax)reader.ReadValue();
		if (expressionSyntax != null)
		{
			AdjustFlagsAndWidth(expressionSyntax);
			condition = expressionSyntax;
		}
		SyntaxToken syntaxToken3 = (SyntaxToken)reader.ReadValue();
		if (syntaxToken3 != null)
		{
			AdjustFlagsAndWidth(syntaxToken3);
			endOfDirectiveToken = syntaxToken3;
		}
		isActive = reader.ReadBoolean();
		branchTaken = reader.ReadBoolean();
		conditionValue = reader.ReadBoolean();
	}

	internal override void WriteTo(ObjectWriter writer)
	{
		base.WriteTo(writer);
		writer.WriteValue(hashToken);
		writer.WriteValue(ifKeyword);
		writer.WriteValue(condition);
		writer.WriteValue(endOfDirectiveToken);
		writer.WriteBoolean(isActive);
		writer.WriteBoolean(branchTaken);
		writer.WriteBoolean(conditionValue);
	}

	static IfDirectiveTriviaSyntax()
	{
		ObjectBinder.RegisterTypeReader(typeof(IfDirectiveTriviaSyntax), (ObjectReader r) => new IfDirectiveTriviaSyntax(r));
	}
}
