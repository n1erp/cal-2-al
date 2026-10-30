using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.PooledObjects;
using Microsoft.CodeAnalysis.RuntimeMembers;

namespace Microsoft.CodeAnalysis.CSharp;

internal static class MethodBodySynthesizer
{
	internal static ImmutableArray<BoundStatement> ConstructScriptConstructorBody(BoundStatement loweredBody, MethodSymbol constructor, SynthesizedSubmissionFields previousSubmissionFields, CSharpCompilation compilation)
	{
		SyntaxNode syntax = loweredBody.Syntax;
		NamedTypeSymbol specialType = constructor.ContainingAssembly.GetSpecialType(SpecialType.System_Object);
		BoundExpression receiverOpt = new BoundThisReference(syntax, constructor.ContainingType)
		{
			WasCompilerGenerated = true
		};
		BoundStatement item = new BoundExpressionStatement(syntax, new BoundCall(syntax, receiverOpt, specialType.InstanceConstructors[0], ImmutableArray<BoundExpression>.Empty, ImmutableArray<string>.Empty, ImmutableArray<RefKind>.Empty, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, ImmutableArray<int>.Empty, LookupResultKind.Viable, null, specialType)
		{
			WasCompilerGenerated = true
		})
		{
			WasCompilerGenerated = true
		};
		ArrayBuilder<BoundStatement> instance = ArrayBuilder<BoundStatement>.GetInstance();
		instance.Add(item);
		if (constructor.IsSubmissionConstructor)
		{
			MakeSubmissionInitialization(instance, syntax, constructor, previousSubmissionFields, compilation);
		}
		instance.Add(loweredBody);
		return instance.ToImmutableAndFree();
	}

	private static void MakeSubmissionInitialization(ArrayBuilder<BoundStatement> statements, SyntaxNode syntax, MethodSymbol submissionConstructor, SynthesizedSubmissionFields synthesizedFields, CSharpCompilation compilation)
	{
		BoundParameter expression = new BoundParameter(syntax, submissionConstructor.Parameters[0])
		{
			WasCompilerGenerated = true
		};
		NamedTypeSymbol specialType = compilation.GetSpecialType(SpecialType.System_Int32);
		NamedTypeSymbol specialType2 = compilation.GetSpecialType(SpecialType.System_Object);
		BoundThisReference boundThisReference = new BoundThisReference(syntax, submissionConstructor.ContainingType)
		{
			WasCompilerGenerated = true
		};
		int submissionSlotIndex = compilation.GetSubmissionSlotIndex();
		statements.Add(new BoundExpressionStatement(syntax, new BoundAssignmentOperator(syntax, new BoundArrayAccess(syntax, expression, ImmutableArray.Create((BoundExpression)new BoundLiteral(syntax, ConstantValue.Create(submissionSlotIndex), specialType)
		{
			WasCompilerGenerated = true
		}), specialType2)
		{
			WasCompilerGenerated = true
		}, boundThisReference, isRef: false, boundThisReference.Type)
		{
			WasCompilerGenerated = true
		})
		{
			WasCompilerGenerated = true
		});
		FieldSymbol hostObjectField = synthesizedFields.GetHostObjectField();
		if ((object)hostObjectField != null)
		{
			statements.Add(new BoundExpressionStatement(syntax, new BoundAssignmentOperator(syntax, new BoundFieldAccess(syntax, boundThisReference, hostObjectField, null)
			{
				WasCompilerGenerated = true
			}, BoundConversion.Synthesized(syntax, new BoundArrayAccess(syntax, expression, ImmutableArray.Create((BoundExpression)new BoundLiteral(syntax, ConstantValue.Create(0), specialType)
			{
				WasCompilerGenerated = true
			}), specialType2), Conversion.ExplicitReference, @checked: false, explicitCastInCode: true, null, hostObjectField.Type), hostObjectField.Type)
			{
				WasCompilerGenerated = true
			})
			{
				WasCompilerGenerated = true
			});
		}
		foreach (FieldSymbol fieldSymbol in synthesizedFields.FieldSymbols)
		{
			ImplicitNamedTypeSymbol implicitNamedTypeSymbol = (ImplicitNamedTypeSymbol)fieldSymbol.Type;
			int submissionSlotIndex2 = implicitNamedTypeSymbol.DeclaringCompilation.GetSubmissionSlotIndex();
			statements.Add(new BoundExpressionStatement(syntax, new BoundAssignmentOperator(syntax, new BoundFieldAccess(syntax, boundThisReference, fieldSymbol, null)
			{
				WasCompilerGenerated = true
			}, BoundConversion.Synthesized(syntax, new BoundArrayAccess(syntax, expression, ImmutableArray.Create((BoundExpression)new BoundLiteral(syntax, ConstantValue.Create(submissionSlotIndex2), specialType)
			{
				WasCompilerGenerated = true
			}), specialType2)
			{
				WasCompilerGenerated = true
			}, Conversion.ExplicitReference, @checked: false, explicitCastInCode: true, null, implicitNamedTypeSymbol), implicitNamedTypeSymbol)
			{
				WasCompilerGenerated = true
			})
			{
				WasCompilerGenerated = true
			});
		}
	}

