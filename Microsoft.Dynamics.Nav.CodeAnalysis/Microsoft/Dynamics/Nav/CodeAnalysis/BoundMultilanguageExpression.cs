using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundMultilanguageExpression : BoundExpression
{
	public BoundExpression LanguageId { get; private set; }

	public ConstantValue LanguageCultureId { get; private set; }

	public ConstantValue LanguageValue { get; private set; }

	protected override OperationKind ExpressionKind => OperationKind.None;

	public BoundMultilanguageExpression(SyntaxNode syntax, BoundExpression languageId, ConstantValue languageCultureId, ConstantValue languageValue, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.MultilanguageExpression, syntax, type, hasErrors || languageId.HasErrors())
	{
		LanguageId = languageId;
		LanguageCultureId = languageCultureId;
		LanguageValue = languageValue;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMultilanguageExpression(this);
	}

	public BoundMultilanguageExpression Update(BoundExpression languageId, ConstantValue languageCultureId, ConstantValue languageValue, TypeSymbol type)
	{
		if (languageId != LanguageId || languageCultureId != LanguageCultureId || languageValue != LanguageValue || type != Type)
		{
			return new BoundMultilanguageExpression(Syntax, languageId, languageCultureId, languageValue, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}

	public override void Accept(OperationVisitor visitor)
	{
		visitor.VisitNoneOperation(this);
	}

	public override TResult Accept<TArgument, TResult>(OperationVisitor<TArgument, TResult> visitor, TArgument argument)
	{
		return visitor.VisitNoneOperation(this, argument);
	}
}
