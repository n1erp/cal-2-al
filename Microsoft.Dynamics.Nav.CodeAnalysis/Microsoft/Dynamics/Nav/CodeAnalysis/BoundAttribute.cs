using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class BoundAttribute : BoundNode
{
	internal string ValueText => ((MemberAttributeSyntax)Syntax).Name.ToString();

	public ImmutableArray<BoundExpression> Arguments { get; private set; }

	public BoundAttribute(SyntaxNode syntax, ImmutableArray<BoundExpression> arguments, bool hasErrors = false)
		: base(BoundKind.Attribute, syntax, hasErrors || arguments.HasErrors())
	{
		Arguments = arguments;
	}

	public override BoundNode Accept(BoundTreeVisitor visitor)
	{
		return visitor.VisitAttribute(this);
	}

	public BoundAttribute Update(ImmutableArray<BoundExpression> arguments)
	{
		if (arguments != Arguments)
		{
			return new BoundAttribute(Syntax, arguments, HasErrors)
			{
				WasCompilerGenerated = WasCompilerGenerated
			};
		}
		return this;
	}
}
