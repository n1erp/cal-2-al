using System;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal abstract class BoundNode
{
	[Flags]
	private enum BoundNodeAttributes : byte
	{
		HasErrors = 1,
		CompilerGenerated = 2
	}

	private readonly BoundKind kind;

	private BoundNodeAttributes attributes;

	public readonly SyntaxNode Syntax;

	public bool HasAnyErrors
	{
		get
		{
			if (HasErrors || (Syntax != null && Syntax.HasErrors))
			{
				return true;
			}
			if (this is BoundExpression { Type: not null } boundExpression)
			{
				return boundExpression.Type.IsErrorType();
			}
			return false;
		}
	}

	public bool HasErrors => (attributes & BoundNodeAttributes.HasErrors) != 0;

	public SyntaxTree SyntaxTree => Syntax?.SyntaxTree;

	public bool WasCompilerGenerated
	{
		get
		{
			return (attributes & BoundNodeAttributes.CompilerGenerated) != 0;
		}
		internal set
		{
			if (value)
			{
				attributes |= BoundNodeAttributes.CompilerGenerated;
			}
			else
			{
				DebugAssertHelper.Assert((attributes & BoundNodeAttributes.CompilerGenerated) == 0, "compiler generated flag should not be reset here");
			}
		}
	}

	public BoundKind Kind => kind;

	protected BoundNode(BoundKind kind, SyntaxNode syntax)
	{
		DebugAssertHelper.Assert(syntax != null);
		this.kind = kind;
		Syntax = syntax;
	}

	protected BoundNode(BoundKind kind, SyntaxNode syntax, bool hasErrors)
		: this(kind, syntax)
	{
		if (hasErrors)
		{
			attributes = BoundNodeAttributes.HasErrors;
		}
	}

	internal int GetStartLineNo()
	{
		return Syntax.GetLocation().GetLineSpan().StartLinePosition.Line;
	}

	public void ResetCompilerGenerated(bool newCompilerGenerated)
	{
		if (newCompilerGenerated)
		{
			attributes |= BoundNodeAttributes.CompilerGenerated;
		}
		else
		{
			attributes &= ~BoundNodeAttributes.CompilerGenerated;
		}
	}

	public virtual BoundNode Accept(BoundTreeVisitor visitor)
	{
		throw new NotImplementedException();
	}
}
