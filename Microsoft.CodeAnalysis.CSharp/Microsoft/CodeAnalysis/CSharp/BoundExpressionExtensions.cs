using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class BoundExpressionExtensions
{
	public static RefKind GetRefKind(this BoundExpression node)
	{
		return node.Kind switch
		{
			BoundKind.Local => ((BoundLocal)node).LocalSymbol.RefKind, 
			BoundKind.Parameter => ((BoundParameter)node).ParameterSymbol.RefKind, 
			BoundKind.Call => ((BoundCall)node).Method.RefKind, 
			BoundKind.PropertyAccess => ((BoundPropertyAccess)node).PropertySymbol.RefKind, 
			_ => RefKind.None, 
		};
	}

	public static bool IsLiteralNull(this BoundExpression node)
	{
		if (node.Kind == BoundKind.Literal)
		{
			return node.ConstantValue.Discriminator == ConstantValueTypeDiscriminator.Nothing;
		}
		return false;
	}

	public static bool IsLiteralDefault(this BoundExpression node)
	{
		if (node.Kind == BoundKind.DefaultExpression)
		{
			return node.Syntax.Kind() == SyntaxKind.DefaultLiteralExpression;
		}
		return false;
	}

	public static bool IsDefaultValue(this BoundExpression node)
	{
		if (node.Kind == BoundKind.DefaultExpression)
		{
			return true;
		}
		ConstantValue constantValue = node.ConstantValue;
		if (constantValue != null)
		{
			return constantValue.IsDefaultValue;
		}
		return false;
	}

	public static bool HasExpressionType(this BoundExpression node)
	{
		return (object)node.Type != null;
	}

	public static bool HasDynamicType(this BoundExpression node)
	{
		return node.Type?.IsDynamic() ?? false;
	}

	public static bool MethodGroupReceiverIsDynamic(this BoundMethodGroup node)
	{
		if (node.InstanceOpt != null)
		{
			return node.InstanceOpt.HasDynamicType();
		}
		return false;
	}

	public static bool HasExpressionSymbols(this BoundExpression node)
	{
		switch (node.Kind)
		{
		case BoundKind.TypeExpression:
		case BoundKind.NamespaceExpression:
		case BoundKind.Local:
		case BoundKind.MethodGroup:
		case BoundKind.Call:
		case BoundKind.ObjectCreationExpression:
		case BoundKind.FieldAccess:
		case BoundKind.PropertyAccess:
		case BoundKind.EventAccess:
		case BoundKind.IndexerAccess:
			return true;
		case BoundKind.BadExpression:
			return ((BoundBadExpression)node).Symbols.Length > 0;
		default:
			return false;
		}
	}

	public static void GetExpressionSymbols(this BoundExpression node, ArrayBuilder<Symbol> symbols, BoundNode parent, Binder binder)
	{
		switch (node.Kind)
		{
		case BoundKind.MethodGroup:
			if (parent is BoundDelegateCreationExpression { MethodOpt: not null } boundDelegateCreationExpression)
			{
				symbols.Add(boundDelegateCreationExpression.MethodOpt);
			}
			else
			{
				symbols.AddRange(CSharpSemanticModel.GetReducedAndFilteredMethodGroupSymbols(binder, (BoundMethodGroup)node));
			}
			return;
		case BoundKind.BadExpression:
			symbols.AddRange(((BoundBadExpression)node).Symbols);
			return;
		case BoundKind.DelegateCreationExpression:
		{
			Symbol symbol = ((BoundDelegateCreationExpression)node).Type.GetMembers(".ctor").FirstOrDefault();
			if ((object)symbol != null)
			{
				symbols.Add(symbol);
			}
			return;
		}
		case BoundKind.Call:
		{
			ImmutableArray<MethodSymbol> originalMethodsOpt = ((BoundCall)node).OriginalMethodsOpt;
			if (!originalMethodsOpt.IsDefault)
			{
				symbols.AddRange(originalMethodsOpt);
				return;
			}
			break;
		}
		case BoundKind.IndexerAccess:
		{
			ImmutableArray<PropertySymbol> originalIndexersOpt = ((BoundIndexerAccess)node).OriginalIndexersOpt;
			if (!originalIndexersOpt.IsDefault)
			{
				symbols.AddRange(originalIndexersOpt);
				return;
			}
			break;
		}
		}
		Symbol expressionSymbol = node.ExpressionSymbol;
		if ((object)expressionSymbol != null)
		{
			symbols.Add(expressionSymbol);
		}
	}

	public static Conversion GetConversion(this BoundExpression boundNode)
	{
		BoundKind kind = boundNode.Kind;
		if (kind == BoundKind.Conversion)
		{
			return ((BoundConversion)boundNode).Conversion;
		}
		return Conversion.Identity;
	}

	internal static bool IsExpressionOfComImportType(this BoundExpression expressionOpt)
	{
		if (expressionOpt == null)
		{
			return false;
		}
		TypeSymbol type = expressionOpt.Type;
		if ((object)type != null && type.Kind == SymbolKind.NamedType)
		{
			return ((NamedTypeSymbol)type).IsComImport;
		}
		return false;
	}

	public static bool NullableAlwaysHasValue(this BoundExpression expr)
	{
		if ((object)expr.Type == null)
		{
			return false;
		}
		if (expr.Type.IsDynamic())
		{
			return false;
		}
		if (!expr.Type.IsNullableType())
		{
			return true;
		}
		if (expr.Kind == BoundKind.ObjectCreationExpression)
		{
			return ((BoundObjectCreationExpression)expr).Constructor.ParameterCount != 0;
		}
		if (expr.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			switch (boundConversion.ConversionKind)
			{
			case ConversionKind.ImplicitNullable:
			case ConversionKind.ExplicitNullable:
				return boundConversion.Operand.NullableAlwaysHasValue();
			case ConversionKind.ImplicitEnumeration:
				return boundConversion.Operand.NullableAlwaysHasValue();
			}
		}
		return false;
	}

	public static bool NullableNeverHasValue(this BoundExpression expr)
	{
		if ((object)expr.Type == null && expr.ConstantValue == ConstantValue.Null)
		{
			return true;
		}
		if ((object)expr.Type == null || !expr.Type.IsNullableType())
		{
			return false;
		}
		if (expr.Kind == BoundKind.DefaultExpression)
		{
			return true;
		}
		if (expr.Kind == BoundKind.ObjectCreationExpression)
		{
			return ((BoundObjectCreationExpression)expr).Constructor.ParameterCount == 0;
		}
		if (expr.Kind == BoundKind.Conversion)
		{
			BoundConversion boundConversion = (BoundConversion)expr;
			switch (boundConversion.ConversionKind)
			{
			case ConversionKind.DefaultOrNullLiteral:
				return true;
			case ConversionKind.ImplicitNullable:
			case ConversionKind.ExplicitNullable:
				return boundConversion.Operand.NullableNeverHasValue();
			}
		}
		return false;
	}

	public static bool IsNullableNonBoolean(this BoundExpression expr)
	{
		if (expr.Type.IsNullableType() && expr.Type.GetNullableUnderlyingType().SpecialType != SpecialType.System_Boolean)
		{
			return true;
		}
		return false;
	}
}
