using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class CodeunitCodeBuilder : ObjectCodeBuilder<SourceCodeunitTypeSymbol>
{
	public MemberDeclarationSyntax InitializeComponentMethod { get; private set; }

	protected override TypeSyntax BaseClassType { get; }

	protected override TypeSyntax ClassType { get; }

	public CodeunitCodeBuilder(SourceCodeunitTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		BaseClassType = GetBaseClassType();
		ClassType = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(ModuleBuilder.Options);
	}

	public override NamespaceDeclarationSyntax CreateNamespaceDeclaration()
	{
		return ApplicationObjectEmitHelper.CreateApplicationObjectNamespaceDeclaration();
	}

	public override ClassDeclarationSyntax AddMethods(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = AddIsSingleInstancePropertyIfNeeded(classDeclaration);
		classDeclaration = AddOnTestRunMethodsHaveTestPermissionsParameterIfNeeded(classDeclaration);
		classDeclaration = base.AddMethods(classDeclaration);
		return classDeclaration;
	}

	private ClassDeclarationSyntax AddOnTestRunMethodsHaveTestPermissionsParameterIfNeeded(ClassDeclarationSyntax classDeclaration)
	{
		if (new CodeunitSubtypeKind?(Symbol.GetEnumPropertyValueOrDefault<CodeunitSubtypeKind>(PropertyKind.Subtype)) == CodeunitSubtypeKind.TestRunner)
		{
			classDeclaration = classDeclaration.AddMembers(CodeGenerator.EmitBooleanPropertyOverloading(CodeGenFacts.OnTestRunMethodsHaveTestPermissionsParameterIdentifier, isTrue: true));
			TestIsolationKind? testIsolationKind = Symbol.GetEnumPropertyValueOrDefault<TestIsolationKind>(PropertyKind.TestIsolation);
			if (testIsolationKind == TestIsolationKind.Codeunit)
			{
				classDeclaration = classDeclaration.AddMembers(CodeGenerator.EmitBooleanPropertyOverloading(CodeGenFacts.CommitTestCodeunitsIdentifier, isTrue: false));
				classDeclaration = classDeclaration.AddMembers(CodeGenerator.EmitBooleanPropertyOverloading(CodeGenFacts.CommitTestFunctionsIdentifier, isTrue: true));
			}
			if (testIsolationKind == TestIsolationKind.Function)
			{
				classDeclaration = classDeclaration.AddMembers(CodeGenerator.EmitBooleanPropertyOverloading(CodeGenFacts.CommitTestCodeunitsIdentifier, isTrue: false));
				classDeclaration = classDeclaration.AddMembers(CodeGenerator.EmitBooleanPropertyOverloading(CodeGenFacts.CommitTestFunctionsIdentifier, isTrue: false));
			}
		}
		return classDeclaration;
	}

	private ClassDeclarationSyntax AddIsSingleInstancePropertyIfNeeded(ClassDeclarationSyntax classDeclaration)
	{
		if (Symbol.GetBooleanPropertyValue(PropertyKind.SingleInstance) != true)
		{
			return classDeclaration;
		}
		PropertyDeclarationSyntax propertyDeclarationSyntax = CodeGenerator.EmitBooleanPropertyOverloading(CodeGenFacts.IsSingleInstancePropertyIdentifier, isTrue: true);
		return classDeclaration.AddMembers(propertyDeclarationSyntax);
	}

	public override ClassDeclarationSyntax CreateClassDeclaration()
	{
		ClassDeclarationSyntax classDeclarationSyntax = base.CreateClassDeclaration();
		bool? booleanPropertyValue = Symbol.GetBooleanPropertyValue(PropertyKind.SingleInstance);
		bool singleInstance = booleanPropertyValue.HasValue && booleanPropertyValue.Value;
		bool eventManualBinding = Symbol.GetEnumPropertyValueOrDefault<EventSubscriberInstanceKind>(PropertyKind.EventSubscriberInstance) == EventSubscriberInstanceKind.Manual;
		int tableNo = Symbol.GetBoundPropertyValue<BoundApplicationObjectReferencePropertyValue>(PropertyKind.TableNo)?.Value.Id ?? 0;
		CodeunitSubtypeKind enumPropertyValueOrDefault = Symbol.GetEnumPropertyValueOrDefault<CodeunitSubtypeKind>(PropertyKind.Subtype);
		return classDeclarationSyntax.AddAttributeLists(CodeGenHelper.NavCodeunitOptionsAttribute(singleInstance, eventManualBinding, tableNo, enumPropertyValueOrDefault));
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		InitializeComponentMethod = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(Symbol.GlobalVariables, ModuleBuilder.Options);
		ConstructorDeclarationSyntax constructorDeclarationSyntax = ApplicationObjectEmitHelper.CreateConstructorDeclaration(ClassType.ToString(), Symbol.Id, InitializeComponentMethod != null, null, null, AddBaseArguments);
		classDeclaration = classDeclaration.AddMembers(constructorDeclarationSyntax);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddInitializeComponentMethod(ClassDeclarationSyntax classDeclaration)
	{
		if (InitializeComponentMethod != null)
		{
			classDeclaration = classDeclaration.AddMembers(InitializeComponentMethod);
		}
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddEvaluateCaptionClass(ClassDeclarationSyntax classDeclaration)
	{
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddFrameworkMethods(ClassDeclarationSyntax classDeclaration)
	{
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol);
	}

	public override ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(ClassType.ToString(), (ParameterSyntax[])null, (ArgumentSyntax[])null);
		return classDeclaration.AddMembers(methodDeclarationSyntax);
	}

	private TypeSyntax GetBaseClassType()
	{
		return Symbol.Subtype switch
		{
			CodeunitSubtypeKind.Test => CodeGenFacts.NavTestCodeunitType, 
			CodeunitSubtypeKind.TestRunner => CodeGenFacts.NavTestRunnerCodeunitType, 
			CodeunitSubtypeKind.Upgrade => CodeGenFacts.NavUpgradeCodeunitType, 
			_ => CodeGenFacts.NavCodeunitType, 
		};
	}

	private ArgumentListSyntax AddBaseArguments(ArgumentListSyntax argumentList)
	{
		TableTypeSymbol relatedTableSymbol = Symbol.GetRelatedTableSymbol();
		if (!(relatedTableSymbol == null))
		{
			return argumentList.AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(relatedTableSymbol.Id)));
		}
		return argumentList;
	}
}
