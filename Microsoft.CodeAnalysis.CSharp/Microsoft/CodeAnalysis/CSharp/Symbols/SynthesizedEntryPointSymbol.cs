using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reflection;
using Microsoft.Cci;
using Roslyn.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.Symbols;

internal abstract class SynthesizedEntryPointSymbol : MethodSymbol
{
	internal sealed class AsyncForwardEntryPoint : SynthesizedEntryPointSymbol
	{
		private readonly CSharpSyntaxNode _userMainReturnTypeSyntax;

		private readonly BoundExpression _getAwaiterGetResultCall;

		private readonly ImmutableArray<ParameterSymbol> _parameters;

		public override string Name => "<Main>";

		public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

		public override TypeSymbol ReturnType => _getAwaiterGetResultCall.Type;

		internal AsyncForwardEntryPoint(CSharpCompilation compilation, NamedTypeSymbol containingType, MethodSymbol userMain)
			: base(containingType)
		{
			_userMainReturnTypeSyntax = userMain.ExtractReturnTypeSyntax();
			Binder binder = compilation.GetBinder(_userMainReturnTypeSyntax);
			_parameters = SynthesizedParameterSymbol.DeriveParameters(userMain, this);
			ImmutableArray<BoundExpression> arguments = Parameters.SelectAsArray((Func<ParameterSymbol, CSharpSyntaxNode, BoundExpression>)((ParameterSymbol p, CSharpSyntaxNode s) => new BoundParameter(s, p, p.Type)), _userMainReturnTypeSyntax);
			BoundCall expression = new BoundCall(_userMainReturnTypeSyntax, null, userMain, arguments, default, default, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default, LookupResultKind.Viable, binder, userMain.ReturnType)
			{
				WasCompilerGenerated = true
			};
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			binder.GetAwaitableExpressionInfo(expression, out var _, out var _, out var _, out _getAwaiterGetResultCall, _userMainReturnTypeSyntax, instance);
			instance.Free();
		}

		internal override BoundBlock CreateBody(DiagnosticBag diagnostics)
		{
			CSharpSyntaxNode userMainReturnTypeSyntax = _userMainReturnTypeSyntax;
			if (ReturnsVoid)
			{
				return new BoundBlock(userMainReturnTypeSyntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create((BoundStatement)new BoundExpressionStatement(userMainReturnTypeSyntax, _getAwaiterGetResultCall)
				{
					WasCompilerGenerated = true
				}, (BoundStatement)new BoundReturnStatement(userMainReturnTypeSyntax, RefKind.None, null)
				{
					WasCompilerGenerated = true
				}))
				{
					WasCompilerGenerated = true
				};
			}
			return new BoundBlock(userMainReturnTypeSyntax, ImmutableArray<LocalSymbol>.Empty, ImmutableArray.Create((BoundStatement)new BoundReturnStatement(userMainReturnTypeSyntax, RefKind.None, _getAwaiterGetResultCall)))
			{
				WasCompilerGenerated = true
			};
		}
	}

	private sealed class ScriptEntryPoint : SynthesizedEntryPointSymbol
	{
		private readonly TypeSymbol _returnType;

		public override string Name => "<Main>";

		public override ImmutableArray<ParameterSymbol> Parameters => ImmutableArray<ParameterSymbol>.Empty;

		public override TypeSymbol ReturnType => _returnType;

		internal ScriptEntryPoint(NamedTypeSymbol containingType, TypeSymbol returnType)
			: base(containingType)
		{
			_returnType = returnType;
		}