	internal static BoundBlock ConstructAutoPropertyAccessorBody(SourceMemberMethodSymbol accessor)
	{
		SourcePropertySymbol sourcePropertySymbol = (SourcePropertySymbol)accessor.AssociatedSymbol;
		CSharpSyntaxNode cSharpSyntaxNode = sourcePropertySymbol.CSharpSyntaxNode;
		BoundExpression receiver = null;
		if (!accessor.IsStatic)
		{
			ParameterSymbol thisParameter = accessor.ThisParameter;
			receiver = new BoundThisReference(cSharpSyntaxNode, thisParameter.Type)
			{
				WasCompilerGenerated = true
			};
		}
		SynthesizedBackingFieldSymbol backingField = sourcePropertySymbol.BackingField;
		BoundFieldAccess boundFieldAccess = new BoundFieldAccess(cSharpSyntaxNode, receiver, backingField, null)
		{
			WasCompilerGenerated = true
		};
		BoundStatement statement;
		if (accessor.MethodKind == MethodKind.PropertyGet)
		{
			statement = new BoundReturnStatement(accessor.SyntaxNode, RefKind.None, boundFieldAccess);
		}
		else
		{
			ParameterSymbol parameterSymbol = accessor.Parameters[0];
			statement = new BoundExpressionStatement(accessor.SyntaxNode, new BoundAssignmentOperator(cSharpSyntaxNode, boundFieldAccess, new BoundParameter(cSharpSyntaxNode, parameterSymbol)
			{
				WasCompilerGenerated = true
			}, sourcePropertySymbol.Type)
			{
				WasCompilerGenerated = true
			});
		}
		return BoundBlock.SynthesizedNoLocals(cSharpSyntaxNode, statement);
	}

	internal static BoundBlock ConstructFieldLikeEventAccessorBody(SourceEventSymbol eventSymbol, bool isAddMethod, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		if (!eventSymbol.IsWindowsRuntimeEvent)
		{
			return ConstructFieldLikeEventAccessorBody_Regular(eventSymbol, isAddMethod, compilation, diagnostics);
		}
		return ConstructFieldLikeEventAccessorBody_WinRT(eventSymbol, isAddMethod, compilation, diagnostics);
	}

