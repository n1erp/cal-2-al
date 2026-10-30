using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundMultilanguagePropertyValue : BoundPropertyValue
{
	private string lazyValueText;

	public ImmutableArray<BoundMultilanguageExpression> Value { get; private set; }

	internal override object ValueObject => Syntax.ToString();

	internal override string ValueText
	{
		get
		{
			if (lazyValueText == null)
			{
				string multilanguageString = PropertyValueTextHelper.GetMultilanguageString(Value.Select((BoundMultilanguageExpression expression) => PropertyValueTextHelper.GetMultilanguageStringItem(expression.LanguageId.ConstantValue.StringValue, expression.LanguageValue.StringValue)));
				Interlocked.Exchange(ref lazyValueText, multilanguageString);
			}
			return lazyValueText;
		}
	}

	public BoundMultilanguagePropertyValue(SyntaxNode syntax, ImmutableArray<BoundMultilanguageExpression> value, TypeSymbol type, bool hasErrors = false)
		: base(BoundKind.MultilanguagePropertyValue, syntax, type, hasErrors || value.HasErrors())
	{
		Value = value;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitMultilanguagePropertyValue(this);
	}

	public BoundMultilanguagePropertyValue Update(ImmutableArray<BoundMultilanguageExpression> value, TypeSymbol type)
	{
		if (value != Value || type != Type)
		{
			return new BoundMultilanguagePropertyValue(Syntax, value, type, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
