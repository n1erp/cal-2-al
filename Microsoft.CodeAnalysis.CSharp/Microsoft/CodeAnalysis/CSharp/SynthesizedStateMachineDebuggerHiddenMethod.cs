using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class SynthesizedStateMachineDebuggerHiddenMethod : SynthesizedStateMachineMethod
{
	public SynthesizedStateMachineDebuggerHiddenMethod(string name, MethodSymbol interfaceMethod, StateMachineTypeSymbol stateMachineType, PropertySymbol associatedProperty, bool hasMethodBodyDependency)
		: base(name, interfaceMethod, stateMachineType, associatedProperty, generateDebugInfo: false, hasMethodBodyDependency)
	{
	}

	internal sealed override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Diagnostics_DebuggerHiddenAttribute__ctor));
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
	}
}