	internal static BoundBlock ConstructFieldLikeEventAccessorBody_WinRT(SourceEventSymbol eventSymbol, bool isAddMethod, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		CSharpSyntaxNode cSharpSyntaxNode = eventSymbol.CSharpSyntaxNode;
		MethodSymbol methodSymbol = (isAddMethod ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
		FieldSymbol associatedField = eventSymbol.AssociatedField;
		NamedTypeSymbol newOwner = (NamedTypeSymbol)associatedField.Type;
		MethodSymbol methodSymbol2 = (MethodSymbol)Binder.GetWellKnownTypeMember(compilation, WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T__GetOrCreateEventRegistrationTokenTable, diagnostics, null, cSharpSyntaxNode);
		if ((object)methodSymbol2 == null)
		{
			return null;
		}
		methodSymbol2 = methodSymbol2.AsMember(newOwner);
		WellKnownMember member = (isAddMethod ? WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T__AddEventHandler : WellKnownMember.System_Runtime_InteropServices_WindowsRuntime_EventRegistrationTokenTable_T__RemoveEventHandler);
		MethodSymbol methodSymbol3 = (MethodSymbol)Binder.GetWellKnownTypeMember(compilation, member, diagnostics, null, cSharpSyntaxNode);
		if ((object)methodSymbol3 == null)
		{
			return null;
		}
		methodSymbol3 = methodSymbol3.AsMember(newOwner);
		BoundFieldAccess arg = new BoundFieldAccess(cSharpSyntaxNode, associatedField.IsStatic ? null : new BoundThisReference(cSharpSyntaxNode, methodSymbol.ThisParameter.Type), associatedField, null)
		{
			WasCompilerGenerated = true
		};
		BoundCall receiverOpt = BoundCall.Synthesized(cSharpSyntaxNode, null, methodSymbol2, arg);
		BoundParameter arg2 = new BoundParameter(cSharpSyntaxNode, methodSymbol.Parameters[0]);
		BoundCall expression = BoundCall.Synthesized(cSharpSyntaxNode, receiverOpt, methodSymbol3, arg2);
		if (isAddMethod)
		{
			BoundStatement statement = BoundReturnStatement.Synthesized(cSharpSyntaxNode, RefKind.None, expression);
			return BoundBlock.SynthesizedNoLocals(cSharpSyntaxNode, statement);
		}
		BoundStatement boundStatement = new BoundExpressionStatement(cSharpSyntaxNode, expression);
		BoundStatement boundStatement2 = new BoundReturnStatement(cSharpSyntaxNode, RefKind.None, null);
		return BoundBlock.SynthesizedNoLocals(cSharpSyntaxNode, boundStatement, boundStatement2);
	}

	internal static BoundBlock ConstructFieldLikeEventAccessorBody_Regular(SourceEventSymbol eventSymbol, bool isAddMethod, CSharpCompilation compilation, DiagnosticBag diagnostics)
	{
		CSharpSyntaxNode cSharpSyntaxNode = eventSymbol.CSharpSyntaxNode;
		TypeSymbol type = eventSymbol.Type;
		MethodSymbol methodSymbol = (isAddMethod ? eventSymbol.AddMethod : eventSymbol.RemoveMethod);
		ParameterSymbol thisParameter = methodSymbol.ThisParameter;
		TypeSymbol specialType = compilation.GetSpecialType(SpecialType.System_Boolean);
		SpecialMember specialMember = (isAddMethod ? SpecialMember.System_Delegate__Combine : SpecialMember.System_Delegate__Remove);
		MethodSymbol methodSymbol2 = (MethodSymbol)compilation.GetSpecialTypeMember(specialMember);
		BoundStatement boundStatement = new BoundReturnStatement(cSharpSyntaxNode, RefKind.None, null)
		{
			WasCompilerGenerated = true
		};
		if (methodSymbol2 == null)
		{
			MemberDescriptor descriptor = SpecialMembers.GetDescriptor(specialMember);
			diagnostics.Add(new CSDiagnostic(new CSDiagnosticInfo(ErrorCode.ERR_MissingPredefinedMember, descriptor.DeclaringTypeMetadataName, descriptor.Name), cSharpSyntaxNode.Location));
			return BoundBlock.SynthesizedNoLocals(cSharpSyntaxNode, boundStatement);
		}
		Binder.ReportUseSiteDiagnostics(methodSymbol2, diagnostics, cSharpSyntaxNode);
		BoundThisReference receiver = (eventSymbol.IsStatic ? null : new BoundThisReference(cSharpSyntaxNode, thisParameter.Type)
		{
			WasCompilerGenerated = true
		});
		BoundFieldAccess boundFieldAccess = new BoundFieldAccess(cSharpSyntaxNode, receiver, eventSymbol.AssociatedField, null)
		{
			WasCompilerGenerated = true
		};
		BoundParameter item = new BoundParameter(cSharpSyntaxNode, methodSymbol.Parameters[0])
		{
			WasCompilerGenerated = true
		};
		MethodSymbol methodSymbol3 = (MethodSymbol)compilation.GetWellKnownTypeMember(WellKnownMember.System_Threading_Interlocked__CompareExchange_T);
		BoundExpression right;
		if ((object)methodSymbol3 == null)
		{
			right = BoundConversion.SynthesizedNonUserDefined(cSharpSyntaxNode, BoundCall.Synthesized(cSharpSyntaxNode, null, methodSymbol2, ImmutableArray.Create((BoundExpression)boundFieldAccess, (BoundExpression)item)), Conversion.ExplicitReference, type);
			BoundStatement item2 = new BoundExpressionStatement(cSharpSyntaxNode, new BoundAssignmentOperator(cSharpSyntaxNode, boundFieldAccess, right, type)
			{
				WasCompilerGenerated = true
			})
			{
				WasCompilerGenerated = true
			};
			return BoundBlock.SynthesizedNoLocals(cSharpSyntaxNode, ImmutableArray.Create(item2, boundStatement));
		}
		methodSymbol3 = methodSymbol3.Construct(ImmutableArray.Create(type));
		Binder.ReportUseSiteDiagnostics(methodSymbol3, diagnostics, cSharpSyntaxNode);
		GeneratedLabelSymbol label = new GeneratedLabelSymbol("loop");
		LocalSymbol[] array = new LocalSymbol[3];
		BoundLocal[] array2 = new BoundLocal[3];
		for (int i = 0; i < 3; i++)
		{
			array[i] = new SynthesizedLocal(methodSymbol, type, SynthesizedLocalKind.LoweringTemp);
			array2[i] = new BoundLocal(cSharpSyntaxNode, array[i], null, type);
		}
		BoundStatement boundStatement2 = new BoundExpressionStatement(cSharpSyntaxNode, new BoundAssignmentOperator(cSharpSyntaxNode, array2[0], boundFieldAccess, type)
		{
			WasCompilerGenerated = true
		})
		{
			WasCompilerGenerated = true
		};
		BoundStatement boundStatement3 = new BoundLabelStatement(cSharpSyntaxNode, label)
		{
			WasCompilerGenerated = true
		};
		BoundStatement boundStatement4 = new BoundExpressionStatement(cSharpSyntaxNode, new BoundAssignmentOperator(cSharpSyntaxNode, array2[1], array2[0], type)
		{
			WasCompilerGenerated = true
		})
		{
			WasCompilerGenerated = true
		};
		right = BoundConversion.SynthesizedNonUserDefined(cSharpSyntaxNode, BoundCall.Synthesized(cSharpSyntaxNode, null, methodSymbol2, ImmutableArray.Create((BoundExpression)array2[1], (BoundExpression)item)), Conversion.ExplicitReference, type);
		BoundStatement boundStatement5 = new BoundExpressionStatement(cSharpSyntaxNode, new BoundAssignmentOperator(cSharpSyntaxNode, array2[2], right, type)
		{
			WasCompilerGenerated = true
		})
		{
			WasCompilerGenerated = true
		};
		BoundExpression right2 = BoundCall.Synthesized(cSharpSyntaxNode, null, methodSymbol3, ImmutableArray.Create<BoundExpression>(boundFieldAccess, array2[2], array2[1]));
		BoundStatement boundStatement6 = new BoundExpressionStatement(cSharpSyntaxNode, new BoundAssignmentOperator(cSharpSyntaxNode, array2[0], right2, type)
		{
			WasCompilerGenerated = true
		})
		{
			WasCompilerGenerated = true
		};
		BoundExpression condition = new BoundBinaryOperator(cSharpSyntaxNode, BinaryOperatorKind.ObjectEqual, array2[0], array2[1], null, null, LookupResultKind.Viable, specialType)
		{
			WasCompilerGenerated = true
		};
		BoundStatement boundStatement7 = new BoundConditionalGoto(cSharpSyntaxNode, condition, jumpIfTrue: false, label)
		{
			WasCompilerGenerated = true
		};
		return new BoundBlock(cSharpSyntaxNode, array.AsImmutable(), ImmutableArray.Create<BoundStatement>(boundStatement2, boundStatement3, boundStatement4, boundStatement5, boundStatement6, boundStatement7, boundStatement))
		{
			WasCompilerGenerated = true
		};
	}

	internal static BoundBlock ConstructDestructorBody(MethodSymbol method, BoundBlock block)
	{
		SyntaxNode syntax = block.Syntax;
		MethodSymbol baseTypeFinalizeMethod = GetBaseTypeFinalizeMethod(method);
		if ((object)baseTypeFinalizeMethod != null)
		{
			BoundStatement boundStatement = new BoundExpressionStatement(syntax, BoundCall.Synthesized(syntax, new BoundBaseReference(syntax, method.ContainingType)
			{
				WasCompilerGenerated = true
			}, baseTypeFinalizeMethod))
			{
				WasCompilerGenerated = true
			};
			if (syntax.Kind() == SyntaxKind.Block)
			{
				boundStatement = new BoundSequencePointWithSpan(syntax, boundStatement, ((BlockSyntax)syntax).CloseBraceToken.Span);
			}
			return new BoundBlock(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create((BoundStatement)new BoundTryStatement(syntax, block, ImmutableArray<BoundCatchBlock>.Empty, new BoundBlock(syntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create(boundStatement))
			{
				WasCompilerGenerated = true
			})
			{
				WasCompilerGenerated = true
			}));
		}
		return block;
	}

	private static MethodSymbol GetBaseTypeFinalizeMethod(MethodSymbol method)
	{
		NamedTypeSymbol baseTypeNoUseSiteDiagnostics = method.ContainingType.BaseTypeNoUseSiteDiagnostics;
		while ((object)baseTypeNoUseSiteDiagnostics != null)
		{
			foreach (Symbol member in baseTypeNoUseSiteDiagnostics.GetMembers("Finalize"))
			{
				if (member.Kind == SymbolKind.Method)
				{
					MethodSymbol methodSymbol = (MethodSymbol)member;
					Accessibility declaredAccessibility = methodSymbol.DeclaredAccessibility;
					if ((declaredAccessibility == Accessibility.ProtectedOrInternal || declaredAccessibility == Accessibility.Protected) && methodSymbol.ParameterCount == 0 && methodSymbol.Arity == 0 && methodSymbol.ReturnsVoid)
					{
						return methodSymbol;
					}
				}
			}
			baseTypeNoUseSiteDiagnostics = baseTypeNoUseSiteDiagnostics.BaseTypeNoUseSiteDiagnostics;
		}
		return null;
	}
}
