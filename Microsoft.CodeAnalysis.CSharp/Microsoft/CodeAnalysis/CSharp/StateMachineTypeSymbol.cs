using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class StateMachineTypeSymbol : SynthesizedContainer, ISynthesizedMethodBodyImplementationSymbol
{
	private ImmutableArray<CSharpAttributeData> _attributes;

	public readonly MethodSymbol KickoffMethod;

	public override Symbol ContainingSymbol => KickoffMethod.ContainingType;

	bool ISynthesizedMethodBodyImplementationSymbol.HasMethodBodyDependency => true;

	IMethodSymbol ISynthesizedMethodBodyImplementationSymbol.Method => KickoffMethod;

	internal override bool HasCodeAnalysisEmbeddedAttribute => false;

	public StateMachineTypeSymbol(VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, MethodSymbol kickoffMethod, int kickoffMethodOrdinal)
		: base(MakeName(slotAllocatorOpt, compilationState, kickoffMethod, kickoffMethodOrdinal), kickoffMethod)
	{
		KickoffMethod = kickoffMethod;
	}

	private static string MakeName(VariableSlotAllocator slotAllocatorOpt, TypeCompilationState compilationState, MethodSymbol kickoffMethod, int kickoffMethodOrdinal)
	{
		return slotAllocatorOpt?.PreviousStateMachineTypeName ?? GeneratedNames.MakeStateMachineTypeName(kickoffMethod.Name, kickoffMethodOrdinal, compilationState.ModuleBuilderOpt.CurrentGenerationOrdinal);
	}

	private static int SequenceNumber(MethodSymbol kickoffMethod)
	{
		int num = 0;
		foreach (Symbol member in kickoffMethod.ContainingType.GetMembers(kickoffMethod.Name))
		{
			num++;
			if ((object)kickoffMethod == member)
			{
				return num;
			}
		}
		return num;
	}

	public sealed override ImmutableArray<CSharpAttributeData> GetAttributes()
	{
		if (_attributes.IsDefault)
		{
			ArrayBuilder<CSharpAttributeData> arrayBuilder = null;
			NamedTypeSymbol containingType = KickoffMethod.ContainingType;
			foreach (CSharpAttributeData attribute in containingType.GetAttributes())
			{
				if (attribute.IsTargetAttribute(containingType, AttributeDescription.DebuggerNonUserCodeAttribute) || attribute.IsTargetAttribute(containingType, AttributeDescription.DebuggerStepThroughAttribute))
				{
					if (arrayBuilder == null)
					{
						arrayBuilder = ArrayBuilder<CSharpAttributeData>.GetInstance(2);
					}
					arrayBuilder.Add(attribute);
				}
			}
			ImmutableInterlocked.InterlockedCompareExchange(ref _attributes, arrayBuilder?.ToImmutableAndFree() ?? ImmutableArray<CSharpAttributeData>.Empty, default);
		}
		return _attributes;
	}
}
