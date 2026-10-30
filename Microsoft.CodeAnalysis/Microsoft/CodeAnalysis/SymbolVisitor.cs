namespace Microsoft.CodeAnalysis;

public abstract class SymbolVisitor
{
	public virtual void Visit(ISymbol symbol)
	{
		symbol?.Accept(this);
	}

	public virtual void DefaultVisit(ISymbol symbol)
	{
	}

	public virtual void VisitAlias(IAliasSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitArrayType(IArrayTypeSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitAssembly(IAssemblySymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitDiscard(IDiscardSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitDynamicType(IDynamicTypeSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitEvent(IEventSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitField(IFieldSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitLabel(ILabelSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitLocal(ILocalSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitMethod(IMethodSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitModule(IModuleSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitNamedType(INamedTypeSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitNamespace(INamespaceSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitParameter(IParameterSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitPointerType(IPointerTypeSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitProperty(IPropertySymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitRangeVariable(IRangeVariableSymbol symbol)
	{
		DefaultVisit(symbol);
	}

	public virtual void VisitTypeParameter(ITypeParameterSymbol symbol)
	{
		DefaultVisit(symbol);
	}
}
public abstract class SymbolVisitor<TResult>
{
	public virtual TResult Visit(ISymbol symbol)
	{
		if (symbol != null)
		{
			return symbol.Accept(this);
		}
		return default;
	}

	public virtual TResult DefaultVisit(ISymbol symbol)
	{
		return default;
	}

	public virtual TResult VisitAlias(IAliasSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitArrayType(IArrayTypeSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitAssembly(IAssemblySymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitDiscard(IDiscardSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitDynamicType(IDynamicTypeSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitEvent(IEventSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitField(IFieldSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitLabel(ILabelSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitLocal(ILocalSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitMethod(IMethodSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitModule(IModuleSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitNamedType(INamedTypeSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitNamespace(INamespaceSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitParameter(IParameterSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitPointerType(IPointerTypeSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitProperty(IPropertySymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitRangeVariable(IRangeVariableSymbol symbol)
	{
		return DefaultVisit(symbol);
	}

	public virtual TResult VisitTypeParameter(ITypeParameterSymbol symbol)
	{
		return DefaultVisit(symbol);
	}
}
