using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class ParameterHelpers
{
	public static ImmutableArray<ParameterSymbol> MakeParameters(Binder binder, Symbol owner, BaseParameterListSyntax syntax, out SyntaxToken arglistToken, DiagnosticBag diagnostics, bool allowRefOrOut, bool allowThis, bool addRefReadOnlyModifier)
	{
		arglistToken = default;
		int num = 0;
		int num2 = -1;
		ArrayBuilder<ParameterSymbol> instance = ArrayBuilder<ParameterSymbol>.GetInstance();
		ParameterSyntax parameterSyntax = null;
		foreach (ParameterSyntax parameter in syntax.Parameters)
		{
			if (parameterSyntax == null && (parameter.Modifiers.Any(SyntaxKind.ParamsKeyword) || parameter.Identifier.Kind() == SyntaxKind.ArgListKeyword))
			{
				parameterSyntax = parameter;
			}
			CheckParameterModifiers(parameter, diagnostics);
			RefKind modifiers = GetModifiers(parameter.Modifiers, out var refnessKeyword, out var paramsKeyword, out var thisKeyword);
			if (thisKeyword.Kind() != SyntaxKind.None && !allowThis)
			{
				diagnostics.Add(ErrorCode.ERR_ThisInBadContext, thisKeyword.GetLocation());
			}
			if (parameter.IsArgList)
			{
				arglistToken = parameter.Identifier;
				if (paramsKeyword.Kind() != SyntaxKind.None || refnessKeyword.Kind() != SyntaxKind.None || thisKeyword.Kind() != SyntaxKind.None)
				{
					diagnostics.Add(ErrorCode.ERR_IllegalVarArgs, arglistToken.GetLocation());
				}
				continue;
			}
			if (parameter.Default != null && num2 == -1)
			{
				num2 = num;
			}
			TypeSymbol parameterType = binder.BindType(parameter.Type, diagnostics);
			if (!allowRefOrOut && (modifiers == RefKind.Ref || modifiers == RefKind.Out))
			{
				diagnostics.Add(ErrorCode.ERR_IllegalRefParam, refnessKeyword.GetLocation());
			}
			SourceParameterSymbol sourceParameterSymbol = SourceParameterSymbol.Create(binder, owner, parameterType, parameter, modifiers, parameter.Identifier, num, paramsKeyword.Kind() != SyntaxKind.None, num == 0 && thisKeyword.Kind() != SyntaxKind.None, addRefReadOnlyModifier, diagnostics);
			ReportParameterErrors(owner, parameter, sourceParameterSymbol, thisKeyword, paramsKeyword, num2, diagnostics);
			instance.Add(sourceParameterSymbol);
			num++;
		}
		if (parameterSyntax != null && parameterSyntax != syntax.Parameters.Last())
		{
			diagnostics.Add((parameterSyntax.Identifier.Kind() == SyntaxKind.ArgListKeyword) ? ErrorCode.ERR_VarargsLast : ErrorCode.ERR_ParamsLast, parameterSyntax.GetLocation());
		}
		ImmutableArray<ParameterSymbol> immutableArray = instance.ToImmutableAndFree();
		ImmutableArray<TypeParameterSymbol> typeParameters = ((owner is MethodSymbol methodSymbol) ? methodSymbol.TypeParameters : default(ImmutableArray<TypeParameterSymbol>));
		binder.ValidateParameterNameConflicts(typeParameters, immutableArray, diagnostics);
		return immutableArray;
	}

	internal static void EnsureIsReadOnlyAttributeExists(ImmutableArray<ParameterSymbol> parameters, DiagnosticBag diagnostics, bool modifyCompilationForRefReadOnly)
	{
		foreach (ParameterSymbol item in parameters)
		{
			if (item.RefKind == RefKind.In)
			{
				item.DeclaringCompilation?.EnsureIsReadOnlyAttributeExists(diagnostics, item.GetNonNullSyntaxNode().Location, modifyCompilationForRefReadOnly);
			}
		}
	}

	private static void CheckParameterModifiers(ParameterSyntax parameter, DiagnosticBag diagnostics)
	{
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		bool flag4 = false;
		bool flag5 = false;
		foreach (SyntaxToken modifier in parameter.Modifiers)
		{
			switch (modifier.Kind())
			{
			case SyntaxKind.ThisKeyword:
				if (flag)
				{
					diagnostics.Add(ErrorCode.ERR_DupParamMod, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.ThisKeyword));
				}
				else if (flag3)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.ThisKeyword), SyntaxFacts.GetText(SyntaxKind.OutKeyword));
				}
				else if (flag4)
				{
					diagnostics.Add(ErrorCode.ERR_BadParamModThis, modifier.GetLocation());
				}
				else
				{
					flag = true;
				}
				break;
			case SyntaxKind.RefKeyword:
				if (flag2)
				{
					diagnostics.Add(ErrorCode.ERR_DupParamMod, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.RefKeyword));
				}
				else if (flag4)
				{
					diagnostics.Add(ErrorCode.ERR_ParamsCantBeWithModifier, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.RefKeyword));
				}
				else if (flag3)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.RefKeyword), SyntaxFacts.GetText(SyntaxKind.OutKeyword));
				}
				else if (flag5)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.RefKeyword), SyntaxFacts.GetText(SyntaxKind.InKeyword));
				}
				else
				{
					flag2 = true;
				}
				break;
			case SyntaxKind.OutKeyword:
				if (flag3)
				{
					diagnostics.Add(ErrorCode.ERR_DupParamMod, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.OutKeyword));
				}
				else if (flag)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.OutKeyword), SyntaxFacts.GetText(SyntaxKind.ThisKeyword));
				}
				else if (flag4)
				{
					diagnostics.Add(ErrorCode.ERR_ParamsCantBeWithModifier, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.OutKeyword));
				}
				else if (flag2)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.OutKeyword), SyntaxFacts.GetText(SyntaxKind.RefKeyword));
				}
				else if (flag5)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.OutKeyword), SyntaxFacts.GetText(SyntaxKind.InKeyword));
				}
				else
				{
					flag3 = true;
				}
				break;
			case SyntaxKind.ParamsKeyword:
				if (flag4)
				{
					diagnostics.Add(ErrorCode.ERR_DupParamMod, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.ParamsKeyword));
				}
				else if (flag)
				{
					diagnostics.Add(ErrorCode.ERR_BadParamModThis, modifier.GetLocation());
				}
				else if (flag2)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.ParamsKeyword), SyntaxFacts.GetText(SyntaxKind.RefKeyword));
				}
				else if (flag5)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.ParamsKeyword), SyntaxFacts.GetText(SyntaxKind.InKeyword));
				}
				else if (flag3)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.ParamsKeyword), SyntaxFacts.GetText(SyntaxKind.OutKeyword));
				}
				else
				{
					flag4 = true;
				}
				break;
			case SyntaxKind.InKeyword:
				if (flag5)
				{
					diagnostics.Add(ErrorCode.ERR_DupParamMod, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.InKeyword));
				}
				else if (flag3)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.InKeyword), SyntaxFacts.GetText(SyntaxKind.OutKeyword));
				}
				else if (flag2)
				{
					diagnostics.Add(ErrorCode.ERR_BadParameterModifiers, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.InKeyword), SyntaxFacts.GetText(SyntaxKind.RefKeyword));
				}
				else if (flag4)
				{
					diagnostics.Add(ErrorCode.ERR_ParamsCantBeWithModifier, modifier.GetLocation(), SyntaxFacts.GetText(SyntaxKind.InKeyword));
				}
				else
				{
					flag5 = true;
				}
				break;
			default:
				throw ExceptionUtilities.UnexpectedValue(modifier.Kind());
			}
		}
	}

	private static void ReportParameterErrors(Symbol owner, ParameterSyntax parameterSyntax, SourceParameterSymbol parameter, SyntaxToken thisKeyword, SyntaxToken paramsKeyword, int firstDefault, DiagnosticBag diagnostics)
	{
		TypeSymbol type = parameter.Type;
		int ordinal = parameter.Ordinal;
		bool flag = parameterSyntax.Default != null;
		if (thisKeyword.Kind() == SyntaxKind.ThisKeyword && ordinal != 0)
		{
			diagnostics.Add(ErrorCode.ERR_BadThisParam, thisKeyword.GetLocation(), owner.Name);
		}
		else if (parameter.IsParams && owner.IsOperator())
		{
			diagnostics.Add(ErrorCode.ERR_IllegalParams, paramsKeyword.GetLocation());
		}
		else if (parameter.IsParams && !type.IsSZArray())
		{
			diagnostics.Add(ErrorCode.ERR_ParamsMustBeArray, paramsKeyword.GetLocation());
		}
		else if (parameter.Type.IsStatic && !parameter.ContainingSymbol.ContainingType.IsInterfaceType())
		{
			diagnostics.Add(ErrorCode.ERR_ParameterIsStaticClass, owner.Locations[0], parameter.Type);
		}
		else if (firstDefault != -1 && ordinal > firstDefault && !flag && !parameter.IsParams)
		{
			Location location = parameterSyntax.Identifier.GetNextToken(includeZeroWidth: true).GetLocation();
			diagnostics.Add(ErrorCode.ERR_DefaultValueBeforeRequiredValue, location);
		}
		else if (parameter.RefKind != RefKind.None && parameter.Type.IsRestrictedType(ignoreSpanLikeTypes: true))
		{
			diagnostics.Add(ErrorCode.ERR_MethodArgCantBeRefAny, parameterSyntax.Location, parameter.Type);
		}
	}

	internal static bool ReportDefaultParameterErrors(Binder binder, Symbol owner, ParameterSyntax parameterSyntax, SourceParameterSymbol parameter, BoundExpression defaultExpression, DiagnosticBag diagnostics)
	{
		bool result = false;
		TypeSymbol type = parameter.Type;
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		Conversion conversion = binder.Conversions.ClassifyImplicitConversionFromExpression(defaultExpression, type, ref useSiteDiagnostics);
		diagnostics.Add(defaultExpression.Syntax, useSiteDiagnostics);
		RefKind modifiers = GetModifiers(parameterSyntax.Modifiers, out var refnessKeyword, out var paramsKeyword, out var thisKeyword);
		if (modifiers == RefKind.Ref || modifiers == RefKind.Out)
		{
			diagnostics.Add(ErrorCode.ERR_RefOutDefaultValue, refnessKeyword.GetLocation());
			result = true;
		}
		else if (paramsKeyword.Kind() == SyntaxKind.ParamsKeyword)
		{
			diagnostics.Add(ErrorCode.ERR_DefaultValueForParamsParameter, paramsKeyword.GetLocation());
			result = true;
		}
		else if (thisKeyword.Kind() == SyntaxKind.ThisKeyword)
		{
			if (parameter.Ordinal == 0)
			{
				diagnostics.Add(ErrorCode.ERR_DefaultValueForExtensionParameter, thisKeyword.GetLocation());
				result = true;
			}
		}
		else if (!defaultExpression.HasAnyErrors && !IsValidDefaultValue(defaultExpression))
		{
			diagnostics.Add(ErrorCode.ERR_DefaultValueMustBeConstant, parameterSyntax.Default.Value.Location, parameterSyntax.Identifier.ValueText);
			result = true;
		}
		else if (!conversion.Exists || conversion.IsUserDefined || (conversion.IsIdentity && type.SpecialType == SpecialType.System_Object && defaultExpression.Type.IsDynamic()))
		{
			diagnostics.Add(ErrorCode.ERR_NoConversionForDefaultParam, parameterSyntax.Identifier.GetLocation(), defaultExpression.Display, type);
			result = true;
		}
		else if ((conversion.IsReference && (type.SpecialType == SpecialType.System_Object || type.Kind == SymbolKind.DynamicType) && (object)defaultExpression.Type != null && defaultExpression.Type.SpecialType == SpecialType.System_String) || conversion.IsBoxing)
		{
			diagnostics.Add(ErrorCode.ERR_NotNullRefDefaultParameter, parameterSyntax.Identifier.GetLocation(), parameterSyntax.Identifier.ValueText, type);
			result = true;
		}
		else if (conversion.IsNullable && !defaultExpression.Type.IsNullableType() && !type.GetNullableUnderlyingType().IsEnumType() && !type.GetNullableUnderlyingType().IsIntrinsicType())
		{
			diagnostics.Add(ErrorCode.ERR_NoConversionForNubDefaultParam, parameterSyntax.Identifier.GetLocation(), defaultExpression.Type, parameterSyntax.Identifier.ValueText);
			result = true;
		}
		if (owner.IsExplicitInterfaceImplementation() || owner.IsPartialImplementation() || owner.IsOperator())
		{
			diagnostics.Add(ErrorCode.WRN_DefaultValueForUnconsumedLocation, parameterSyntax.Identifier.GetLocation(), parameterSyntax.Identifier.ValueText);
		}
		return result;
	}

	private static bool IsValidDefaultValue(BoundExpression expression)
	{
		if (!(expression.ConstantValue != null) && expression.Kind != BoundKind.DefaultExpression)
		{
			if (expression.Kind == BoundKind.ObjectCreationExpression)
			{
				return IsValidDefaultValue((BoundObjectCreationExpression)expression);
			}
			return false;
		}
		return true;
	}

	private static bool IsValidDefaultValue(BoundObjectCreationExpression expression)
	{
		if (expression.Constructor.IsDefaultValueTypeConstructor())
		{
			return expression.InitializerExpressionOpt == null;
		}
		return false;
	}

	internal static MethodSymbol FindContainingGenericMethod(Symbol symbol)
	{
		Symbol symbol2 = symbol;
		while ((object)symbol2 != null)
		{
			if (symbol2.Kind == SymbolKind.Method)
			{
				MethodSymbol methodSymbol = (MethodSymbol)symbol2;
				if (methodSymbol.MethodKind != MethodKind.AnonymousFunction)
				{
					if (!methodSymbol.IsGenericMethod)
					{
						return null;
					}
					return methodSymbol;
				}
			}
			symbol2 = symbol2.ContainingSymbol;
		}
		return null;
	}

	private static RefKind GetModifiers(SyntaxTokenList modifiers, out SyntaxToken refnessKeyword, out SyntaxToken paramsKeyword, out SyntaxToken thisKeyword)
	{
		RefKind refKind = RefKind.None;
		refnessKeyword = default;
		paramsKeyword = default;
		thisKeyword = default;
		foreach (SyntaxToken item in modifiers)
		{
			switch (item.Kind())
			{
			case SyntaxKind.OutKeyword:
				if (refKind == RefKind.None)
				{
					refnessKeyword = item;
					refKind = RefKind.Out;
				}
				break;
			case SyntaxKind.RefKeyword:
				if (refKind == RefKind.None)
				{
					refnessKeyword = item;
					refKind = RefKind.Ref;
				}
				break;
			case SyntaxKind.InKeyword:
				if (refKind == RefKind.None)
				{
					refnessKeyword = item;
					refKind = RefKind.In;
				}
				break;
			case SyntaxKind.ParamsKeyword:
				paramsKeyword = item;
				break;
			case SyntaxKind.ThisKeyword:
				thisKeyword = item;
				break;
			}
		}
		return refKind;
	}
}
