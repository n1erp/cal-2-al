using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis.CSharp.Emit;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal class SourceFixedFieldSymbol : SourceMemberFieldSymbolFromDeclarator
{
	private const int FixedSizeNotInitialized = -1;

	private int _fixedSize = -1;

	public sealed override int FixedSize
	{
		get
		{
			if (_fixedSize == -1)
			{
				DiagnosticBag instance = DiagnosticBag.GetInstance();
				int value = 0;
				VariableDeclaratorSyntax variableDeclaratorNode = VariableDeclaratorNode;
				if (variableDeclaratorNode.ArgumentList != null)
				{
					SeparatedSyntaxList<ArgumentSyntax> arguments = variableDeclaratorNode.ArgumentList.Arguments;
					if (arguments.Count != 0 && arguments[0].Expression.Kind() != SyntaxKind.OmittedArraySizeExpression)
					{
						if (arguments.Count > 1)
						{
							instance.Add(ErrorCode.ERR_FixedBufferTooManyDimensions, variableDeclaratorNode.ArgumentList.Location);
						}
						ExpressionSyntax expression = arguments[0].Expression;
						Binder binder = DeclaringCompilation.GetBinderFactory(SyntaxTree).GetBinder(expression);
						binder = new ExecutableCodeBinder(expression, binder.ContainingMemberOrLambda, binder).GetBinder(expression);
						TypeSymbol specialType = binder.GetSpecialType(SpecialType.System_Int32, instance, expression);
						ConstantValue andValidateConstantValue = ConstantValueUtils.GetAndValidateConstantValue(binder.GenerateConversionForAssignment(specialType, binder.BindValue(expression, instance, Binder.BindValueKind.RValue), instance), this, specialType, expression.Location, instance);
						if (andValidateConstantValue.IsIntegral)
						{
							int int32Value = andValidateConstantValue.Int32Value;
							if (int32Value > 0)
							{
								value = int32Value;
								TypeSymbol pointedAtType = ((PointerTypeSymbol)Type).PointedAtType;
								if ((long)pointedAtType.FixedBufferElementSizeInBytes() * (long)int32Value > int.MaxValue)
								{
									instance.Add(ErrorCode.ERR_FixedOverflow, expression.Location, int32Value, pointedAtType);
								}
							}
							else
							{
								instance.Add(ErrorCode.ERR_InvalidFixedArraySize, expression.Location);
							}
						}
					}
				}
				if (Interlocked.CompareExchange(ref _fixedSize, value, -1) == -1)
				{
					AddDeclarationDiagnostics(instance);
					state.NotePartComplete(CompletionPart.Members);
				}
				instance.Free();
			}
			return _fixedSize;
		}
	}

	internal SourceFixedFieldSymbol(SourceMemberContainerTypeSymbol containingType, VariableDeclaratorSyntax declarator, DeclarationModifiers modifiers, bool modifierErrors, DiagnosticBag diagnostics)
		: base(containingType, declarator, modifiers, modifierErrors, diagnostics)
	{
	}

	internal override void AddSynthesizedAttributes(PEModuleBuilder moduleBuilder, ref ArrayBuilder<SynthesizedAttributeData> attributes)
	{
		base.AddSynthesizedAttributes(moduleBuilder, ref attributes);
		CSharpCompilation declaringCompilation = DeclaringCompilation;
		NamedTypeSymbol wellKnownType = declaringCompilation.GetWellKnownType(WellKnownType.System_Type);
		NamedTypeSymbol specialType = declaringCompilation.GetSpecialType(SpecialType.System_Int32);
		TypedConstant item = new TypedConstant(wellKnownType, TypedConstantKind.Type, ((PointerTypeSymbol)Type).PointedAtType);
		TypedConstant item2 = new TypedConstant(specialType, TypedConstantKind.Primitive, FixedSize);
		Symbol.AddSynthesizedAttribute(ref attributes, declaringCompilation.TrySynthesizeAttribute(WellKnownMember.System_Runtime_CompilerServices_FixedBufferAttribute__ctor, ImmutableArray.Create(item, item2)));
	}

	internal override NamedTypeSymbol FixedImplementationType(PEModuleBuilder emitModule)
	{
		return emitModule.SetFixedImplementationType(this);
	}
}
