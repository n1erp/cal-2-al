using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SourceLabelSymbol : LabelSymbol
{
	private readonly MethodSymbol _containingMethod;

	private readonly SyntaxNodeOrToken _identifierNodeOrToken;

	private readonly ConstantValue _switchCaseLabelConstant;

	private string _lazyName;

	public override string Name => _lazyName ?? (_lazyName = MakeLabelName());

	public override ImmutableArray<Location> Locations
	{
		get
		{
			if (!_identifierNodeOrToken.IsToken || _identifierNodeOrToken.Parent != null)
			{
				return ImmutableArray.Create(_identifierNodeOrToken.GetLocation());
			}
			return ImmutableArray<Location>.Empty;
		}
	}

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences
	{
		get
		{
			CSharpSyntaxNode cSharpSyntaxNode = null;
			if (_identifierNodeOrToken.IsToken)
			{
				if (_identifierNodeOrToken.Parent != null)
				{
					cSharpSyntaxNode = _identifierNodeOrToken.Parent.FirstAncestorOrSelf<LabeledStatementSyntax>();
				}
			}
			else
			{
				cSharpSyntaxNode = _identifierNodeOrToken.AsNode().FirstAncestorOrSelf<SwitchLabelSyntax>();
			}
			if (cSharpSyntaxNode != null)
			{
				return ImmutableArray.Create(cSharpSyntaxNode.GetReference());
			}
			return ImmutableArray<SyntaxReference>.Empty;
		}
	}

	public override MethodSymbol ContainingMethod => _containingMethod;

	public override Symbol ContainingSymbol => _containingMethod;

	internal override SyntaxNodeOrToken IdentifierNodeOrToken => _identifierNodeOrToken;

	public ConstantValue SwitchCaseLabelConstant => _switchCaseLabelConstant;

	public SourceLabelSymbol(MethodSymbol containingMethod, SyntaxNodeOrToken identifierNodeOrToken, ConstantValue switchCaseLabelConstant = null)
	{
		_containingMethod = containingMethod;
		_identifierNodeOrToken = identifierNodeOrToken;
		_switchCaseLabelConstant = switchCaseLabelConstant;
	}

	private string MakeLabelName()
	{
		SyntaxNode syntaxNode = _identifierNodeOrToken.AsNode();
		if (syntaxNode != null)
		{
			if (syntaxNode.Kind() == SyntaxKind.DefaultSwitchLabel)
			{
				return ((DefaultSwitchLabelSyntax)syntaxNode).Keyword.ToString();
			}
			return syntaxNode.ToString();
		}
		SyntaxToken token = _identifierNodeOrToken.AsToken();
		if (token.Kind() != SyntaxKind.None)
		{
			return token.ValueText;
		}
		return _switchCaseLabelConstant?.ToString() ?? "";
	}

	public SourceLabelSymbol(MethodSymbol containingMethod, ConstantValue switchCaseLabelConstant)
	{
		_containingMethod = containingMethod;
		_identifierNodeOrToken = default(SyntaxToken);
		_switchCaseLabelConstant = switchCaseLabelConstant;
	}

	public override bool Equals(object obj)
	{
		if (obj == this)
		{
			return true;
		}
		if (obj is SourceLabelSymbol sourceLabelSymbol && sourceLabelSymbol._identifierNodeOrToken.Kind() != SyntaxKind.None && sourceLabelSymbol._identifierNodeOrToken.Equals(_identifierNodeOrToken))
		{
			return object.Equals(sourceLabelSymbol._containingMethod, _containingMethod);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return _identifierNodeOrToken.GetHashCode();
	}
}
