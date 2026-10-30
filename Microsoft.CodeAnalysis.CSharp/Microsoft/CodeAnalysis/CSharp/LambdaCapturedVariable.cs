using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class LambdaCapturedVariable : SynthesizedFieldSymbolBase
{
	private readonly TypeSymbol _type;

	private readonly bool _isThis;

	internal override bool IsCapturedFrame => _isThis;

	internal override bool SuppressDynamicAttribute => false;

	private LambdaCapturedVariable(SynthesizedContainer frame, TypeSymbol type, string fieldName, bool isThisParameter)
		: base(frame, fieldName, isPublic: true, isReadOnly: false, isStatic: false)
	{
		_type = type;
		_isThis = isThisParameter;
	}

	public static LambdaCapturedVariable Create(SynthesizedClosureEnvironment frame, Symbol captured, ref int uniqueId)
	{
		string capturedVariableFieldName = GetCapturedVariableFieldName(captured, ref uniqueId);
		TypeSymbol capturedVariableFieldType = GetCapturedVariableFieldType(frame, captured);
		return new LambdaCapturedVariable(frame, capturedVariableFieldType, capturedVariableFieldName, IsThis(captured));
	}

	private static bool IsThis(Symbol captured)
	{
		if (captured is ParameterSymbol parameterSymbol)
		{
			return parameterSymbol.IsThis;
		}
		return false;
	}

	private static string GetCapturedVariableFieldName(Symbol variable, ref int uniqueId)
	{
		if (IsThis(variable))
		{
			return GeneratedNames.ThisProxyFieldName();
		}
		if (variable is LocalSymbol localSymbol)
		{
			if (localSymbol.SynthesizedKind == SynthesizedLocalKind.LambdaDisplayClass)
			{
				return GeneratedNames.MakeLambdaDisplayLocalName(uniqueId++);
			}
			if (localSymbol.SynthesizedKind == SynthesizedLocalKind.ExceptionFilterAwaitHoistedExceptionLocal)
			{
				return GeneratedNames.MakeHoistedLocalFieldName(localSymbol.SynthesizedKind, uniqueId++);
			}
			if (localSymbol.SynthesizedKind == SynthesizedLocalKind.InstrumentationPayload)
			{
				return GeneratedNames.MakeSynthesizedInstrumentationPayloadLocalFieldName(uniqueId++);
			}
			if (localSymbol.SynthesizedKind == SynthesizedLocalKind.UserDefined)
			{
				SyntaxNode scopeDesignatorOpt = localSymbol.ScopeDesignatorOpt;
				if (scopeDesignatorOpt != null && scopeDesignatorOpt.Kind() == SyntaxKind.SwitchSection)
				{
					return GeneratedNames.MakeHoistedLocalFieldName(localSymbol.SynthesizedKind, uniqueId++, localSymbol.Name);
				}
			}
		}
		return variable.Name;
	}

	private static TypeSymbol GetCapturedVariableFieldType(SynthesizedContainer frame, Symbol variable)
	{
		LocalSymbol localSymbol = variable as LocalSymbol;
		if ((object)localSymbol != null && localSymbol.Type.OriginalDefinition is SynthesizedClosureEnvironment synthesizedClosureEnvironment)
		{
			ImmutableArray<TypeSymbol> items = frame.TypeArgumentsNoUseSiteDiagnostics;
			if (items.Length > synthesizedClosureEnvironment.Arity)
			{
				items = ImmutableArray.Create(items, 0, synthesizedClosureEnvironment.Arity);
			}
			return synthesizedClosureEnvironment.ConstructIfGeneric(items.SelectAsArray(TypeMap.TypeSymbolAsTypeWithModifiers));
		}
		return frame.TypeMap.SubstituteType(((object)localSymbol != null) ? localSymbol.Type : ((ParameterSymbol)variable).Type).Type;
	}

	internal override TypeSymbol GetFieldType(ConsList<FieldSymbol> fieldsBeingBound)
	{
		return _type;
	}
}
