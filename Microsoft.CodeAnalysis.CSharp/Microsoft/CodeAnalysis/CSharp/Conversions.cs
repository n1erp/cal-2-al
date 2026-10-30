using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal sealed class Conversions : ConversionsBase
{
	private readonly Binder _binder;

	private CSharpCompilation Compilation => _binder.Compilation;

	public Conversions(Binder binder)
		: this(binder, 0)
	{
	}

	private Conversions(Binder binder, int currentRecursionDepth)
		: base(binder.Compilation.Assembly.CorLibrary, currentRecursionDepth)
	{
		_binder = binder;
	}

	protected override ConversionsBase CreateInstance(int currentRecursionDepth)
	{
		return new Conversions(_binder, currentRecursionDepth);
	}

	public override Conversion GetMethodGroupConversion(BoundMethodGroup source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!destination.IsDelegateType())
		{
			return Conversion.NoConversion;
		}
		MethodSymbol delegateInvokeMethodIfAvailable = GetDelegateInvokeMethodIfAvailable(destination);
		if ((object)delegateInvokeMethodIfAvailable == null)
		{
			return Conversion.NoConversion;
		}
		MethodGroupResolution methodGroupResolution = ResolveDelegateMethodGroup(_binder, source, delegateInvokeMethodIfAvailable, ref useSiteDiagnostics);
		Conversion result = ((methodGroupResolution.IsEmpty || methodGroupResolution.HasAnyErrors) ? Conversion.NoConversion : ToConversion(methodGroupResolution.OverloadResolutionResult, methodGroupResolution.MethodGroup, (NamedTypeSymbol)destination));
		methodGroupResolution.Free();
		return result;
	}

	protected override Conversion GetInterpolatedStringConversion(BoundInterpolatedString source, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (!(destination == Compilation.GetWellKnownType(WellKnownType.System_IFormattable)) && !(destination == Compilation.GetWellKnownType(WellKnownType.System_FormattableString)))
		{
			return Conversion.NoConversion;
		}
		return Conversion.InterpolatedString;
	}

	private static MethodGroupResolution ResolveDelegateMethodGroup(Binder binder, BoundMethodGroup source, MethodSymbol delegateInvokeMethodOpt, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if ((object)delegateInvokeMethodOpt != null)
		{
			AnalyzedArguments instance = AnalyzedArguments.GetInstance();
			GetDelegateArguments(source.Syntax, instance, delegateInvokeMethodOpt.Parameters, binder.Compilation);
			MethodGroupResolution result = binder.ResolveMethodGroup(source, instance, isMethodGroupConversion: true, ref useSiteDiagnostics, inferWithDynamic: true, delegateInvokeMethodOpt.RefKind, delegateInvokeMethodOpt.ReturnType);
			instance.Free();
			return result;
		}
		return binder.ResolveMethodGroup(source, null, isMethodGroupConversion: true, ref useSiteDiagnostics);
	}

	private static MethodSymbol GetDelegateInvokeMethodIfAvailable(TypeSymbol type)
	{
		NamedTypeSymbol delegateType = type.GetDelegateType();
		if ((object)delegateType == null)
		{
			return null;
		}
		MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
		if ((object)delegateInvokeMethod == null || delegateInvokeMethod.HasUseSiteError)
		{
			return null;
		}
		return delegateInvokeMethod;
	}

	public static bool ReportDelegateMethodGroupDiagnostics(Binder binder, BoundMethodGroup expr, TypeSymbol targetType, DiagnosticBag diagnostics)
	{
		MethodSymbol delegateInvokeMethodIfAvailable = GetDelegateInvokeMethodIfAvailable(targetType);
		HashSet<DiagnosticInfo> useSiteDiagnostics = null;
		MethodGroupResolution methodGroupResolution = ResolveDelegateMethodGroup(binder, expr, delegateInvokeMethodIfAvailable, ref useSiteDiagnostics);
		diagnostics.Add(expr.Syntax, useSiteDiagnostics);
		bool flag = methodGroupResolution.HasAnyErrors;
		diagnostics.AddRange(methodGroupResolution.Diagnostics);
		if (methodGroupResolution.MethodGroup != null)
		{
			OverloadResolutionResult<MethodSymbol> overloadResolutionResult = methodGroupResolution.OverloadResolutionResult;
			if (overloadResolutionResult != null)
			{
				if (overloadResolutionResult.Succeeded)
				{
					MethodSymbol member = overloadResolutionResult.BestResult.Member;
					if (methodGroupResolution.MethodGroup.IsExtensionMethodGroup)
					{
						ParameterSymbol parameterSymbol = member.Parameters[0];
						if (!parameterSymbol.Type.IsReferenceType)
						{
							diagnostics.Add(ErrorCode.ERR_ValueTypeExtDelegate, expr.Syntax.Location, member, parameterSymbol.Type);
							flag = true;
						}
					}
					else if (member.ContainingType.IsNullableType() && !member.IsOverride)
					{
						diagnostics.Add(ErrorCode.ERR_DelegateOnNullable, expr.Syntax.Location, member);
						flag = true;
					}
				}
				else if (!flag && !methodGroupResolution.IsEmpty && methodGroupResolution.ResultKind == LookupResultKind.Viable)
				{
					DiagnosticBag instance = DiagnosticBag.GetInstance();
					overloadResolutionResult.ReportDiagnostics(binder, expr.Syntax.Location, expr.Syntax, instance, expr.Name, methodGroupResolution.MethodGroup.Receiver, expr.Syntax, methodGroupResolution.AnalyzedArguments, methodGroupResolution.MethodGroup.Methods.ToImmutable(), null, null, null, isMethodGroupConversion: true, delegateInvokeMethodIfAvailable?.RefKind, targetType);
					if (!instance.IsEmptyWithoutResolution)
					{
						flag = instance.HasAnyErrors();
						diagnostics.AddRange(instance);
					}
					instance.Free();
				}
			}
		}
		methodGroupResolution.Free();
		return flag;
	}

	public Conversion MethodGroupConversion(SyntaxNode syntax, MethodGroup methodGroup, NamedTypeSymbol delegateType, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		AnalyzedArguments instance = AnalyzedArguments.GetInstance();
		OverloadResolutionResult<MethodSymbol> instance2 = OverloadResolutionResult<MethodSymbol>.GetInstance();
		MethodSymbol delegateInvokeMethod = delegateType.DelegateInvokeMethod;
		GetDelegateArguments(syntax, instance, delegateInvokeMethod.Parameters, Compilation);
		_binder.OverloadResolution.MethodInvocationOverloadResolution(methodGroup.Methods, methodGroup.TypeArguments, methodGroup.Receiver, instance, instance2, ref useSiteDiagnostics, isMethodGroupConversion: true, allowRefOmittedArguments: false, inferWithDynamic: false, allowUnexpandedForm: true, delegateInvokeMethod.RefKind, delegateInvokeMethod.ReturnType);
		Conversion result = ToConversion(instance2, methodGroup, delegateType);
		instance.Free();
		instance2.Free();
		return result;
	}

	public static void GetDelegateArguments(SyntaxNode syntax, AnalyzedArguments analyzedArguments, ImmutableArray<ParameterSymbol> delegateParameters, CSharpCompilation compilation)
	{
		foreach (ParameterSymbol item in delegateParameters)
		{
			ParameterSymbol parameterSymbol = item;
			if (parameterSymbol.Type.IsDynamic())
			{
				parameterSymbol = new SignatureOnlyParameterSymbol(compilation.GetSpecialType(SpecialType.System_Object), parameterSymbol.CustomModifiers, parameterSymbol.RefCustomModifiers, parameterSymbol.IsParams, parameterSymbol.RefKind);
			}
			analyzedArguments.Arguments.Add(new BoundParameter(syntax, parameterSymbol)
			{
				WasCompilerGenerated = true
			});
			analyzedArguments.RefKinds.Add(parameterSymbol.RefKind);
		}
	}

	private static Conversion ToConversion(OverloadResolutionResult<MethodSymbol> result, MethodGroup methodGroup, NamedTypeSymbol delegateType)
	{
		if (!result.Succeeded)
		{
			return Conversion.NoConversion;
		}
		MethodSymbol member = result.BestResult.Member;
		if (methodGroup.IsExtensionMethodGroup && !member.Parameters[0].Type.IsReferenceType)
		{
			return Conversion.NoConversion;
		}
		if (!member.IsStatic)
		{
			BoundExpression receiver = methodGroup.Receiver;
			if (receiver != null && receiver.Type?.IsRestrictedType() == true)
			{
				return Conversion.NoConversion;
			}
		}
		if (member.ContainingType.IsNullableType() && !member.IsOverride)
		{
			return Conversion.NoConversion;
		}
		return new Conversion(ConversionKind.MethodGroup, member, methodGroup.IsExtensionMethodGroup);
	}

	public override Conversion GetStackAllocConversion(BoundStackAllocArrayCreation sourceExpression, TypeSymbol destination, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		if (sourceExpression.Syntax.IsVariableDeclarationInitialization())
		{
			PointerTypeSymbol source = new PointerTypeSymbol(sourceExpression.ElementType);
			Conversion underlyingConversion = ClassifyImplicitConversionFromType(source, destination, ref useSiteDiagnostics);
			if (underlyingConversion.IsValid)
			{
				return Conversion.MakeStackAllocToPointerType(underlyingConversion);
			}
			NamedTypeSymbol wellKnownType = _binder.GetWellKnownType(WellKnownType.System_Span_T, ref useSiteDiagnostics);
			if (wellKnownType.TypeKind == TypeKind.Struct && wellKnownType.IsByRefLikeType)
			{
				NamedTypeSymbol source2 = wellKnownType.Construct(sourceExpression.ElementType);
				Conversion underlyingConversion2 = ClassifyImplicitConversionFromType(source2, destination, ref useSiteDiagnostics);
				if (underlyingConversion2.Exists)
				{
					return Conversion.MakeStackAllocToSpanType(underlyingConversion2);
				}
			}
		}
		return Conversion.NoConversion;
	}
}
