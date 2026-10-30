using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class RequestPageCodeBuilder : PageBaseCodeBuilder<RequestPageTypeSymbol>
{
	private static readonly string RequestPageClassName = CodeGenFacts.RequestPageType.ToString();

	private MemberDeclarationSyntax initializeComponentMethod;

	private string CurrentObjectName { get; }

	private TypeSyntax ParentClassType { get; }

	protected override TypeSyntax BaseClassType { get; } = CodeGenFacts.NavRequestPageBaseType;

	protected override TypeSyntax ClassType { get; }

	private ApplicationObjectTypeSymbol ContainingApplicationObject { get; }

	protected override MemberDeclarationSyntax InitializeComponentMethod => initializeComponentMethod;

	private ParameterSyntax[] ConstructorParameters { get; }

	protected override bool HasRelatedTable => Symbol.RelatedTable != null;

	protected override ImmutableArray<ControlSymbol> FlattenedControls => Symbol.FlattenedControls;

	public RequestPageCodeBuilder(RequestPageTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		ContainingApplicationObject = symbol.ContainingSymbol;
		CurrentObjectName = ContainingApplicationObject.GetCurrObjectName();
		ParentClassType = ContainingApplicationObject.GetApplicationObjectTypeWithSuffixTypeSyntax(ModuleBuilder.Options);
		ConstructorParameters = new ParameterSyntax[2]
		{
			Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.ParentParameterIdentifier).WithType(ParentClassType),
			Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.MetaFormParameterIdentifier).WithType(CodeGenFacts.MasterPageType)
		};
		ClassType = CodeGenFacts.RequestPageType;
	}

	public override NamespaceDeclarationSyntax CreateNamespaceDeclaration()
	{
		return null;
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		ConstructorDeclarationSyntax constructorDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ConstructorDeclaration(RequestPageClassName).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)).AddParameterListParameters(ConstructorParameters);
		ArgumentListSyntax argumentList = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CodeGenFacts.ParentParameterIdentifier)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.NavCurrentThreadSessionCompanySharedObjectsValue), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("metaForm")));
		constructorDeclarationSyntax = constructorDeclarationSyntax.AddBodyStatements().WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ConstructorInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxKind.BaseConstructorInitializer, argumentList));
		MemberAccessExpressionSyntax left = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CurrentObjectName));
		IdentifierNameSyntax right = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("parent");
		MemberAccessExpressionSyntax left2 = CodeGenHelper.MemberAccessExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CurrentObjectName)), CodeGenFacts.RequestOptionsPageNameSyntax);
		constructorDeclarationSyntax = constructorDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, left, right)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, left2, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression())));
		initializeComponentMethod = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(Symbol.GlobalVariables, ModuleBuilder.Options, AppendToInitializeComponent);
		if (initializeComponentMethod != null)
		{
			constructorDeclarationSyntax = constructorDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.InitializeComponentMethodNameSyntax))));
		}
		return classDeclaration.AddMembers(constructorDeclarationSyntax);
	}

	public override ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration)
	{
		ArgumentSyntax[] constructorArguments = new ArgumentSyntax[2]
		{
			Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("parent")),
			Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("metaForm"))
		};
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(RequestPageClassName, constructorArguments, ConstructorParameters);
		return classDeclaration.AddMembers(methodDeclarationSyntax);
	}

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddProperties(classDeclaration);
		classDeclaration = PageEmitHelper.AddRecAndCurrPage(classDeclaration, HasRelatedTable, ClassType);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddGlobalVariables(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddGlobalVariables(classDeclaration);
		FieldDeclarationSyntax fieldDeclarationSyntax = CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(CurrentObjectName), ParentClassType).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword));
		return classDeclaration.AddMembers(fieldDeclarationSyntax);
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol, 0);
	}

	protected override void AppendToInitializeComponent(ArrayBuilder<StatementSyntax> builder)
	{
		builder.Add(CodeGenFacts.InvokeCallInitializeComponentExtensionMethodStatement);
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.InitializeFormMethodNameSyntax))));
		base.AppendToInitializeComponent(builder);
		IEnumerable<VariableSymbol> inDataSetVariables = ContainingApplicationObject.GlobalVariables.Where((VariableSymbol g) => g.IsInDataSet);
		MemberAccessExpressionSyntax variableScopeAccess = CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(ContainingApplicationObject.GetCurrObjectName()));
		PageEmitHelper.ProcessInDataSetVariables(Symbol, inDataSetVariables, variableScopeAccess, builder);
		AppendRegisterSourceExpression(builder);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private void AppendRegisterSourceExpression(ArrayBuilder<StatementSyntax> builder)
	{
		SourceReportTypeSymbol sourceReportTypeSymbol = ContainingApplicationObject as SourceReportTypeSymbol;
		if (sourceReportTypeSymbol != null)
		{
			for (int i = 0; i < sourceReportTypeSymbol.FlattenedDataItems.Length; i++)
			{
				EmitDataItemTableViewExpression(builder, i, sourceReportTypeSymbol.FlattenedDataItems[i].MetadataName, "CurrReport");
			}
		}
		SourceXmlPortTypeSymbol sourceXmlPortTypeSymbol = ContainingApplicationObject as SourceXmlPortTypeSymbol;
		if (!(sourceXmlPortTypeSymbol != null))
		{
			return;
		}
		foreach (SourceXmlPortNodeSymbol item in sourceXmlPortTypeSymbol.FlattenedNodes.Cast<SourceXmlPortNodeSymbol>())
		{
			if (item.SourceTypeKind == XmlPortSourceTypeKind.Table)
			{
				EmitDataItemTableViewExpression(builder, item.Id, item.MetadataName, "currXMLport");
			}
		}
	}

	private static void EmitDataItemTableViewExpression(ArrayBuilder<StatementSyntax> builder, int idOrIndex, string metadataName, string currObjectPropertyName)
	{
		string dataItemTableViewName = MetadataEmitterHelper.GetDataItemTableViewName(idOrIndex);
		ArrayTypeSyntax targetType = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayType(CodeGenFacts.ClrByteType).AddRankSpecifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayRankSpecifier().AddSizes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.OmittedArraySizeExpression()));
		MemberAccessExpressionSyntax expression = CodeGenHelper.MemberAccessExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(currObjectPropertyName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(metadataName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Target"));
		ExpressionSyntax getter = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AnonymousMethodExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenerator.EmitToJokerConversion(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("GetTableView"))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ToByteArray")))))));
		ExpressionSyntax setter = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AnonymousMethodExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("SetTableView"))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavTableView"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Create"))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("setValue"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("GetBytes")))))))))).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("setValue")).WithType(CodeGenFacts.NavValueType));
		builder.Add(PageEmitHelper.CreateRegisterSourceExpressionSyntax(dataItemTableViewName, dataItemTableViewName, targetType, CodeGenFacts.SourceExpressionTypeDataItemExpression, getter, setter));
	}
}
