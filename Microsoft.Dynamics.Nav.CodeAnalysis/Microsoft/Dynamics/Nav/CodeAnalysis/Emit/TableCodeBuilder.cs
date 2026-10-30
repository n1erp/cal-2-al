using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class TableCodeBuilder : TableBaseCodeBuilder<SourceTableTypeSymbol>
{
	private static readonly ParameterSyntax[] ConstructorParameters = new ParameterSyntax[5]
	{
		Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("metaTable")).WithType(CodeGenFacts.NclMetaTableType),
		Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("isTemporary")).WithType(CodeGenFacts.ClrBoolType),
		Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("sharedTable")).WithType(CodeGenFacts.NavRecordType),
		Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("companyName")).WithType(CodeGenFacts.ClrStringType),
		Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("securityFiltering")).WithType(CodeGenFacts.RuntimeNavSecurityFilterTypeSyntax)
	};

	private static readonly ArgumentSyntax[] ConstructorArguments = ConstructorParameters.Select((ParameterSyntax p) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(p.Identifier))).ToArray();

	private MethodDeclarationSyntax initializeComponentMethod;

	protected override TypeSyntax BaseClassType { get; } = CodeGenFacts.NavRecordType;

	protected override TypeSyntax ClassType { get; }

	protected override bool DelayInitializeComponent { get; } = true;

	protected override MemberDeclarationSyntax InitializeComponentMethod => initializeComponentMethod;

	public TableCodeBuilder(SourceTableTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		ClassType = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(ModuleBuilder.Options);
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		initializeComponentMethod = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(Symbol.GlobalVariables, ModuleBuilder.Options);
		ConstructorDeclarationSyntax constructorDeclarationSyntax = ApplicationObjectEmitHelper.CreateConstructorDeclaration(ClassType.ToString(), Symbol.Id, HasConstructorBodyInitializeComponentCall(), ConstructorParameters, ConstructorArguments, (ArgumentListSyntax syntax) => syntax);
		classDeclaration = classDeclaration.AddMembers(constructorDeclarationSyntax);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddProperties(classDeclaration);
		return classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(ClassType, "Rec").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(ClassType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(ClassType, "xRec").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(ClassType, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.OldRecordPropertyNameSyntax)))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
	}

	public override ClassDeclarationSyntax AddFrameworkMethods(ClassDeclarationSyntax classDeclaration)
	{
		if (InitializeComponentMethod != null)
		{
			classDeclaration = classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, "InitializeGlobalVariables").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.InitializeComponentMethodNameSyntax)))));
		}
		PooledDictionary<int, Tuple<int, ExpressionSyntax>> instance = PooledDictionary<int, Tuple<int, ExpressionSyntax>>.GetInstance();
		PooledDictionary<int, string> instance2 = PooledDictionary<int, string>.GetInstance();
		try
		{
			ProcessFields(instance, instance2);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetAutoFormatString(classDeclaration, instance, Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetDecimalPlaces(classDeclaration, instance2, Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword);
			return classDeclaration;
		}
		finally
		{
			instance.Free();
			instance2.Free();
		}
	}

	public override ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(ClassType.ToString(), ConstructorParameters, ConstructorArguments);
		return classDeclaration.AddMembers(methodDeclarationSyntax);
	}

	public override ClassDeclarationSyntax AddEvaluateCaptionClass(ClassDeclarationSyntax classDeclaration)
	{
		IEnumerable<SourceFieldSymbol> fieldsToEmitCaptionClassFor = Symbol.GetFields().Cast<SourceFieldSymbol>();
		return ApplicationObjectEmitHelper.EmitEvaluateCaptionClass(isExtension: false, (ArrayBuilder<SwitchSectionSyntax> statementBuilder) =>
		{
			EmitEvaluateCaptionClassSwitchSections(fieldsToEmitCaptionClassFor, statementBuilder);
		}, classDeclaration);
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol);
	}

	private void ProcessFields(PooledDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, PooledDictionary<int, string> decimalPlaceString)
	{
		foreach (SourceFieldSymbol item in Symbol.GetFields().Cast<SourceFieldSymbol>())
		{
			if (item.Type.NavTypeKind == NavTypeKind.Decimal)
			{
				BoundLiteralPropertyValue boundPropertyValue = item.GetBoundPropertyValue<BoundLiteralPropertyValue>(PropertyKind.AutoFormatType);
				BoundCodeExpressionPropertyValue boundPropertyValue2 = item.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.AutoFormatExpression);
				BoundDecimalPlacesPropertyValue boundPropertyValue3;
				if (boundPropertyValue2 != null && boundPropertyValue != null && boundPropertyValue.Value.Int32Value > 0)
				{
					ExpressionSyntax expression = ModuleBuilder.CodeGenerator.EmitExpression(boundPropertyValue2.Expression);
					expression = CodeGenerator.EmitToTextConversion(TypeSymbol.GetNavTypeSymbol(NavTypeKind.Text), boundPropertyValue2.Expression.Type, expression);
					autoFormatStrings.Add(item.Id, new Tuple<int, ExpressionSyntax>(boundPropertyValue.Value.Int32Value, expression));
				}
				else if (boundPropertyValue != null && boundPropertyValue.Value.Int32Value == 2)
				{
					ExpressionSyntax expression = CodeGenFacts.AlCompilerInvokeAutoFormatTranslateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(2)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.RuntimeTextType).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(string.Empty)))));
					autoFormatStrings.Add(item.Id, new Tuple<int, ExpressionSyntax>(boundPropertyValue.Value.Int32Value, expression));
				}
				else if ((boundPropertyValue == null || boundPropertyValue.Value.Int32Value == 0) && (boundPropertyValue3 = item.GetBoundPropertyValue<BoundDecimalPlacesPropertyValue>(PropertyKind.DecimalPlaces)) != null)
				{
					string formatString = ApplicationObjectEmitHelper.GetFormatString(boundPropertyValue3, 0);
					decimalPlaceString.Add(item.Id, formatString);
				}
			}
		}
	}
}
