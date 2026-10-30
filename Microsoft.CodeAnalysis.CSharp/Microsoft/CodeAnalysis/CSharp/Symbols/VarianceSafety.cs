using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal static class VarianceSafety
{
	private delegate Location LocationProvider<T>(T arg);

	internal static void CheckInterfaceVarianceSafety(this NamedTypeSymbol interfaceType, DiagnosticBag diagnostics)
	{
		foreach (NamedTypeSymbol item in interfaceType.InterfacesNoUseSiteDiagnostics())
		{
			IsVarianceUnsafe(item, requireOutputSafety: true, requireInputSafety: false, item, (NamedTypeSymbol i) => (Location)null, item, diagnostics);
		}
		foreach (Symbol item2 in interfaceType.GetMembersUnordered())
		{
			switch (item2.Kind)
			{
			case SymbolKind.Method:
				if (!item2.IsAccessor())
				{
					((MethodSymbol)item2).CheckMethodVarianceSafety(diagnostics);
				}
				break;
			case SymbolKind.Property:
				CheckPropertyVarianceSafety((PropertySymbol)item2, diagnostics);
				break;
			case SymbolKind.Event:
				CheckEventVarianceSafety((EventSymbol)item2, diagnostics);
				break;
			}
		}
	}

	internal static void CheckDelegateVarianceSafety(this SourceDelegateMethodSymbol method, DiagnosticBag diagnostics)
	{
		method.CheckMethodVarianceSafety((MethodSymbol m) => m.GetDeclaringSyntax<DelegateDeclarationSyntax>()?.ReturnType.Location, diagnostics);
	}

	private static void CheckMethodVarianceSafety(this MethodSymbol method, DiagnosticBag diagnostics)
	{
		method.CheckMethodVarianceSafety((MethodSymbol m) => m.GetDeclaringSyntax<MethodDeclarationSyntax>()?.ReturnType.Location, diagnostics);
	}

	private static void CheckMethodVarianceSafety(this MethodSymbol method, LocationProvider<MethodSymbol> returnTypeLocationProvider, DiagnosticBag diagnostics)
	{
		CheckTypeParametersVarianceSafety(method.TypeParameters, method, diagnostics);
		IsVarianceUnsafe(method.ReturnType, requireOutputSafety: true, method.RefKind != RefKind.None, method, returnTypeLocationProvider, method, diagnostics);
		CheckParametersVarianceSafety(method.Parameters, method, diagnostics);
	}

	private static void CheckPropertyVarianceSafety(PropertySymbol property, DiagnosticBag diagnostics)
	{
		bool flag = (object)property.GetMethod != null;
		bool flag2 = (object)property.SetMethod != null;
		if (flag | flag2)
		{
			TypeSymbol type = property.Type;
			bool requireOutputSafety = flag;
			int requireInputSafety;
			if (!flag2)
			{
				MethodSymbol getMethod = property.GetMethod;
				requireInputSafety = (((object)getMethod == null || getMethod.RefKind != RefKind.None) ? 1 : 0);
			}
			else
			{
				requireInputSafety = 1;
			}
			IsVarianceUnsafe(type, requireOutputSafety, (byte)requireInputSafety != 0, property, (PropertySymbol p) => p.GetDeclaringSyntax<BasePropertyDeclarationSyntax>()?.Type.Location, property, diagnostics);
		}
		CheckParametersVarianceSafety(property.Parameters, property, diagnostics);
	}

	private static void CheckEventVarianceSafety(EventSymbol @event, DiagnosticBag diagnostics)
	{
		IsVarianceUnsafe(@event.Type, requireOutputSafety: false, requireInputSafety: true, @event, (EventSymbol e) => e.Locations[0], @event, diagnostics);
	}

	private static void CheckParametersVarianceSafety(ImmutableArray<ParameterSymbol> parameters, Symbol context, DiagnosticBag diagnostics)
	{
		foreach (ParameterSymbol item in parameters)
		{
			IsVarianceUnsafe(item.Type, item.RefKind != RefKind.None, requireInputSafety: true, context, (ParameterSymbol p) => p.GetDeclaringSyntax<ParameterSyntax>()?.Type.Location, item, diagnostics);
		}
	}

	private static void CheckTypeParametersVarianceSafety(ImmutableArray<TypeParameterSymbol> typeParameters, MethodSymbol context, DiagnosticBag diagnostics)
	{
		foreach (TypeParameterSymbol item in typeParameters)
		{
			foreach (TypeSymbol constraintTypesNoUseSiteDiagnostic in item.ConstraintTypesNoUseSiteDiagnostics)
			{
				IsVarianceUnsafe(constraintTypesNoUseSiteDiagnostic, requireOutputSafety: false, requireInputSafety: true, context, (TypeParameterSymbol t) => t.Locations[0], item, diagnostics);
			}
		}
	}

	private static bool IsVarianceUnsafe<T>(TypeSymbol type, bool requireOutputSafety, bool requireInputSafety, Symbol context, LocationProvider<T> locationProvider, T locationArg, DiagnosticBag diagnostics) where T : Symbol
	{
		switch (type.Kind)
		{
		case SymbolKind.TypeParameter:
		{
			TypeParameterSymbol typeParameterSymbol = (TypeParameterSymbol)type;
			if ((requireInputSafety & requireOutputSafety) && typeParameterSymbol.Variance != VarianceKind.None)
			{
				diagnostics.AddVarianceError(typeParameterSymbol, context, locationProvider, locationArg, MessageID.IDS_Invariantly);
				return true;
			}
			if (requireOutputSafety && typeParameterSymbol.Variance == VarianceKind.In)
			{
				diagnostics.AddVarianceError(typeParameterSymbol, context, locationProvider, locationArg, MessageID.IDS_Covariantly);
				return true;
			}
			if (requireInputSafety && typeParameterSymbol.Variance == VarianceKind.Out)
			{
				diagnostics.AddVarianceError(typeParameterSymbol, context, locationProvider, locationArg, MessageID.IDS_Contravariantly);
				return true;
			}
			return false;
		}
		case SymbolKind.ArrayType:
			return IsVarianceUnsafe(((ArrayTypeSymbol)type).ElementType, requireOutputSafety, requireInputSafety, context, locationProvider, locationArg, diagnostics);
		case SymbolKind.ErrorType:
		case SymbolKind.NamedType:
			return IsVarianceUnsafe((NamedTypeSymbol)type.TupleUnderlyingTypeOrSelf(), requireOutputSafety, requireInputSafety, context, locationProvider, locationArg, diagnostics);
		default:
			return false;
		}
	}

	private static bool IsVarianceUnsafe<T>(NamedTypeSymbol namedType, bool requireOutputSafety, bool requireInputSafety, Symbol context, LocationProvider<T> locationProvider, T locationArg, DiagnosticBag diagnostics) where T : Symbol
	{
		switch (namedType.TypeKind)
		{
		default:
			return false;
		case TypeKind.Class:
		case TypeKind.Delegate:
		case TypeKind.Enum:
		case TypeKind.Error:
		case TypeKind.Interface:
		case TypeKind.Struct:
			break;
		}
		while ((object)namedType != null)
		{
			for (int i = 0; i < namedType.Arity; i++)
			{
				TypeParameterSymbol typeParameterSymbol = namedType.TypeParameters[i];
				TypeSymbol type = namedType.TypeArgumentsNoUseSiteDiagnostics[i];
				bool requireOutputSafety2;
				bool requireInputSafety2;
				switch (typeParameterSymbol.Variance)
				{
				case VarianceKind.Out:
					requireOutputSafety2 = requireOutputSafety;
					requireInputSafety2 = requireInputSafety;
					break;
				case VarianceKind.In:
					requireOutputSafety2 = requireInputSafety;
					requireInputSafety2 = requireOutputSafety;
					break;
				case VarianceKind.None:
					requireInputSafety2 = true;
					requireOutputSafety2 = true;
					break;
				default:
					throw ExceptionUtilities.UnexpectedValue(typeParameterSymbol.Variance);
				}
				if (IsVarianceUnsafe(type, requireOutputSafety2, requireInputSafety2, context, locationProvider, locationArg, diagnostics))
				{
					return true;
				}
			}
			namedType = namedType.ContainingType;
		}
		return false;
	}

	private static void AddVarianceError<T>(this DiagnosticBag diagnostics, TypeParameterSymbol unsafeTypeParameter, Symbol context, LocationProvider<T> locationProvider, T locationArg, MessageID expectedVariance) where T : Symbol
	{
		MessageID id = unsafeTypeParameter.Variance switch
		{
			VarianceKind.In => MessageID.IDS_Contravariant, 
			VarianceKind.Out => MessageID.IDS_Covariant, 
			_ => throw ExceptionUtilities.UnexpectedValue(unsafeTypeParameter.Variance), 
		};
		Location location = locationProvider(locationArg) ?? unsafeTypeParameter.Locations[0];
		diagnostics.Add(ErrorCode.ERR_UnexpectedVariance, location, context, unsafeTypeParameter, id.Localize(), expectedVariance.Localize());
	}

	private static T GetDeclaringSyntax<T>(this Symbol symbol) where T : SyntaxNode
	{
		ImmutableArray<SyntaxReference> declaringSyntaxReferences = symbol.DeclaringSyntaxReferences;
		if (declaringSyntaxReferences.Length == 0)
		{
			return null;
		}
		return declaringSyntaxReferences[0].GetSyntax() as T;
	}
}
