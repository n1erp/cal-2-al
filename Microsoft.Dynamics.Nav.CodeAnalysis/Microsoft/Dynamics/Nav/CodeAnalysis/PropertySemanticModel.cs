using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal sealed class PropertySemanticModel : MemberSemanticModel
{
	private bool addToMap;

	public override bool IsSpeculativeSemanticModel => false;

	public override int OriginalPositionForSpeculation => 0;

	private PropertySemanticModel(Compilation compilation, Symbol owner, Binder rootBinder, SyntaxNode syntax)
		: base(compilation, syntax, owner, rootBinder)
	{
		DebugAssertHelper.Assert((object)owner != null);
		DebugAssertHelper.Assert(owner.Kind == SymbolKind.Property);
		DebugAssertHelper.Assert(syntax != null);
	}

	internal static PropertySemanticModel Create(Compilation compilation, PropertySymbol owner, Binder rootBinder, SyntaxNode syntax)
	{
		ExecutableCodeBinder rootBinder2 = new ExecutableCodeBinder(syntax, owner, rootBinder);
		return new PropertySemanticModel(compilation, owner, rootBinder2, syntax);
	}

	internal override BoundNode Bind(Binder binder, SyntaxNode node, DiagnosticBag diagnostics)
	{
		if (node is PropertyExpressionSyntax)
		{
			return null;
		}
		if (node.Kind == SyntaxKind.Property)
		{
			return binder.BindProperty((PropertySyntax)node, diagnostics);
		}
		return null;
	}

	internal override bool TryGetSpeculativeSemanticModelForMethodBodyCore(SyntaxTreeSemanticModel parentModel, int position, MemberSyntax method, out SemanticModel speculativeModel)
	{
		speculativeModel = null;
		return false;
	}

	protected internal override SyntaxNode GetBindableSyntaxNode(SyntaxNode node)
	{
		SyntaxKind kind = node.Kind;
		if (kind == SyntaxKind.Property || kind == SyntaxKind.PermissionValue)
		{
			return node;
		}
		if (node is PropertyValueSyntax)
		{
			return node;
		}
		if (IsObjectReference(node) && node.Parent.ParentIsKind(SyntaxKind.PermissionValue))
		{
			return node.Parent.Parent;
		}
		return node.GetAncestor<PropertyValueSyntax>();
	}

	private static bool IsObjectReference(SyntaxNode node)
	{
		if (node.Kind == SyntaxKind.IdentifierName || node.Kind == SyntaxKind.ObjectId)
		{
			return node.ParentIsKind(SyntaxKind.ObjectReference);
		}
		return false;
	}

	protected override SyntaxNode GetBindableParentNode(SyntaxNode node)
	{
		if (node.Kind == SyntaxKind.Property)
		{
			return node;
		}
		return base.GetBindableParentNode(node);
	}

	protected override SyntaxNode GetBindingRoot(SyntaxNode node)
	{
		return Root;
	}

	internal override SymbolInfo GetSymbolInfoWorker(SyntaxNode node, CancellationToken cancellationToken = default(CancellationToken))
	{
		if (node.Kind == SyntaxKind.Property)
		{
			return new SymbolInfo(MemberSymbol);
		}
		PropertySymbol propertySymbol = MemberSymbol as PropertySymbol;
		if (propertySymbol != null)
		{
			if (propertySymbol.Property?.PropertyValue != null && propertySymbol.Property.PropertyValue.Kind == BoundKind.ApplicationObjectReferencePropertyValue && propertySymbol.Property.PropertyValue is BoundApplicationObjectReferencePropertyValue boundApplicationObjectReferencePropertyValue)
			{
				return new SymbolInfo(boundApplicationObjectReferencePropertyValue.Value, CandidateReason.None);
			}
			if (propertySymbol.PropertyKind == PropertyKind.OptionMembers && propertySymbol.ContainingType.Kind == SymbolKind.Field)
			{
				string name = node.GetIdentifierOrLiteralValue()?.UnquoteIdentifier();
				Symbol symbol = ((FieldSymbol)propertySymbol.ContainingType).Type.GetMembers(name).FirstOrDefault();
				if (!(symbol != null))
				{
					return SymbolInfo.None;
				}
				return new SymbolInfo(symbol, CandidateReason.None);
			}
		}
		ImmutableArray<BoundNode> boundNodeFromSyntax = GetBoundNodeFromSyntax(node);
		if (!boundNodeFromSyntax.IsDefaultOrEmpty)
		{
			foreach (BoundNode item in boundNodeFromSyntax)
			{
				Symbol symbol2 = (item as BoundExpression)?.ExpressionSymbol;
				if (symbol2 != null)
				{
					return new SymbolInfo(symbol2, CandidateReason.None);
				}
			}
		}
		return base.GetSymbolInfoWorker(node, cancellationToken);
	}

	internal ImmutableArray<BoundNode> GetBoundNodeFromSyntax(SyntaxNode node)
	{
		if (!addToMap)
		{
			using (NodeMapLock.DisposableWrite())
			{
				if (!addToMap)
				{
					GuardedAddBoundTreeAndGetBoundNodeFromMap(((PropertySyntax)Root).Value, ((PropertySymbol)MemberSymbol).Property.PropertyValue);
					addToMap = true;
				}
			}
		}
		ImmutableArray<BoundNode> result;
		using (NodeMapLock.DisposableRead())
		{
			result = GuardedGetBoundNodesFromMap(node);
		}
		if (!result.IsDefaultOrEmpty)
		{
			return result;
		}
		return ImmutableArray<BoundNode>.Empty;
	}
}
