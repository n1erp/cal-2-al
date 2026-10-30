using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class ReportCodeBuilder : ObjectCodeBuilder<SourceReportTypeSymbol>
{
	private static readonly ParameterSyntax[] ConstructorParameters = new ParameterSyntax[1] { Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("metadata")).WithType(CodeGenFacts.NCLMetaReportType) };

	private static readonly ArgumentSyntax[] ConstructorArguments = ConstructorParameters.Select((ParameterSyntax p) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(p.Identifier))).ToArray();

	private readonly string objectName;

	private MethodDeclarationSyntax initializeComponentMethod;

	protected override TypeSyntax BaseClassType { get; } = CodeGenFacts.NavReportType;

	protected override TypeSyntax ClassType { get; }

	protected override ImmutableArray<VariableSymbol> GlobalVariables { get; }

	public ReportCodeBuilder(SourceReportTypeSymbol report, ModuleBuilder moduleBuilder)
		: base(report, moduleBuilder)
	{
		GlobalVariables = GetDeclaredGlobalVariables();
		ClassType = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(moduleBuilder.Options);
		objectName = ClassType.ToString();
	}

	public override ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(objectName, ConstructorParameters, ConstructorArguments);
		classDeclaration = classDeclaration.AddMembers(methodDeclarationSyntax);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		initializeComponentMethod = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(GlobalVariables, ModuleBuilder.Options, AppendToInitializeComponent);
		bool hasConstructorBodyInitializeComponentCall = initializeComponentMethod != null;
		ConstructorDeclarationSyntax constructorDeclarationSyntax = ApplicationObjectEmitHelper.CreateConstructorDeclaration(objectName, Symbol.Id, hasConstructorBodyInitializeComponentCall, ConstructorParameters, ConstructorArguments, (ArgumentListSyntax syntax) => syntax);
		classDeclaration = classDeclaration.AddMembers(constructorDeclarationSyntax);
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

	public override ClassDeclarationSyntax AddEvaluateCaptionClass(ClassDeclarationSyntax classDeclaration)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrStringType, "EvaluateCaptionClass").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.FieldNoParameterIdentifier).WithType(CodeGenFacts.ClrInt32Type));
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("EnsureGlobalVariablesInitialized")))));
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ClrStringType, CodeGenFacts.EmptyPropertyNameSyntax)));
		return classDeclaration.AddMembers(methodDeclarationSyntax);
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol);
	}

	public override ClassDeclarationSyntax AddFrameworkMethods(ClassDeclarationSyntax classDeclaration)
	{
		PooledDictionary<string, ExpressionSyntax> instance = PooledDictionary<string, ExpressionSyntax>.GetInstance();
		PooledDictionary<int, Tuple<int, ExpressionSyntax>> instance2 = PooledDictionary<int, Tuple<int, ExpressionSyntax>>.GetInstance();
		PooledDictionary<int, string> instance3 = PooledDictionary<int, string>.GetInstance();
		try
		{
			ProcessDataItems(instance2, instance3, instance);
			classDeclaration = EmitEvaluateSourceExpression(classDeclaration, instance);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetAutoFormatString(classDeclaration, instance2, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword, callExtensionMethod: false);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetDecimalPlaces(classDeclaration, instance3, Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword, callExtensionMethod: false);
			classDeclaration = AddRequestPageInnerClass(classDeclaration);
			return classDeclaration;
		}
		finally
		{
			instance.Free();
			instance2.Free();
			instance3.Free();
		}
	}

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddProperties(classDeclaration);
		TypeSyntax requestPageType = CodeGenFacts.RequestPageType;
		classDeclaration = classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(requestPageType, "RequestOptionsPage").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NewKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword)).AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(requestPageType, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RequestOptionsPage")))))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RequestOptionsPage")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("value")))))));
		TypeSyntax applicationObjectTypeWithSuffixTypeSyntax = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(ModuleBuilder.Options);
		classDeclaration = classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(applicationObjectTypeWithSuffixTypeSyntax, "CurrReport").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(applicationObjectTypeWithSuffixTypeSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
		return classDeclaration;
	}

	public override NamespaceDeclarationSyntax CreateNamespaceDeclaration()
	{
		return ApplicationObjectEmitHelper.CreateApplicationObjectNamespaceDeclaration();
	}

	internal ClassDeclarationSyntax AddRequestPageInnerClass(ClassDeclarationSyntax classDecl)
	{
		ICodeEmitter<MemberDeclarationSyntax> emitter = Symbol.RequestPage.GetEmitter(ModuleBuilder);
		return classDecl.AddMembers(emitter.Emit());
	}

	private void AppendToInitializeComponent(ArrayBuilder<StatementSyntax> builder)
	{
		AppendDataItemCreation(builder);
		AppendDataItemInitialization(builder);
		AppendRequestPageCreation(builder);
	}

	private static void AppendRequestPageCreation(ArrayBuilder<StatementSyntax> builder)
	{
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RequestOptionsPage")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.RequestPageType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Metadata"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RequestFormMetadata")))), null))));
	}

	private void AppendDataItemCreation(ArrayBuilder<StatementSyntax> builder)
	{
		foreach (ReportDataItemSymbol flattenedDataItem in Symbol.FlattenedDataItems)
		{
			ObjectCreationExpressionSyntax value = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.DataItemType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(CodeGenFacts.NavRecordHandleType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(flattenedDataItem.MetadataName)))), null);
			builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.DataItemType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(flattenedDataItem.MetadataName + "DataItem")).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(value)))));
		}
	}

	private ImmutableArray<VariableSymbol> GetDeclaredGlobalVariables()
	{
		PooledHashSet<VariableSymbol> instance = PooledHashSet<VariableSymbol>.GetInstance();
		try
		{
			instance.AddAll(Symbol.GlobalVariables);
			foreach (ReportDataItemSymbol flattenedDataItem in Symbol.FlattenedDataItems)
			{
				SynthesizedGlobalVariableSymbol item = SynthesizedGlobalVariableSymbol.Create(Symbol, flattenedDataItem.Type, flattenedDataItem.Name, isThis: false, hasImplicitWith: false, Accessibility.Private, isImplicitDeclared: false, suppressTarget: true, flattenedDataItem.MetadataName);
				instance.Add(item);
			}
			return instance.ToImmutableArray();
		}
		finally
		{
			instance.Free();
		}
	}

	private static ClassDeclarationSyntax EmitEvaluateSourceExpression(ClassDeclarationSyntax classDecl, Dictionary<string, ExpressionSyntax> expressions)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.NavValueType, "EvaluateSourceExpression").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("fieldName")).WithType(CodeGenFacts.ClrStringType));
		if (expressions.Count != 0)
		{
			methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("fieldName")).AddSections(expressions.Select((KeyValuePair<string, ExpressionSyntax> e) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(e.Key))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(e.Value))).ToArray()));
		}
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression)));
		return classDecl.AddMembers(methodDeclarationSyntax);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private void ProcessDataItems(IDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, IDictionary<int, string> decimalPlaceString, IDictionary<string, ExpressionSyntax> expressions)
	{
		foreach (SourceReportColumnSymbol item in Symbol.FlattenedColumns.Where((SourceReportColumnSymbol m) => m.Kind == SymbolKind.ReportColumn))
		{
			ExpressionSyntax expression = ModuleBuilder.CodeGenerator.EmitExpression(item.SourceExpression);
			ExpressionSyntax value = CodeGenerator.EmitToJokerConversion(item.SourceExpression.Type, expression);
			expressions.Add(item.MetadataName, value);
			bool hasDecimalPlacesFormatString = TryAddDecimalPlacesFormatString(decimalPlaceString, item);
			bool hasAutoFormat = TryAddAutoFormatExpression(autoFormatStrings, item);
			TryAddSourceExpression(expressions, item, hasDecimalPlacesFormatString, hasAutoFormat);
		}
	}

	private void TryAddSourceExpression(IDictionary<string, ExpressionSyntax> expressions, SourceReportColumnSymbol column, bool hasDecimalPlacesFormatString, bool hasAutoFormat)
	{
		BoundFieldAccess boundFieldAccess = column.SourceExpression as BoundFieldAccess;
		if ((hasDecimalPlacesFormatString | hasAutoFormat) || column.Type.NavTypeKind == NavTypeKind.Decimal || (boundFieldAccess != null && boundFieldAccess.FieldSymbol.Type.NavTypeKind == NavTypeKind.Decimal))
		{
			ExpressionSyntax decimalString = GetDecimalString(((ReportDataItemSymbol)column.ContainingSymbol).MetadataName, boundFieldAccess?.FieldSymbol.Id, column.Id);
			string key = column.MetadataName + "Format";
			expressions.Add(key, decimalString);
		}
	}

	private static bool TryAddDecimalPlacesFormatString(IDictionary<int, string> decimalPlaceString, SourceReportColumnSymbol column)
	{
		bool result = false;
		BoundFieldAccess boundFieldAccess = column.SourceExpression as BoundFieldAccess;
		string decimalPlacesFormatString = ApplicationObjectEmitHelper.GetDecimalPlacesFormatString(column);
		if (decimalPlacesFormatString == null && boundFieldAccess != null)
		{
			decimalPlacesFormatString = ApplicationObjectEmitHelper.GetDecimalPlacesFormatString(boundFieldAccess.FieldSymbol);
		}
		if (decimalPlacesFormatString != null)
		{
			decimalPlaceString.Add(column.Id, decimalPlacesFormatString);
			result = true;
		}
		return result;
	}

	private bool TryAddAutoFormatExpression(IDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, SourceReportColumnSymbol column)
	{
		BoundCodeExpressionPropertyValue boundPropertyValue = column.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.AutoFormatExpression);
		BoundLiteralPropertyValue boundPropertyValue2 = column.GetBoundPropertyValue<BoundLiteralPropertyValue>(PropertyKind.AutoFormatType);
		if (boundPropertyValue != null && boundPropertyValue2 != null && boundPropertyValue2.Value.Int32Value > 0)
		{
			ExpressionSyntax item = ModuleBuilder.CodeGenerator.EmitExpression(boundPropertyValue.Expression);
			autoFormatStrings.Add(column.Id, new Tuple<int, ExpressionSyntax>(boundPropertyValue2.Value.Int32Value, item));
			return true;
		}
		if (boundPropertyValue2 != null && boundPropertyValue2.Value?.Int32Value == 2)
		{
			ExpressionSyntax item = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.RuntimeTextType).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(string.Empty)));
			autoFormatStrings.Add(column.Id, new Tuple<int, ExpressionSyntax>(boundPropertyValue2.Value.Int32Value, item));
			return true;
		}
		return false;
	}

	private static ExpressionSyntax GetDecimalString(string dataItemName, int? fieldId, int controlId)
	{
		LiteralExpressionSyntax expression = (fieldId.HasValue ? CodeGenHelper.LiteralExpression(fieldId.Value) : CodeGenFacts.NullLiteralExpression);
		return CodeGenFacts.ALCompilerConvertToDotNetFormatStringCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenFacts.GetDecimalStringMethodNameSyntax).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(dataItemName))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(controlId)))));
	}

	private void AppendDataItemInitialization(ArrayBuilder<StatementSyntax> builder)
	{
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("BeginInitialization")))));
		foreach (ReportDataItemSymbol flattenedDataItem in Symbol.FlattenedDataItems)
		{
			IdentifierNameSyntax expression = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(flattenedDataItem.MetadataName + "DataItem");
			foreach (MethodSymbol trigger in flattenedDataItem.Triggers)
			{
				MemberAccessExpressionSyntax left = CodeGenHelper.MemberAccessExpression(expression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(trigger.Name));
				ObjectCreationExpressionSyntax objectCreationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavTrigger"));
				objectCreationExpressionSyntax = objectCreationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(trigger.MetadataName)));
				ExpressionStatementSyntax item = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, left, objectCreationExpressionSyntax));
				builder.Add(item);
			}
			ExpressionStatementSyntax item2 = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Add"))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)));
			builder.Add(item2);
		}
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("EndInitialization")))));
	}
}
