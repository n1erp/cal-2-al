using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Rewriter;

internal static class TriggerToEventRewriter
{
	private const int SystemEventEmitterUpgradeCodeunitId = 2000000008;

	private const int SystemEventEmitterExtensionCodeunitId = 2000000010;

	public static MethodSymbol Rewrite(ModuleCompilationState compilationState, MethodSymbol sourceSymbol)
	{
		if (!sourceSymbol.IsSourceSymbol() || !sourceSymbol.IsEventSubscriberTrigger())
		{
			return sourceSymbol;
		}
		string name = sourceSymbol.Name;
		string metadataName = sourceSymbol.MetadataName;
		TypeSymbol returnType = sourceSymbol.ReturnValue?.Type;
		ImmutableArray<ParameterSymbol> parameters = sourceSymbol.Parameters;
		ImmutableArray<AttributeSymbol> attributes = CreateEventSubscriberAttribute(compilationState, (SourceMethodOrTriggerSymbol)sourceSymbol);
		ImmutableArray<VariableSymbol> localVariables = sourceSymbol.LocalVariables;
		return new SynthesizedMethodSymbol(sourceSymbol.ContainingSymbol, name, metadataName, returnType, parameters, attributes, localVariables);
	}

	private static ImmutableArray<AttributeSymbol> CreateEventSubscriberAttribute(ModuleCompilationState compilationState, SourceMethodOrTriggerSymbol methodSymbol)
	{
		DiagnosticBag instance = DiagnosticBag.GetInstance();
		try
		{
			SynthesizedAttributeSymbol item = ConstructSynthesizedEventEmitterAttributeSymbol(compilationState, methodSymbol, instance);
			return methodSymbol.Attributes.Add(item);
		}
		finally
		{
			instance.Free();
		}
	}

	private static SynthesizedAttributeSymbol ConstructSynthesizedEventEmitterAttributeSymbol(ModuleCompilationState compilationState, SourceMethodOrTriggerSymbol methodSymbol, DiagnosticBag diagnostics)
	{
		BoundAttribute attribute = ConstructSyntheticEventEmitterAttribute(compilationState, methodSymbol, diagnostics);
		AttributeTypeInfo attributeInfo = AttributeDefinitions.GetAttributeInfo(AttributeKind.EventSubscriber.ToString());
		ImmutableArray<AttributeArgumentSymbol> arguments = ImmutableArray.Create(new AttributeArgumentSymbol[6]
		{
			new SynthesizedAttributeArgumentSymbol(methodSymbol, NavCorLib.ObjectTypeType, SymbolKind.Codeunit.ToString()),
			new SynthesizedAttributeArgumentSymbol(methodSymbol, NavCorLib.IntegerType, GetCodeunitId(methodSymbol).ToString(CultureInfo.InvariantCulture)),
			new SynthesizedAttributeArgumentSymbol(methodSymbol, NavCorLib.TextType, methodSymbol.Name),
			new SynthesizedAttributeArgumentSymbol(methodSymbol, NavCorLib.TextType, string.Empty),
			new SynthesizedAttributeArgumentSymbol(methodSymbol, NavCorLib.BooleanType, false.ToString()),
			new SynthesizedAttributeArgumentSymbol(methodSymbol, NavCorLib.BooleanType, false.ToString())
		});
		return new SynthesizedAttributeSymbol(methodSymbol, attribute, arguments, attributeInfo);
	}

	private static BoundAttribute ConstructSyntheticEventEmitterAttribute(ModuleCompilationState compilationState, SourceMethodOrTriggerSymbol methodSymbol, DiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(methodSymbol, methodSymbol.SyntaxNode, compilationState, diagnostics);
		BoundOptionAccess boundOptionAccess = syntheticBoundNodeFactory.OptionAccess(syntheticBoundNodeFactory.SystemOption((SystemOptionTypeSymbol)NavCorLib.ObjectTypeType), (OptionSymbol)NavCorLib.ObjectTypeType.GetMembers(SymbolKind.Codeunit.ToString()).First());
		BoundLiteral boundLiteral = syntheticBoundNodeFactory.Literal(GetCodeunitId(methodSymbol));
		BoundLiteral boundLiteral2 = syntheticBoundNodeFactory.Literal(methodSymbol.Name);
		BoundLiteral boundLiteral3 = syntheticBoundNodeFactory.Literal(string.Empty);
		BoundLiteral boundLiteral4 = syntheticBoundNodeFactory.Literal(value: false);
		BoundLiteral boundLiteral5 = syntheticBoundNodeFactory.Literal(value: false);
		return syntheticBoundNodeFactory.Attribute(boundOptionAccess, boundLiteral, boundLiteral2, boundLiteral3, boundLiteral4, boundLiteral5);
	}

	private static int GetCodeunitId(SourceMethodOrTriggerSymbol sourceSymbol)
	{
		switch (((SourceTriggerSymbol)sourceSymbol).TriggerInfo?.Kind)
		{
		case TriggerTypeKind.CodeunitOnCheckPreconditionsPerDatabase:
		case TriggerTypeKind.CodeunitOnCheckPreconditionsPerCompany:
		case TriggerTypeKind.CodeunitOnUpgradePerDatabase:
		case TriggerTypeKind.CodeunitOnUpgradePerCompany:
		case TriggerTypeKind.CodeunitOnValidateUpgradePerDatabase:
		case TriggerTypeKind.CodeunitOnValidateUpgradePerCompany:
			return 2000000008;
		case TriggerTypeKind.CodeunitOnInstallAppPerDatabase:
		case TriggerTypeKind.CodeunitOnInstallAppPerCompany:
			return 2000000010;
		default:
			return 0;
		}
	}

	internal static bool IsEventSubscriberTrigger(this MethodSymbol sourceSymbol)
	{
		if (sourceSymbol.MethodKind != MethodKind.Trigger)
		{
			return false;
		}
		TriggerTypeKind? triggerTypeKind = ((SourceTriggerSymbol)sourceSymbol).TriggerInfo?.Kind;
		if (triggerTypeKind.HasValue)
		{
			TriggerTypeKind valueOrDefault = triggerTypeKind.GetValueOrDefault();
			if ((uint)(valueOrDefault - 1) <= 7u)
			{
				return true;
			}
		}
		return false;
	}
}
