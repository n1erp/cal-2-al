using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class TypeofBinder : Binder
{
	private class OpenTypeVisitor : CSharpSyntaxVisitor
	{
		private Dictionary<GenericNameSyntax, bool> _allowedMap;

		private bool _seenConstructed;

		private bool _seenGeneric;

		public static void Visit(ExpressionSyntax typeSyntax, out Dictionary<GenericNameSyntax, bool> allowedMap, out bool isUnboundGenericType)
		{
			OpenTypeVisitor openTypeVisitor = new OpenTypeVisitor();
			openTypeVisitor.Visit(typeSyntax);
			allowedMap = openTypeVisitor._allowedMap;
			isUnboundGenericType = openTypeVisitor._seenGeneric && !openTypeVisitor._seenConstructed;
		}

		public override void VisitGenericName(GenericNameSyntax node)
		{
			_seenGeneric = true;
			SeparatedSyntaxList<TypeSyntax> arguments = node.TypeArgumentList.Arguments;
			if (node.IsUnboundGenericName)
			{
				if (_allowedMap == null)
				{
					_allowedMap = new Dictionary<GenericNameSyntax, bool>();
				}
				_allowedMap[node] = !_seenConstructed;
				return;
			}
			_seenConstructed = true;
			foreach (TypeSyntax item in arguments)
			{
				Visit(item);
			}
		}

		public override void VisitQualifiedName(QualifiedNameSyntax node)
		{
			bool seenConstructed = _seenConstructed;
			Visit(node.Right);
			bool seenConstructed2 = _seenConstructed;
			Visit(node.Left);
			if (!seenConstructed && !seenConstructed2 && _seenConstructed)
			{
				Visit(node.Right);
			}
		}

		public override void VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
		{
			Visit(node.Name);
		}

		public override void VisitArrayType(ArrayTypeSyntax node)
		{
			_seenConstructed = true;
			Visit(node.ElementType);
		}

		public override void VisitPointerType(PointerTypeSyntax node)
		{
			_seenConstructed = true;
			Visit(node.ElementType);
		}

		public override void VisitNullableType(NullableTypeSyntax node)
		{
			_seenConstructed = true;
			Visit(node.ElementType);
		}
	}

	private readonly Dictionary<GenericNameSyntax, bool> _allowedMap;

	private readonly bool _isTypeExpressionOpen;

	internal bool IsTypeExpressionOpen => _isTypeExpressionOpen;

	internal TypeofBinder(ExpressionSyntax typeExpression, Binder next)
		: base(next, next.Flags | BinderFlags.UnsafeRegion)
	{
		OpenTypeVisitor.Visit(typeExpression, out _allowedMap, out _isTypeExpressionOpen);
	}

	protected override bool IsUnboundTypeAllowed(GenericNameSyntax syntax)
	{
		bool value = default;
		return (_allowedMap != null && _allowedMap.TryGetValue(syntax, out value)) & value;
	}
}
