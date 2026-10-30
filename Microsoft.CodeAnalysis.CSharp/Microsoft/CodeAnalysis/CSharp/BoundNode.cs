using System;
using System.Collections.Immutable;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.Operations;

namespace Microsoft.CodeAnalysis.CSharp;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class BoundNode : IBoundNodeWithIOperationChildren
{
	[Flags]
	private enum BoundNodeAttributes : byte
	{
		HasErrors = 1,
		CompilerGenerated = 2
	}

	private readonly BoundKind _kind;

	private BoundNodeAttributes _attributes;

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

	public bool HasErrors => (_attributes & BoundNodeAttributes.HasErrors) != 0;

	public SyntaxTree SyntaxTree => Syntax?.SyntaxTree;

	public bool WasCompilerGenerated
	{
		get
		{
			return (_attributes & BoundNodeAttributes.CompilerGenerated) != 0;
		}
		internal set
		{
			if (value)
			{
				_attributes |= BoundNodeAttributes.CompilerGenerated;
			}
		}
	}

	public BoundKind Kind => _kind;

	ImmutableArray<BoundNode> IBoundNodeWithIOperationChildren.Children => Children;

	protected virtual ImmutableArray<BoundNode> Children => ImmutableArray<BoundNode>.Empty;

	protected BoundNode(BoundKind kind, SyntaxNode syntax)
	{
		_kind = kind;
		Syntax = syntax;
	}

	protected BoundNode(BoundKind kind, SyntaxNode syntax, bool hasErrors)
		: this(kind, syntax)
	{
		if (hasErrors)
		{
			_attributes = BoundNodeAttributes.HasErrors;
		}
	}

	public void ResetCompilerGenerated(bool newCompilerGenerated)
	{
		if (newCompilerGenerated)
		{
			_attributes |= BoundNodeAttributes.CompilerGenerated;
		}
		else
		{
			_attributes &= ~BoundNodeAttributes.CompilerGenerated;
		}
	}

	public virtual BoundNode Accept(BoundTreeVisitor visitor)
	{
		throw new NotImplementedException();
	}

	internal string GetDebuggerDisplay()
	{
		string text = GetType().Name;
		if (Syntax != null)
		{
			text = text + " " + Syntax.ToString();
		}
		return text;
	}
}
