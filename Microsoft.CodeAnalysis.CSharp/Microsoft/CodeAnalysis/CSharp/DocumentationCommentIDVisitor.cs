using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class DocumentationCommentIDVisitor : CSharpSymbolVisitor<StringBuilder, object>
{
	private sealed class PartVisitor : CSharpSymbolVisitor<StringBuilder, object>
	{
		internal static readonly PartVisitor Instance = new PartVisitor(inParameterOrReturnType: false);

		private static readonly PartVisitor s_parameterOrReturnTypeInstance = new PartVisitor(inParameterOrReturnType: true);

		private readonly bool _inParameterOrReturnType;

		private PartVisitor(bool inParameterOrReturnType)
		{
			_inParameterOrReturnType = inParameterOrReturnType;
		}

		public override object VisitArrayType(ArrayTypeSymbol symbol, StringBuilder builder)
		{
			Visit(symbol.ElementType, builder);
			if (symbol.IsSZArray)
			{
				builder.Append("[]");
			}
			else
			{
				builder.Append("[0:");
				for (int i = 0; i < symbol.Rank - 1; i++)
				{
					builder.Append(",0:");
				}
				builder.Append(']');
			}
			return null;
		}

		public override object VisitField(FieldSymbol symbol, StringBuilder builder)
		{
			Visit(symbol.ContainingType, builder);
			builder.Append('.');
			builder.Append(symbol.Name);
			return null;
		}

		private void VisitParameters(ImmutableArray<ParameterSymbol> parameters, bool isVararg, StringBuilder builder)
		{
			builder.Append('(');
			bool flag = false;
			foreach (ParameterSymbol item in parameters)
			{
				if (flag)
				{
					builder.Append(',');
				}
				Visit(item, builder);
				flag = true;
			}
			if (isVararg & flag)
			{
				builder.Append(',');
			}
			builder.Append(')');
		}

		public override object VisitMethod(MethodSymbol symbol, StringBuilder builder)
		{
			Visit(symbol.ContainingType, builder);
			builder.Append('.');
			builder.Append(GetEscapedMetadataName(symbol));
			if (symbol.Arity != 0)
			{
				builder.Append("``");
				builder.Append(symbol.Arity);
			}
			if (symbol.Parameters.Any() || symbol.IsVararg)
			{
				s_parameterOrReturnTypeInstance.VisitParameters(symbol.Parameters, symbol.IsVararg, builder);
			}
			if (symbol.MethodKind == MethodKind.Conversion)
			{
				builder.Append('~');
				s_parameterOrReturnTypeInstance.Visit(symbol.ReturnType, builder);
			}
			return null;
		}

		public override object VisitProperty(PropertySymbol symbol, StringBuilder builder)
		{
			Visit(symbol.ContainingType, builder);
			builder.Append('.');
			builder.Append(GetEscapedMetadataName(symbol));
			if (symbol.Parameters.Any())
			{
				s_parameterOrReturnTypeInstance.VisitParameters(symbol.Parameters, isVararg: false, builder);
			}
			return null;
		}

		public override object VisitEvent(EventSymbol symbol, StringBuilder builder)
		{
			Visit(symbol.ContainingType, builder);
			builder.Append('.');
			builder.Append(GetEscapedMetadataName(symbol));
			return null;
		}

		public override object VisitTypeParameter(TypeParameterSymbol symbol, StringBuilder builder)
		{
			int num = 0;
			Symbol containingSymbol = symbol.ContainingSymbol;
			if (containingSymbol.Kind == SymbolKind.Method)
			{
				builder.Append("``");
			}
			else
			{
				NamedTypeSymbol containingType = containingSymbol.ContainingType;
				while ((object)containingType != null)
				{
					num += containingType.Arity;
					containingType = containingType.ContainingType;
				}
				builder.Append('`');
			}
			builder.Append(symbol.Ordinal + num);
			return null;
		}

		public override object VisitNamedType(NamedTypeSymbol symbol, StringBuilder builder)
		{
			if (symbol.IsTupleType)
			{
				return VisitNamedType(((TupleTypeSymbol)symbol).UnderlyingNamedType, builder);
			}
			if ((object)symbol.ContainingSymbol != null && symbol.ContainingSymbol.Name.Length != 0)
			{
				Visit(symbol.ContainingSymbol, builder);
				builder.Append('.');
			}
			builder.Append(symbol.Name);
			if (symbol.Arity != 0)
			{
				if (!_inParameterOrReturnType && symbol == symbol.ConstructedFrom)
				{
					builder.Append('`');
					builder.Append(symbol.Arity);
				}
				else
				{
					builder.Append('{');
					bool flag = false;
					foreach (TypeSymbol typeArgumentsNoUseSiteDiagnostic in symbol.TypeArgumentsNoUseSiteDiagnostics)
					{
						if (flag)
						{
							builder.Append(',');
						}
						Visit(typeArgumentsNoUseSiteDiagnostic, builder);
						flag = true;
					}
					builder.Append('}');
				}
			}
			return null;
		}

		public override object VisitPointerType(PointerTypeSymbol symbol, StringBuilder builder)
		{
			Visit(symbol.PointedAtType, builder);
			builder.Append('*');
			return null;
		}

		public override object VisitNamespace(NamespaceSymbol symbol, StringBuilder builder)
		{
			if ((object)symbol.ContainingNamespace != null && symbol.ContainingNamespace.Name.Length != 0)
			{
				Visit(symbol.ContainingNamespace, builder);
				builder.Append('.');
			}
			builder.Append(symbol.Name);
			return null;
		}

		public override object VisitParameter(ParameterSymbol symbol, StringBuilder builder)
		{
			Visit(symbol.Type, builder);
			if (symbol.RefKind != RefKind.None)
			{
				builder.Append('@');
			}
			return null;
		}

		public override object VisitErrorType(ErrorTypeSymbol symbol, StringBuilder builder)
		{
			return VisitNamedType(symbol, builder);
		}

		public override object VisitDynamicType(DynamicTypeSymbol symbol, StringBuilder builder)
		{
			builder.Append("System.Object");
			return null;
		}

		private static string GetEscapedMetadataName(Symbol symbol)
		{
			string metadataName = symbol.MetadataName;
			int num = metadataName.IndexOf("::", StringComparison.Ordinal);
			int num2 = ((num >= 0) ? (num + 2) : 0);
			PooledStringBuilder instance = PooledStringBuilder.GetInstance();
			instance.Builder.Append(metadataName, num2, metadataName.Length - num2);
			instance.Builder.Replace('.', '#').Replace('<', '{').Replace('>', '}');
			return instance.ToStringAndFree();
		}
	}

	public static readonly DocumentationCommentIDVisitor Instance = new DocumentationCommentIDVisitor();

	private DocumentationCommentIDVisitor()
	{
	}

	public override object DefaultVisit(Symbol symbol, StringBuilder builder)
	{
		return null;
	}

	public override object VisitNamespace(NamespaceSymbol symbol, StringBuilder builder)
	{
		if (!symbol.IsGlobalNamespace)
		{
			builder.Append("N:");
			PartVisitor.Instance.Visit(symbol, builder);
		}
		return null;
	}

	public override object VisitMethod(MethodSymbol symbol, StringBuilder builder)
	{
		builder.Append("M:");
		PartVisitor.Instance.Visit(symbol, builder);
		return null;
	}

	public override object VisitField(FieldSymbol symbol, StringBuilder builder)
	{
		builder.Append("F:");
		PartVisitor.Instance.Visit(symbol, builder);
		return null;
	}

	public override object VisitEvent(EventSymbol symbol, StringBuilder builder)
	{
		builder.Append("E:");
		PartVisitor.Instance.Visit(symbol, builder);
		return null;
	}

	public override object VisitProperty(PropertySymbol symbol, StringBuilder builder)
	{
		builder.Append("P:");
		PartVisitor.Instance.Visit(symbol, builder);
		return null;
	}

	public override object VisitNamedType(NamedTypeSymbol symbol, StringBuilder builder)
	{
		builder.Append("T:");
		PartVisitor.Instance.Visit(symbol, builder);
		return null;
	}

	public override object VisitDynamicType(DynamicTypeSymbol symbol, StringBuilder builder)
	{
		return DefaultVisit(symbol, builder);
	}

	public override object VisitErrorType(ErrorTypeSymbol symbol, StringBuilder builder)
	{
		builder.Append("!:");
		PartVisitor.Instance.Visit(symbol, builder);
		return null;
	}

	public override object VisitTypeParameter(TypeParameterSymbol symbol, StringBuilder builder)
	{
		builder.Append("!:");
		builder.Append(symbol.Name);
		return null;
	}
}
