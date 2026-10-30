using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class TableBaseCodeBuilder<T> : ObjectCodeBuilder<T> where T : ObjectTypeSymbol
{
	protected virtual bool DelayInitializeComponent { get; }

	protected abstract MemberDeclarationSyntax InitializeComponentMethod { get; }

	protected TableBaseCodeBuilder(T symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
	}

	public override ClassDeclarationSyntax AddInitializeComponentMethod(ClassDeclarationSyntax classDeclaration)
	{
		if (InitializeComponentMethod != null)
		{
			classDeclaration = classDeclaration.AddMembers(InitializeComponentMethod);
		}
		return classDeclaration;
	}

	public override NamespaceDeclarationSyntax CreateNamespaceDeclaration()
	{
		return ApplicationObjectEmitHelper.CreateApplicationObjectNamespaceDeclaration();
	}

	protected bool HasConstructorBodyInitializeComponentCall()
	{
		if (InitializeComponentMethod != null)
		{
			return !DelayInitializeComponent;
		}
		return false;
	}

	protected override EmittedMethod BeforeEmitMethod(EmittedMethod emittedMethod)
	{
		if (emittedMethod.Method.IsTrigger() && emittedMethod.Method.ContainingType.Kind == SymbolKind.Field)
		{
			return emittedMethod.WithDeclaration(emittedMethod.MethodDeclaration.AddAttributeLists(CodeGenHelper.FieldTriggerHandlerAttribute(((SourceTriggerSymbol)emittedMethod.Method).TriggerInfo.Name, ((FieldSymbol)emittedMethod.Method.ContainingType).Id)));
		}
		return base.BeforeEmitMethod(emittedMethod);
	}

	protected void EmitEvaluateCaptionClassSwitchSections(IEnumerable<SourceFieldSymbol> sourceFields, ArrayBuilder<SwitchSectionSyntax> builder)
	{
		foreach (SourceFieldSymbol sourceField in sourceFields)
		{
			PropertySymbol property = sourceField.GetProperty(PropertyKind.CaptionClass);
			if (property != null)
			{
				BoundCodeExpressionPropertyValue boundCodeExpressionPropertyValue = property.Property.PropertyValue as BoundCodeExpressionPropertyValue;
				builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(sourceField.Id))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(ModuleBuilder.CodeGenerator.EmitExpression(boundCodeExpressionPropertyValue.Expression))));
			}
		}
	}
}
