using Microsoft.CodeAnalysis.CSharp.Symbols;

namespace Microsoft.CodeAnalysis.CSharp;

internal abstract class CSharpSymbolVisitor
{
	public virtual void Visit(Symbol symbol)
	{
		symbol?.Accept(this);
	}

	public virtual void DefaultVisit(Symbol symbol)
	{
	}

	public virtual void VisitAlias(AliasSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitArrayType(ArrayTypeSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitAssembly(AssemblySymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitDynamicType(DynamicTypeSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitDiscard(DiscardSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitEvent(EventSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitField(FieldSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitLabel(LabelSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitLocal(LocalSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitMethod(MethodSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitModule(ModuleSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitNamedType(NamedTypeSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitNamespace(NamespaceSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitParameter(ParameterSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitPointerType(PointerTypeSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitProperty(PropertySymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitRangeVariable(RangeVariableSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitTypeParameter(TypeParameterSymbol symbol)
	{
		DefaultVisit(symbol);
	}
}
internal abstract class CSharpSymbolVisitor<TResult>
{
	public virtual TResult Visit(Symbol symbol)
	{
		if ((object)symbol != null)
		{
			return symbol.Accept(this);
		}
		return default;
	}

	public virtual TResult DefaultVisit(Symbol symbol)
	{
		return default;
	}

	public virtual TResult VisitAlias(AliasSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitArrayType(ArrayTypeSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitAssembly(AssemblySymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitDynamicType(DynamicTypeSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitDiscard(DiscardSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitEvent(EventSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitField(FieldSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitLabel(LabelSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitLocal(LocalSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitMethod(MethodSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitModule(ModuleSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitNamedType(NamedTypeSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitNamespace(NamespaceSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitParameter(ParameterSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitPointerType(PointerTypeSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitProperty(PropertySymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitRangeVariable(RangeVariableSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitTypeParameter(TypeParameterSymbol symbol)
	{
		return DefaultVisit(symbol);
	}
}
internal abstract class CSharpSymbolVisitor<TArgument, TResult>
{
	public virtual TResult Visit(Symbol symbol, TArgument argument = default(TArgument))
	{
		if ((object)symbol == null)
		{
			return default;
		}
		return symbol.Accept(this, argument);
	}

	public virtual TResult DefaultVisit(Symbol symbol, TArgument argument)
	{
		return default;
	}

	public virtual TResult VisitAssembly(AssemblySymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitModule(ModuleSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitNamespace(NamespaceSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitNamedType(NamedTypeSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitArrayType(ArrayTypeSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitPointerType(PointerTypeSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitErrorType(ErrorTypeSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitTypeParameter(TypeParameterSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitDynamicType(DynamicTypeSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitDiscard(DiscardSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitMethod(MethodSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitField(FieldSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitProperty(PropertySymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitEvent(EventSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitParameter(ParameterSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitLocal(LocalSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitLabel(LabelSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitAlias(AliasSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}

	public virtual TResult VisitRangeVariable(RangeVariableSymbol symbol, TArgument argument)
	{
		return DefaultVisit(symbol, argument);
	}
}
