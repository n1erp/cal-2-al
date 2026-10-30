using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[DebuggerNonUserCode]
[CompilerGenerated]
[DebuggerDisplay("{GetDebuggerDisplay(), nq}")]
internal abstract class MethodSymbol : Symbol, IMethodSymbol, ISymbol, IEquatable<ISymbol>, ISymbolWithId, ISymbolWithAttributes, ISymbolWithType
{
	private SymbolCompilationScope? symbolCompilationScope;

	private ParameterSignature lazyParameterSignature;

	public override SymbolKind Kind => SymbolKind.Method;

	public virtual int Id => -1;

	public TypeSymbol Type => ReturnType;

	public override SymbolCompilationScope CompilationScope
	{
		get
		{
			if (symbolCompilationScope.HasValue)
			{
				return symbolCompilationScope.Value;
			}
			symbolCompilationScope = SymbolCompilationScope.Personalization;
			if (Attributes == null || Attributes.Length == 0)
			{
				return symbolCompilationScope.Value;
			}
			AttributeSymbol attributeSymbol = Attributes.FirstOrDefault((AttributeSymbol a) => a.AttributeInfo != null && a.AttributeInfo.Kind == AttributeKind.Scope && a.Arguments != null && a.Arguments.Length == 1);
			if (attributeSymbol != null && Enum.TryParse<SymbolCompilationScope>(attributeSymbol.Arguments[0].ValueText.UnquoteLiteral(), out var result))
			{
				symbolCompilationScope = result;
			}
			return symbolCompilationScope.Value;
		}
	}

	public abstract ReturnValueSymbol ReturnValue { get; }

	public TypeSymbol ReturnType
	{
		get
		{
			if (!(ReturnValue != null))
			{
				return NavCorLib.None;
			}
			return ReturnValue.Type;
		}
	}

	internal virtual int ParameterCount => Parameters.Length;

	public abstract ImmutableArray<ParameterSymbol> Parameters { get; }

	public abstract ImmutableArray<AttributeSymbol> Attributes { get; }

	internal virtual int LocalVariableCount => LocalVariables.Length;

	public abstract ImmutableArray<VariableSymbol> LocalVariables { get; }

	internal ImmutableArray<TypeSymbol> ParameterTypes
	{
		get
		{
			ParameterSignature.PopulateParameterSignature(Parameters, ref lazyParameterSignature);
			return lazyParameterSignature.ParameterTypes;
		}
	}

	internal ImmutableArray<bool> ParameterIsVars
	{
		get
		{
			ParameterSignature.PopulateParameterSignature(Parameters, ref lazyParameterSignature);
			return lazyParameterSignature.ParameterRefKinds;
		}
	}

	public abstract bool IsStatic { get; }

	public abstract bool IsLocal { get; }

	public virtual bool IsTryFunction => false;

	public abstract MethodKind MethodKind { get; }

	ImmutableArray<IParameterSymbol> IMethodSymbol.Parameters => StaticCast<IParameterSymbol>.From(Parameters);

	ImmutableArray<IVariableSymbol> IMethodSymbol.LocalVariables => StaticCast<IVariableSymbol>.From(LocalVariables);

	ImmutableArray<IAttributeSymbol> IMethodSymbol.Attributes => StaticCast<IAttributeSymbol>.From(Attributes);

	public IReturnValueSymbol ReturnValueSymbol => ReturnValue;

	public bool IsEvent => Attributes.Any((AttributeSymbol attribute) => attribute.AttributeInfo != null && (attribute.AttributeInfo.Kind == AttributeKind.BusinessEvent || attribute.AttributeInfo.Kind == AttributeKind.IntegrationEvent));

	public bool IsNonDebuggable => Attributes.Any((AttributeSymbol attribute) =>
	{
		AttributeTypeInfo attributeInfo = attribute.AttributeInfo;
		return attributeInfo != null && attributeInfo.Kind == AttributeKind.NonDebuggable;
	});

