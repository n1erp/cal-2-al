using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SynthesizedStateMachineMoveNextMethod : SynthesizedStateMachineMethod
{
	private ImmutableArray<CSharpAttributeData> _attributes;

	public SynthesizedStateMachineMoveNextMethod(MethodSymbol interfaceMethod, StateMachineTypeSymbol stateMachineType)
		: base("MoveNext", interfaceMethod, stateMachineType, null, generateDebugInfo: true, hasMethodBodyDependency: true)
	{
	}

	public override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_attributes.IsDefault)
		{
			ArrayBuilder<CSharpAttributeData> arrayBuilder = null;
			MethodSymbol kickoffMethod = StateMachineType.KickoffMethod;
			foreach (CSharpAttributeData attribute in kickoffMethod.GetAttributes())
			{
				if (attribute.IsTargetAttribute(kickoffMethod, AttributeDescription.DebuggerHiddenAttribute) || attribute.IsTargetAttribute(kickoffMethod, AttributeDescription.DebuggerNonUserCodeAttribute) || attribute.IsTargetAttribute(kickoffMethod, AttributeDescription.DebuggerStepperBoundaryAttribute) || attribute.IsTargetAttribute(kickoffMethod, AttributeDescription.DebuggerStepThroughAttribute))
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<CSharpAttributeData>.GetInstance(4);
					}
					arrayBuilder.Add(attribute);
				}
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref _attributes, arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<CSharpAttributeData>.Empty, default);
		}
		return _attributes;
	}
}
