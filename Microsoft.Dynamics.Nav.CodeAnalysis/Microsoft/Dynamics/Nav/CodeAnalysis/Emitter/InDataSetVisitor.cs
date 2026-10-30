using System.Collections.Immutable;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emitter;

internal class InDataSetVisitor : SymbolVisitor
{
	private PooledHashSet<Symbol> VariablesToInclude { get; }

	internal static ImmutableHashSet<Symbol> GetAllImplicitInDatasetVariables(ObjectTypeSymbol target)
	{
		PooledHashSet<Symbol> instance = PooledHashSet<Symbol>.GetInstance();
		try
		{
			InDataSetVisitor implicitInDataSetVisitor = new InDataSetVisitor(instance);
			target.GetMembers().Do((Symbol m) =>
			{
				m.Accept(implicitInDataSetVisitor);
			});
			return instance.ToImmutableHashSet();
		}
		finally
		{
			instance.Free();
		}
	}

	private InDataSetVisitor(PooledHashSet<Symbol> variables)
	{
		VariablesToInclude = variables;
	}

	public override void VisitObjectType(ObjectTypeSymbol symbol)
	{
		foreach (Symbol member in symbol.GetMembers())
		{
			Visit(member);
		}
	}

	public override void VisitControl(ControlSymbol symbol)
	{
		ValidatePropertiesForFieldDependency(symbol.Properties);
		foreach (Symbol member in symbol.GetMembers())
		{
			Visit(member);
		}
	}

	public override void VisitField(FieldSymbol symbol)
	{
		ValidatePropertiesForFieldDependency(symbol.Properties);
		foreach (Symbol member in symbol.GetMembers())
		{
			Visit(member);
		}
	}

	public override void VisitAction(ActionSymbol symbol)
	{
		ValidatePropertiesForFieldDependency(symbol.Properties);
		foreach (Symbol member in symbol.GetMembers())
		{
			Visit(member);
		}
	}

	public override void VisitChange(ChangeSymbol symbol)
	{
		ChangeModifySymbol changeModifySymbol = symbol as ChangeModifySymbol;
		if (changeModifySymbol != null)
		{
			ValidatePropertiesForFieldDependency(changeModifySymbol.Properties);
		}
		if (symbol as ChangeAddSymbol != null)
		{
			foreach (Symbol member in symbol.GetMembers())
			{
				Visit(member);
			}
		}
	}

	public override void VisitView(ViewSymbol node)
	{
		ValidatePropertiesForFieldDependency(node.Properties);
	}

	private void ValidatePropertiesForFieldDependency(ImmutableArray<PropertySymbol> fieldProperties)
	{
		CheckClientExpressions(fieldProperties);
	}

	private void CheckClientExpressions(ImmutableArray<PropertySymbol> properties)
	{
		foreach (PropertySymbol item in properties)
		{
			BoundProperty property = item.Property;
			if (property != null && (property.PropertyValue.Syntax.Kind == SyntaxKind.ClientSideBooleanExpressionPropertyValue || property.PropertyValue.Syntax.Kind == SyntaxKind.StyleExpressionPropertyValue) && !item.Property.HasAnyErrors)
			{
				MarkVariablesWithInDataSet(item);
			}
		}
	}

	private void MarkVariablesWithInDataSet(PropertySymbol propertySymbol)
	{
		BoundProperty property = propertySymbol.Property;
		if (propertySymbol.Value != null)
		{
			InDataSetExpressionVisitor inDataSetExpressionVisitor = new InDataSetExpressionVisitor(VariablesToInclude);
			switch (property.PropertyValue.Kind)
			{
			case BoundKind.Global:
				inDataSetExpressionVisitor.Visit(property.PropertyValue);
				break;
			case BoundKind.CodeExpressionPropertyValue:
			case BoundKind.StyleExpressionPropertyValue:
			{
				BoundCodeExpressionPropertyValue boundCodeExpressionPropertyValue = (BoundCodeExpressionPropertyValue)property.PropertyValue;
				inDataSetExpressionVisitor.Visit(boundCodeExpressionPropertyValue.Expression);
				break;
			}
			}
		}
	}
}