	public override void Accept(SymbolVisitor visitor)
	{
		visitor.VisitMethod(this);
	}

	public override TResult Accept<TResult>(SymbolVisitor<TResult> visitor)
	{
		return visitor.VisitMethod(this);
	}

	internal override TResult Accept<TArgument, TResult>(SymbolVisitor<TArgument, TResult> visitor, TArgument a)
	{
		return visitor.VisitMethod(this, a);
	}

	protected int CalculateMethodId(bool isOverloadedMethod = false)
	{
		int fNVHashCode = Hash.GetFNVHashCode(Name.ToUpperInvariant());
		fNVHashCode = Hash.Combine(fNVHashCode, ReturnType.NavTypeKind.GetHashCode());
		bool flag = isOverloadedMethod && OverloadRequiresRuntimeDisambiguation();
		for (int i = 0; i < ParameterCount; i++)
		{
			ParameterSymbol parameterSymbol = Parameters[i];
			fNVHashCode = Hash.Combine(fNVHashCode, i, parameterSymbol.IsVar.GetHashCode(), parameterSymbol.Type.NavTypeKind.GetHashCode());
			if (flag)
			{
				fNVHashCode = Hash.Combine(fNVHashCode, GetParameterSubTypeHashCode(parameterSymbol.Type));
			}
		}
		return fNVHashCode;
	}

	private bool OverloadRequiresRuntimeDisambiguation()
	{
		if (!OverloadRequiresComparisonWithOthers())
		{
			return false;
		}
		foreach (MethodSymbol item in (from m in ContainingType.GetMembers(Name)
			where (object)this != m && m.Kind == SymbolKind.Method
			select m).Cast<MethodSymbol>())
		{
			if (HasMatchingSignature(item))
			{
				return true;
			}
		}
		return false;
	}

	internal bool HasMatchingSignature(MethodSymbol methodSymbol)
	{
		if (!SemanticFacts.IsSameName(Name, methodSymbol.Name))
		{
			return false;
		}
		if (ParameterCount != methodSymbol.ParameterCount)
		{
			return false;
		}
		for (int i = 0; i < ParameterCount; i++)
		{
			if (Parameters[i].Type.NavTypeKind != methodSymbol.Parameters[i].Type.NavTypeKind || Parameters[i].IsVar != methodSymbol.Parameters[i].IsVar)
			{
				return false;
			}
		}
		return true;
	}

	private bool OverloadRequiresComparisonWithOthers()
	{
		for (int i = 0; i < ParameterCount; i++)
		{
			if (IsParameterWithSubtype(Parameters[i].Type.NavTypeKind))
			{
				return true;
			}
		}
		return false;
	}

	private static int GetParameterSubTypeHashCode(TypeSymbol parameterType)
	{
		switch (parameterType.NavTypeKind)
		{
		case NavTypeKind.Table:
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.TestPage:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.Page:
		case NavTypeKind.XmlPort:
		case NavTypeKind.Query:
		case NavTypeKind.Enum:
			return ((ISymbolWithId)parameterType).Id.GetHashCode();
		case NavTypeKind.DotNet:
			return parameterType.OriginalDefinition.GetHashCode();
		default:
			return 1;
		}
	}

	internal static bool IsParameterWithSubtype(NavTypeKind kind)
	{
		switch (kind)
		{
		case NavTypeKind.Table:
		case NavTypeKind.TestRequestPage:
		case NavTypeKind.DotNet:
		case NavTypeKind.TestPage:
		case NavTypeKind.Record:
		case NavTypeKind.Codeunit:
		case NavTypeKind.Page:
		case NavTypeKind.XmlPort:
		case NavTypeKind.Query:
		case NavTypeKind.Enum:
			return true;
		default:
			return false;
		}
	}

	internal virtual IMethodCallEmitter GetCallEmitter()
	{
		return null;
	}
}
