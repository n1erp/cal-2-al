using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class QueryCodeBuilder : ObjectCodeBuilder<QueryTypeSymbol>
{
	private static readonly ParameterSyntax[] ConstructorParameters = new ParameterSyntax[2]
	{
		Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("securityFiltering")).WithType(CodeGenFacts.RuntimeNavSecurityFilterTypeSyntax),
		Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("metadata")).WithType(CodeGenFacts.NCLMetaQueryType)
	};

	private static readonly ArgumentSyntax[] ConstructorArguments = ConstructorParameters.Select((ParameterSyntax p) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(p.Identifier))).ToArray();

	private MethodDeclarationSyntax initializeComponentMethod;

	protected override TypeSyntax BaseClassType { get; } = CodeGenFacts.NavQueryType;

	protected override TypeSyntax ClassType { get; }

	protected override ImmutableArray<VariableSymbol> GlobalVariables { get; }

	public QueryCodeBuilder(QueryTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		GlobalVariables = GetDeclaredGlobalVariables();
		ClassType = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(moduleBuilder.Options);
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		initializeComponentMethod = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(Symbol.GlobalVariables, ModuleBuilder.Options);
		ConstructorDeclarationSyntax constructorDeclarationSyntax = ApplicationObjectEmitHelper.CreateConstructorDeclaration(ClassType.ToString(), Symbol.Id, initializeComponentMethod != null, ConstructorParameters, ConstructorArguments, (ArgumentListSyntax syntax) => syntax);
		classDeclaration = classDeclaration.AddMembers(constructorDeclarationSyntax);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(ClassType.ToString(), ConstructorParameters, ConstructorArguments);
		classDeclaration = classDeclaration.AddMembers(methodDeclarationSyntax);
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

	public override ClassDeclarationSyntax AddInitializeComponentMethod(ClassDeclarationSyntax classDeclaration)
	{
		if (initializeComponentMethod != null)
		{
			classDeclaration = classDeclaration.AddMembers(initializeComponentMethod);
		}
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol);
	}

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddProperties(classDeclaration);
		return classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(ClassType, "CurrQuery").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(ClassType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
	}

	public override NamespaceDeclarationSyntax CreateNamespaceDeclaration()
	{
		return ApplicationObjectEmitHelper.CreateApplicationObjectNamespaceDeclaration();
	}

	private ImmutableArray<VariableSymbol> GetDeclaredGlobalVariables()
	{
		PooledHashSet<VariableSymbol> instance = PooledHashSet<VariableSymbol>.GetInstance();
		try
		{
			instance.AddAll(Symbol.GlobalVariables);
			foreach (QueryDataItemSymbol flattenedDataItem in Symbol.FlattenedDataItems)
			{
				SynthesizedGlobalVariableSymbol item = SynthesizedGlobalVariableSymbol.Create(Symbol, flattenedDataItem.RelatedTable, flattenedDataItem.Name, isThis: false, hasImplicitWith: false, Accessibility.Private, isImplicitDeclared: false, suppressTarget: true, flattenedDataItem.MetadataName);
				instance.Add(item);
			}
			return instance.ToImmutableArray();
		}
		finally
		{
			instance.Free();
		}
	}
}