		internal override BoundBlock CreateBody(DiagnosticBag diagnostics)
		{
			CSharpSyntaxNode cSharpSyntaxNode = DummySyntax();
			CSharpCompilation declaringCompilation = _containingType.DeclaringCompilation;
			InContainerBinder next = new InContainerBinder(null, new BuckStopsHereBinder(declaringCompilation), declaringCompilation.GlobalImports);
			next = new InContainerBinder(declaringCompilation.GlobalNamespace, next);
			SynthesizedInstanceConstructor scriptConstructor = _containingType.GetScriptConstructor();
			SynthesizedInteractiveInitializerMethod scriptInitializer = _containingType.GetScriptInitializer();
			BoundLocal boundLocal = new BoundLocal(cSharpSyntaxNode, new SynthesizedLocal(this, _containingType, SynthesizedLocalKind.LoweringTemp), null, _containingType)
			{
				WasCompilerGenerated = true
			};
			BoundCall expression = CreateParameterlessCall(cSharpSyntaxNode, boundLocal, scriptInitializer);
			if (!next.GetAwaitableExpressionInfo(expression, out var _, out var _, out var _, out var getAwaiterGetResultCall, cSharpSyntaxNode, diagnostics))
			{
				return new BoundBlock(cSharpSyntaxNode, ImmutableArray<LocalSymbol>.Empty, ImmutableArray<BoundStatement>.Empty, hasErrors: true);
			}
			return new BoundBlock(cSharpSyntaxNode, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundStatement)new BoundExpressionStatement(cSharpSyntaxNode, new BoundAssignmentOperator(cSharpSyntaxNode, boundLocal, new BoundObjectCreationExpression(cSharpSyntaxNode, scriptConstructor, null)
			{
				WasCompilerGenerated = true
			}, _containingType)
			{
				WasCompilerGenerated = true
			})
			{
				WasCompilerGenerated = true
			}, (BoundStatement)new BoundExpressionStatement(cSharpSyntaxNode, getAwaiterGetResultCall)
			{
				WasCompilerGenerated = true
			}, (BoundStatement)new BoundReturnStatement(cSharpSyntaxNode, RefKind.None, null)
			{
				WasCompilerGenerated = true
			}))
			{
				WasCompilerGenerated = true
			};
		}
	}

	private sealed class SubmissionEntryPoint : SynthesizedEntryPointSymbol
	{
		private readonly ImmutableArray<ParameterSymbol> _parameters;

		private readonly TypeSymbol _returnType;

		public override string Name => "<Factory>";

		public override ImmutableArray<ParameterSymbol> Parameters => _parameters;

		public override TypeSymbol ReturnType => _returnType;

		internal SubmissionEntryPoint(NamedTypeSymbol containingType, TypeSymbol returnType, TypeSymbol submissionArrayType)
			: base(containingType)
		{
			_parameters = ImmutableArray.Create(SynthesizedParameterSymbol.Create(this, submissionArrayType, 0, RefKind.None, "submissionArray"));
			_returnType = returnType;
		}

		internal override BoundBlock CreateBody(DiagnosticBag diagnostics)
		{
			CSharpSyntaxNode syntax = DummySyntax();
			SynthesizedInstanceConstructor scriptConstructor = _containingType.GetScriptConstructor();
			SynthesizedInteractiveInitializerMethod scriptInitializer = _containingType.GetScriptInitializer();
			BoundParameter item = new BoundParameter(syntax, _parameters[0])
			{
				WasCompilerGenerated = true
			};
			BoundLocal boundLocal = new BoundLocal(syntax, new SynthesizedLocal(this, _containingType, SynthesizedLocalKind.LoweringTemp), null, _containingType)
			{
				WasCompilerGenerated = true
			};
			BoundExpressionStatement item2 = new BoundExpressionStatement(syntax, new BoundAssignmentOperator(syntax, boundLocal, new BoundObjectCreationExpression(syntax, scriptConstructor, ImmutableArray.Create((BoundExpression)item), default, default, expanded: false, default, null, null, null, _containingType)
			{
				WasCompilerGenerated = true
			}, _containingType)
			{
				WasCompilerGenerated = true
			})
			{
				WasCompilerGenerated = true
			};
			BoundCall expressionOpt = CreateParameterlessCall(syntax, boundLocal, scriptInitializer);
			BoundReturnStatement item3 = new BoundReturnStatement(syntax, RefKind.None, expressionOpt)
			{
				WasCompilerGenerated = true
			};
			return new BoundBlock(syntax, ImmutableArray.Create(boundLocal.LocalSymbol), ImmutableArray.Create((BoundStatement)item2, (BoundStatement)item3))
			{
				WasCompilerGenerated = true
			};
		}
	}

	internal const string MainName = "<Main>";

	internal const string FactoryName = "<Factory>";

	private readonly NamedTypeSymbol _containingType;

	internal override bool GenerateDebugInfo => false;

	public override Symbol ContainingSymbol => _containingType;

	public abstract override string Name { get; }

	internal override bool HasSpecialName => true;

	internal override MethodImplAttributes ImplementationAttributes => MethodImplAttributes.IL;

	internal override bool RequiresSecurityObject => false;

	public override bool IsVararg => false;

	public override ImmutableArray<TypeParameterSymbol> TypeParameters => ImmutableArray<TypeParameterSymbol>.Empty;

	public override Accessibility DeclaredAccessibility => Accessibility.Private;

	public override ImmutableArray<Location> Locations => ImmutableArray<Location>.Empty;

	public override ImmutableArray<SyntaxReference> DeclaringSyntaxReferences => ImmutableArray<SyntaxReference>.Empty;

	public override RefKind RefKind => RefKind.None;

	public override ImmutableArray<CustomModifier> ReturnTypeCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<CustomModifier> RefCustomModifiers => ImmutableArray<CustomModifier>.Empty;

	public override ImmutableArray<TypeSymbol> TypeArguments => ImmutableArray<TypeSymbol>.Empty;

	public override Symbol AssociatedSymbol => null;

	public override int Arity => 0;

	public override bool ReturnsVoid => ReturnType.SpecialType == SpecialType.System_Void;

	public override MethodKind MethodKind => MethodKind.Ordinary;

	public override bool IsExtern => false;

	public override bool IsSealed => false;

	public override bool IsAbstract => false;

	public override bool IsOverride => false;

	public override bool IsVirtual => false;

	public override bool IsStatic => true;

	public override bool IsAsync => false;

	public override bool HidesBaseMethodsByName => false;

	public override bool IsExtensionMethod => false;

	internal sealed override ObsoleteAttributeData ObsoleteAttributeData => null;

	internal override CallingConvention CallingConvention => CallingConvention.Default;

	internal override bool IsExplicitInterfaceImplementation => false;

	public override ImmutableArray<MethodSymbol> ExplicitInterfaceImplementations => ImmutableArray<MethodSymbol>.Empty;

	internal override bool IsMetadataFinal => false;

	public override bool IsImplicitlyDeclared => true;

	internal override MarshalPseudoCustomAttributeData ReturnValueMarshallingInformation => null;

	internal override bool HasDeclarativeSecurity => false;

	internal static SynthesizedEntryPointSymbol Create(SynthesizedInteractiveInitializerMethod initializerMethod, DiagnosticBag diagnostics)
	{
		NamedTypeSymbol containingType = initializerMethod.ContainingType;
		CSharpCompilation declaringCompilation = containingType.DeclaringCompilation;
		if (declaringCompilation.IsSubmission)
		{
			NamedTypeSymbol specialType = Binder.GetSpecialType(declaringCompilation, SpecialType.System_Object, DummySyntax(), diagnostics);
			ArrayTypeSymbol arrayTypeSymbol = declaringCompilation.CreateArrayTypeSymbol(specialType);
			ReportUseSiteDiagnostics(arrayTypeSymbol, diagnostics);
			return new SubmissionEntryPoint(containingType, initializerMethod.ReturnType, arrayTypeSymbol);
		}
		NamedTypeSymbol specialType2 = Binder.GetSpecialType(declaringCompilation, SpecialType.System_Void, DummySyntax(), diagnostics);
		return new ScriptEntryPoint(containingType, specialType2);
	}

	private SynthesizedEntryPointSymbol(NamedTypeSymbol containingType)
	{
		_containingType = containingType;
	}

	internal abstract BoundBlock CreateBody(DiagnosticBag diagnostics);

	internal sealed override bool IsMetadataNewSlot(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	internal sealed override bool IsMetadataVirtual(bool ignoreInterfaceImplementationChanges = false)
	{
		return false;
	}

	public override DllImportData GetDllImportData()
	{
		return null;
	}

	internal override IEnumerable<SecurityAttribute> GetSecurityInformation()
	{
		throw ExceptionUtilities.Unreachable;
	}

	internal sealed override ImmutableArray<string> GetAppliedConditionalSymbols()
	{
		return ImmutableArray<string>.Empty;
	}

	internal override int CalculateLocalSyntaxOffset(int localPosition, SyntaxTree localTree)
	{
		throw ExceptionUtilities.Unreachable;
	}

	private static CSharpSyntaxNode DummySyntax()
	{
		return (CSharpSyntaxNode)CSharpSyntaxTree.Dummy.GetRoot();
	}

	private static void ReportUseSiteDiagnostics(Symbol symbol, DiagnosticBag diagnostics)
	{
		DiagnosticInfo useSiteDiagnostic = symbol.GetUseSiteDiagnostic();
		if (useSiteDiagnostic != null)
		{
			Symbol.ReportUseSiteDiagnostic(useSiteDiagnostic, diagnostics, NoLocation.Singleton);
		}
	}

	private static BoundCall CreateParameterlessCall(CSharpSyntaxNode syntax, BoundExpression receiver, MethodSymbol method)
	{
		return new BoundCall(syntax, receiver, method, ImmutableArray<BoundExpression>.Empty, default, default, isDelegateCall: false, expanded: false, invokedAsExtensionMethod: false, default, LookupResultKind.Viable, null, method.ReturnType)
		{
			WasCompilerGenerated = true
		};
	}
}
