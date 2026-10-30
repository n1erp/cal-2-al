using System;
using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Text;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

internal static class TriggerValidationHelper
{
	public static void CheckTriggers(Symbol containingSymbol, ImmutableArray<MethodSymbol> triggers, Func<string, TriggerTypeInfo> getTriggerTypeInfo, DiagnosticBag diagnostics)
	{
		DebugAssertHelper.Assert(getTriggerTypeInfo != null);
		if (triggers.IsDefaultOrEmpty)
		{
			return;
		}
		PooledNameComparisonHashSet instance = PooledNameComparisonHashSet.GetInstance();
		try
		{
			foreach (MethodSymbol item in triggers)
			{
				if (instance.Contains(item.Name))
				{
					diagnostics.Add(ErrorCode.ERR_DuplicateTrigger, item.Locations.IsEmpty ? Location.None : item.Locations[0], item.Name);
				}
				else
				{
					instance.Add(item.Name);
				}
				TriggerTypeInfo triggerTypeInfo = getTriggerTypeInfo(item.Name);
				if (triggerTypeInfo != null)
				{
					CheckTrigger(containingSymbol, item, triggerTypeInfo, diagnostics);
				}
			}
		}
		finally
		{
			instance.Free();
		}
	}

	private static void CheckTrigger(Symbol containingSymbol, MethodSymbol trigger, TriggerTypeInfo info, DiagnosticBag diagnostics)
	{
		if (!HasValidSignature(trigger, info))
		{
			diagnostics.Add(ErrorCode.ERR_InvalidTriggerSignature, trigger.Locations.IsEmpty ? Location.None : trigger.Locations[0], info.Name, info.GetSignatureString());
		}
		foreach (DependentProperty dependentParentProperty in info.DependentParentProperties)
		{
			if (!SymbolWithPropertiesVisitor.IsPropertyDependencySatisfied(containingSymbol, dependentParentProperty))
			{
				SetError(trigger, containingSymbol, dependentParentProperty, diagnostics);
			}
		}
	}

	private static void SetError(MethodSymbol trigger, Symbol containingSymbol, DependentProperty dependentProperty, DiagnosticBag diagnostics)
	{
		switch (dependentProperty.AllowedValues.Count)
		{
		case 0:
		{
			ErrorCode code = ErrorCode.ERR_TriggerMissingAssociatedProperty;
			diagnostics.Add(code, trigger.GetLocation(), trigger.Name, dependentProperty.Name, containingSymbol.Name);
			break;
		}
		case 1:
		{
			ErrorCode code = ErrorCode.ERR_TriggerMissingAssociatedPropertySingleValue;
			diagnostics.Add(code, trigger.GetLocation(), trigger.Name, dependentProperty.Name, containingSymbol.Name, dependentProperty.ConcatenateValues());
			break;
		}
		default:
		{
			ErrorCode code = ErrorCode.ERR_TriggerMissingAssociatedPropertyMultipleValues;
			diagnostics.Add(code, trigger.GetLocation(), trigger.Name, dependentProperty.Name, containingSymbol.Name, dependentProperty.ConcatenateValues());
			break;
		}
		}
	}

	private static bool HasValidSignature(MethodSymbol trigger, TriggerTypeInfo info)
	{
		if (trigger.ReturnType != info.ReturnType)
		{
			return false;
		}
		if (trigger.ParameterCount != info.Parameters.Length)
		{
			return false;
		}
		for (int i = 0; i < trigger.ParameterCount; i++)
		{
			if (!trigger.Parameters[i].Type.Equals(info.Parameters[i].Type))
			{
				return false;
			}
		}
		return true;
	}
}
