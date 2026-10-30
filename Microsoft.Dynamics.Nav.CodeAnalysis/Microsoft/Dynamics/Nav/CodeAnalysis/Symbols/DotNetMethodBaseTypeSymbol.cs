using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
internal class DotNetMethodBaseTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter, IDotNetControlAddInMemberSymbol, ISymbol, IEquatable<ISymbol>
{
	private const uint EnumMask = 2147483648u;

	private const uint NoCacheMask = 268435455u;

	private static readonly string ClrRuntimeVoidTypeName = typeof(void).FullName;

	public override int Id { get; }

	public override TypeSymbol ContainingType { get; }

	public DotNetInvokeKind InvocationKind { get; }

	public bool IsStaticDotNetMember { get; }

	public bool IsVisibleInControlAddIn { get; }

	public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
	{
		DotNetMethodBaseTypeSymbol dotNetMethodBaseTypeSymbol = call.Method as DotNetMethodBaseTypeSymbol;
		DebugAssertHelper.Assert(dotNetMethodBaseTypeSymbol != null);
		ExpressionSyntax expression = generator.EmitExpression(call.ReceiverOpt);
		TypeArgumentListSyntax typeArgumentList = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.TypeArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SingletonSeparatedList(GetMethodReturnType(call, dotNetMethodBaseTypeSymbol)));
		SyntaxToken dotNetMethodIdentifier = GetDotNetMethodIdentifier(dotNetMethodBaseTypeSymbol, call.Arguments);
		InvocationExpressionSyntax invocation;
		switch (dotNetMethodBaseTypeSymbol.InvocationKind)
		{
		case DotNetInvokeKind.InvokeMethod:
		case DotNetInvokeKind.InvokePropertyGet:
		case DotNetInvokeKind.InvokePropertySet:
		case DotNetInvokeKind.InvokePropertyGetOrSet:
		case DotNetInvokeKind.InvokeEnumerator:
			invocation = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(dotNetMethodIdentifier, typeArgumentList))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(dotNetMethodBaseTypeSymbol.MetadataName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(GetMethodId(call, dotNetMethodBaseTypeSymbol))));
			break;
		case DotNetInvokeKind.InvokeConstructor:
			invocation = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(dotNetMethodIdentifier)));
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(dotNetMethodBaseTypeSymbol.InvocationKind);
		}
		return generator.EmitArguments(invocation, call);
	}

	internal override IMethodCallEmitter GetCallEmitter()
	{
		return this;
	}

	private static uint GetMethodId(BoundCall call, DotNetMethodBaseTypeSymbol callMethod)
	{
		uint id = (uint)callMethod.Id;
		if (call.ReceiverOpt != null && call.ReceiverOpt.Kind == BoundKind.Parameter)
		{
			return 268435455u;
		}
		switch (callMethod.InvocationKind)
		{
		case DotNetInvokeKind.InvokePropertyGet:
		case DotNetInvokeKind.InvokeConstructor:
		case DotNetInvokeKind.InvokeEnumerator:
			return id;
		case DotNetInvokeKind.InvokePropertySet:
		case DotNetInvokeKind.InvokePropertyGetOrSet:
			if (IsCallToPropertySetter(callMethod, call.Arguments) && GetSetterType(callMethod).DotNetTypeInfo.IsEnum)
			{
				return id | 0x80000000u;
			}
			break;
		case DotNetInvokeKind.InvokeMethod:
			foreach (DotNetTypeSymbol parameterType in callMethod.ParameterTypes)
			{
				if (parameterType.DotNetTypeInfo.IsEnum)
				{
					return id | 0x80000000u;
				}
			}
			break;
		default:
			throw ExceptionUtilities.UnexpectedValue(callMethod.InvocationKind);
		}
		return id;
	}

	private static SyntaxToken GetDotNetMethodIdentifier(DotNetMethodBaseTypeSymbol callMethod, ImmutableArray<BoundExpression> arguments)
	{
		switch (callMethod.InvocationKind)
		{
		case DotNetInvokeKind.InvokeMethod:
			if (!callMethod.IsStaticDotNetMember)
			{
				return CodeGenFacts.NavDotNetInvokeMethodNameIdentifier;
			}
			return CodeGenFacts.NavDotNetInvokeStaticMethodNameIdentifier;
		case DotNetInvokeKind.InvokePropertyGet:
			if (!callMethod.IsStaticDotNetMember)
			{
				return CodeGenFacts.NavDotNetInvokePropertyGetMethodNameIdentifier;
			}
			return CodeGenFacts.NavDotNetInvokeStaticPropertyGetMethodNameIdentifier;
		case DotNetInvokeKind.InvokeConstructor:
			return CodeGenFacts.NavDotNetCreateDotNetMethodNameIdentifier;
		case DotNetInvokeKind.InvokeEnumerator:
			return CodeGenFacts.NavDotNetInvokeStaticFieldMethodNameIdentifier;
		case DotNetInvokeKind.InvokePropertySet:
		case DotNetInvokeKind.InvokePropertyGetOrSet:
			if (IsCallToPropertySetter(callMethod, arguments))
			{
				if (!callMethod.IsStaticDotNetMember)
				{
					return CodeGenFacts.NavDotNetInvokePropertySetMethodNameIdentifier;
				}
				return CodeGenFacts.NavDotNetInvokeStaticPropertySetMethodNameIdentifier;
			}
			if (!callMethod.IsStaticDotNetMember)
			{
				return CodeGenFacts.NavDotNetInvokePropertyGetMethodNameIdentifier;
			}
			return CodeGenFacts.NavDotNetInvokeStaticPropertyGetMethodNameIdentifier;
		default:
			throw ExceptionUtilities.UnexpectedValue(callMethod.InvocationKind);
		}
	}

	private static bool IsCallToPropertySetter(DotNetMethodBaseTypeSymbol callMethod, ImmutableArray<BoundExpression> arguments)
	{
		if (callMethod.InvocationKind != DotNetInvokeKind.InvokePropertySet)
		{
			if (callMethod.InvocationKind == DotNetInvokeKind.InvokePropertyGetOrSet)
			{
				return callMethod.ParameterCount == arguments.Length;
			}
			return false;
		}
		return true;
	}

	private static TypeSyntax GetMethodReturnType(BoundCall call, DotNetMethodBaseTypeSymbol method)
	{
		if (IsCallToPropertySetter(method, call.Arguments))
		{
			TypeSyntax clrValueType = CodeGenerator.GetClrValueType(GetSetterType(method).DotNetTypeInfo.SpecialType);
			if (clrValueType != null)
			{
				return clrValueType;
			}
		}
		else if (method.ReturnType.Name == ClrRuntimeVoidTypeName)
		{
			return CodeGenFacts.NavVoidTypeIdentifier;
		}
		return CodeGenFacts.NavDotNetTypeIdentifier;
	}

	private static DotNetTypeSymbol GetSetterType(DotNetMethodBaseTypeSymbol method)
	{
		return (DotNetTypeSymbol)method.ParameterTypes[method.ParameterCount - 1];
	}

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitDotNetMethodBaseType(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitDotNetMethodBaseType(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitDotNetMethodBaseType(this, a);
	}

	protected DotNetMethodBaseTypeSymbol(DotNetTypeSymbol containingSymbol, string name, bool isVisibleInControlAddIn, bool isProperty = false, Func<TypeSymbol> returnValue = null, bool returnValueIsOptional = false, Func<Symbol, ParameterSymbol[]> getParameters = null, int id = 0, DotNetInvokeKind invocationKind = DotNetInvokeKind.InvokeMethod, bool isStaticDotNetMember = false)
		: base(containingSymbol, name, SymbolCompilationScope.DefinedByOwner, isStatic: false, null, name, isProperty, addDataError: false, addThis: false, isLocal: false, isObsolete: false, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument: false, getParameters)
	{
		ContainingType = containingSymbol;
		Id = id;
		InvocationKind = invocationKind;
		IsStaticDotNetMember = isStaticDotNetMember;
		IsVisibleInControlAddIn = isVisibleInControlAddIn;
	}

	protected DotNetMethodBaseTypeSymbol(DotNetTypeSymbol containingSymbol, string name, bool isVisibleInControlAddIn, bool isProperty = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, Func<Symbol, ParameterSymbol[]> getParameters = null, int id = 0, DotNetInvokeKind invocationKind = DotNetInvokeKind.InvokeMethod, bool isStaticDotNetMember = false)
		: base(containingSymbol, name, SymbolCompilationScope.DefinedByOwner, isStatic: false, null, name, isProperty, addDataError: false, addThis: false, isLocal: false, isObsolete: false, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument: false, getParameters)
	{
		ContainingType = containingSymbol;
		Id = id;
		InvocationKind = invocationKind;
		IsStaticDotNetMember = isStaticDotNetMember;
		IsVisibleInControlAddIn = isVisibleInControlAddIn;
	}
}
