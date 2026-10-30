using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CodeGen;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal sealed class SynthesizedStringSwitchHashMethod : SynthesizedGlobalMethodSymbol
{
	internal static uint ComputeStringHash(string text)
	{
		uint num = 0u;
		if (text != null)
		{
			num = 2166136261u;
			for (int i = 0; i < text.Length; i++)
			{
				num = (text[i] ^ num) * 16777619;
			}
		}
		return num;
	}

	internal override void GenerateMethodBody(TypeCompilationState compilationState, DiagnosticBag diagnostics)
	{
		SyntheticBoundNodeFactory syntheticBoundNodeFactory = new SyntheticBoundNodeFactory(this, this.GetNonNullSyntaxNode(), compilationState, diagnostics);
		syntheticBoundNodeFactory.CurrentFunction = this;
		try
		{
			LocalSymbol localSymbol = syntheticBoundNodeFactory.SynthesizedLocal(syntheticBoundNodeFactory.SpecialType(SpecialType.System_Int32));
			LocalSymbol localSymbol2 = syntheticBoundNodeFactory.SynthesizedLocal(syntheticBoundNodeFactory.SpecialType(SpecialType.System_UInt32));
			LabelSymbol label = syntheticBoundNodeFactory.GenerateLabel("again");
			LabelSymbol label2 = syntheticBoundNodeFactory.GenerateLabel("start");
			ParameterSymbol parameterSymbol = Parameters[0];
			BoundBlock body = syntheticBoundNodeFactory.Block(ImmutableArray.Create(localSymbol2, localSymbol), syntheticBoundNodeFactory.If(syntheticBoundNodeFactory.Binary(BinaryOperatorKind.ObjectNotEqual, syntheticBoundNodeFactory.SpecialType(SpecialType.System_Boolean), syntheticBoundNodeFactory.Parameter(parameterSymbol), syntheticBoundNodeFactory.Null(parameterSymbol.Type)), syntheticBoundNodeFactory.Block(syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Local(localSymbol2), syntheticBoundNodeFactory.Literal(2166136261u)), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Local(localSymbol), syntheticBoundNodeFactory.Literal(0)), syntheticBoundNodeFactory.Goto(label2), syntheticBoundNodeFactory.Label(label), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Local(localSymbol2), syntheticBoundNodeFactory.Binary(BinaryOperatorKind.Multiplication, localSymbol2.Type, syntheticBoundNodeFactory.Binary(BinaryOperatorKind.Xor, localSymbol2.Type, syntheticBoundNodeFactory.Convert(localSymbol2.Type, syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.Parameter(parameterSymbol), syntheticBoundNodeFactory.SpecialMethod(SpecialMember.System_String__Chars), syntheticBoundNodeFactory.Local(localSymbol)), Conversion.ImplicitNumeric), syntheticBoundNodeFactory.Local(localSymbol2)), syntheticBoundNodeFactory.Literal(16777619))), syntheticBoundNodeFactory.Assignment(syntheticBoundNodeFactory.Local(localSymbol), syntheticBoundNodeFactory.Binary(BinaryOperatorKind.Addition, localSymbol.Type, syntheticBoundNodeFactory.Local(localSymbol), syntheticBoundNodeFactory.Literal(1))), syntheticBoundNodeFactory.Label(label2), syntheticBoundNodeFactory.If(syntheticBoundNodeFactory.Binary(BinaryOperatorKind.LessThan, syntheticBoundNodeFactory.SpecialType(SpecialType.System_Boolean), syntheticBoundNodeFactory.Local(localSymbol), syntheticBoundNodeFactory.Call(syntheticBoundNodeFactory.Parameter(parameterSymbol), syntheticBoundNodeFactory.SpecialMethod(SpecialMember.System_String__Length))), syntheticBoundNodeFactory.Goto(label)))), syntheticBoundNodeFactory.Return(syntheticBoundNodeFactory.Local(localSymbol2)));
			syntheticBoundNodeFactory.CloseMethod(body);
		}
		catch (SyntheticBoundNodeFactory.MissingPredefinedMember missingPredefinedMember)
		{
			diagnostics.Add(missingPredefinedMember.Diagnostic);
			syntheticBoundNodeFactory.CloseMethod(syntheticBoundNodeFactory.ThrowNull());
		}
	}

	internal SynthesizedStringSwitchHashMethod(SourceModuleSymbol containingModule, PrivateImplementationDetails privateImplType, TypeSymbol returnType, TypeSymbol paramType)
		: base(containingModule, privateImplType, returnType, "ComputeStringHash")
	{
		SetParameters(ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, paramType, 0, RefKind.None, "s")));
	}
}
