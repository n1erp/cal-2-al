using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal sealed class TableExtensionCodeBuilder : TableBaseCodeBuilder<SourceTableExtensionTypeSymbol>
{
	private MemberDeclarationSyntax initializeComponentMethod;

	protected override MemberDeclarationSyntax InitializeComponentMethod => initializeComponentMethod;

	protected override TypeSyntax BaseClassType { get; } = CodeGenFacts.NavRecordExtensionType;

	protected override TypeSyntax ClassType { get; }

	protected override bool DelayInitializeComponent { get; } = true;

	protected override Microsoft.CodeAnalysis.CSharp.SyntaxKind OnClearMethodAccessModifier { get; } = Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword;

	public TableExtensionCodeBuilder(SourceTableExtensionTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		ClassType = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(ModuleBuilder.Options);
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		initializeComponentMethod = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(Symbol.GlobalVariables, ModuleBuilder.Options);
		ConstructorDeclarationSyntax constructorDeclarationSyntax = ApplicationObjectEmitHelper.CreateConstructorDeclaration(ClassType.ToString(), Symbol.Id, HasConstructorBodyInitializeComponentCall(), null, null, (ArgumentListSyntax syntax) => syntax);
		classDeclaration = classDeclaration.AddMembers(constructorDeclarationSyntax);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddEvaluateCaptionClass(ClassDeclarationSyntax classDeclaration)
	{
		IEnumerable<SourceFieldSymbol> fieldsToEmitCaptionClassFor = Symbol.GetAddedFields().Cast<SourceFieldSymbol>();
		IEnumerable<SourceChangeModifyFieldSymbol> modifiedFieldsToEmitCaptionClassFor = Symbol.ModifiedFields();
		return ApplicationObjectEmitHelper.EmitEvaluateCaptionClass(isExtension: true, (ArrayBuilder<SwitchSectionSyntax> statementBuilder) =>
		{
			EmitEvaluateCaptionClassSwitchSections(fieldsToEmitCaptionClassFor, statementBuilder);
			EmitEvaluateCaptionClassSwitchSectionsForModified(modifiedFieldsToEmitCaptionClassFor, statementBuilder);
		}, classDeclaration);
	}

	private void EmitEvaluateCaptionClassSwitchSectionsForModified(IEnumerable<SourceChangeModifyFieldSymbol> sourceModifyFields, ArrayBuilder<SwitchSectionSyntax> builder)
	{
		foreach (SourceChangeModifyFieldSymbol sourceModifyField in sourceModifyFields)
		{
			PropertySymbol property = sourceModifyField.GetProperty(PropertyKind.CaptionClass);
			if (property != null)
			{
				BoundCodeExpressionPropertyValue boundCodeExpressionPropertyValue = property.Property.PropertyValue as BoundCodeExpressionPropertyValue;
				builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(((ISymbolWithId)sourceModifyField.Target).Id))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(ModuleBuilder.CodeGenerator.EmitExpression(boundCodeExpressionPropertyValue.Expression))));
			}
		}
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol);
	}

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddProperties(classDeclaration);
		TypeSyntax navRecordType = CodeGenFacts.NavRecordType;
		return classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(navRecordType, "ParentObject").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(navRecordType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ParentObject")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken))))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NewKeyword)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(navRecordType, "Rec").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(navRecordType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ParentObject")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken))))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(navRecordType, "xRec").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(navRecordType, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ParentObject")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("OldRecord")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken))))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
	}

	public override ClassDeclarationSyntax AddFrameworkMethods(ClassDeclarationSyntax classDeclaration)
	{
		if (InitializeComponentMethod != null)
		{
			classDeclaration = classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, "InitializeGlobalVariables").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.InitializeComponentMethodNameSyntax)))));
		}
		PooledDictionary<int, Tuple<int, ExpressionSyntax>> instance = PooledDictionary<int, Tuple<int, ExpressionSyntax>>.GetInstance();
		PooledDictionary<int, string> instance2 = PooledDictionary<int, string>.GetInstance();
		try
		{
			ProcessFields(instance, instance2);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetAutoFormatStringForExtensions(classDeclaration, instance);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetDecimalPlacesForExtensions(classDeclaration, instance2);
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
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(ClassType.ToString(), (ParameterSyntax[])null, (ArgumentSyntax[])null);
		return classDeclaration.AddMembers(methodDeclarationSyntax);
	}

	protected override EmittedMethod BeforeEmitMethod(EmittedMethod emittedMethod)
	{
		if (emittedMethod.Method.IsTrigger() && emittedMethod.Method.ContainingSymbol.Kind == SymbolKind.Change)
		{
			return emittedMethod.WithDeclaration(emittedMethod.MethodDeclaration.AddAttributeLists(CodeGenHelper.FieldTriggerHandlerAttribute(((SourceTriggerSymbol)emittedMethod.Method).TriggerInfo.Name, ((FieldSymbol)((SourceChangeModifyFieldSymbol)emittedMethod.Method.ContainingSymbol).Target).Id)));
		}
		return base.BeforeEmitMethod(emittedMethod);
	}

	private void ProcessFields(PooledDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, PooledDictionary<int, string> decimalPlaceString)
	{
		foreach (SourceFieldSymbol item2 in Symbol.GetAddedFields().Cast<SourceFieldSymbol>())
		{
			if (item2.Type.NavTypeKind == NavTypeKind.Decimal)
			{
				BoundLiteralPropertyValue boundPropertyValue = item2.GetBoundPropertyValue<BoundLiteralPropertyValue>(PropertyKind.AutoFormatType);
				BoundCodeExpressionPropertyValue boundPropertyValue2 = item2.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.AutoFormatExpression);
				BoundDecimalPlacesPropertyValue boundPropertyValue3;
				if (boundPropertyValue2 != null && boundPropertyValue != null && boundPropertyValue.Value.Int32Value > 0)
				{
					ExpressionSyntax item = ModuleBuilder.CodeGenerator.EmitExpression(boundPropertyValue2.Expression);
					autoFormatStrings.Add(item2.Id, new Tuple<int, ExpressionSyntax>(boundPropertyValue.Value.Int32Value, item));
				}
				else if ((boundPropertyValue == null || boundPropertyValue.Value.Int32Value == 0) && (boundPropertyValue3 = item2.GetBoundPropertyValue<BoundDecimalPlacesPropertyValue>(PropertyKind.DecimalPlaces)) != null)
				{
					string formatString = ApplicationObjectEmitHelper.GetFormatString(boundPropertyValue3, 0);
					decimalPlaceString.Add(item2.Id, formatString);
				}
			}
		}
	}
}
