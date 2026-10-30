using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.CodeAnalysis.CSharp.Syntax.InternalSyntax;

[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal struct Directive
{
	private readonly DirectiveTriviaSyntax _node;

	public SyntaxKind Kind => _node.Kind;

	internal bool IsActive => _node.IsActive;

	internal bool BranchTaken
	{
		get
		{
			if (_node is BranchingDirectiveTriviaSyntax branchingDirectiveTriviaSyntax)
			{
				return branchingDirectiveTriviaSyntax.BranchTaken;
			}
			return false;
		}
	}

	internal Directive(DirectiveTriviaSyntax node)
	{
		_node = node;
	}

	public bool IncrementallyEquivalent(Directive other)
	{
		if (Kind != other.Kind)
		{
			return false;
		}
		bool isActive = IsActive;
		bool isActive2 = other.IsActive;
		if (!isActive && !isActive2)
		{
			return true;
		}
		if (isActive != isActive2)
		{
			return false;
		}
		switch (Kind)
		{
		case SyntaxKind.DefineDirectiveTrivia:
		case SyntaxKind.UndefDirectiveTrivia:
			return GetIdentifier() == other.GetIdentifier();
		case SyntaxKind.IfDirectiveTrivia:
		case SyntaxKind.ElifDirectiveTrivia:
		case SyntaxKind.ElseDirectiveTrivia:
			return BranchTaken == other.BranchTaken;
		default:
			return true;
		}
	}

	internal string GetDebuggerDisplay()
	{
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		_node.WriteTo(stringWriter, leading: false, trailing: false);
		return stringWriter.ToString();
	}

	internal string GetIdentifier()
	{
		return _node.Kind switch
		{
			SyntaxKind.DefineDirectiveTrivia => ((DefineDirectiveTriviaSyntax)_node).Name.ValueText, 
			SyntaxKind.UndefDirectiveTrivia => ((UndefDirectiveTriviaSyntax)_node).Name.ValueText, 
			_ => null, 
		};
	}
}
