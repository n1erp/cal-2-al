using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Rewriter;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal static class ApplicationObjectEmitHelper
{
	public static ConstructorDeclarationSyntax CreateConstructorDeclaration(string objectName, int objectId, bool hasConstructorBodyInitializeComponentCall, ParameterSyntax[] constructorParameters, ArgumentSyntax[] constructorArguments, Func<ArgumentListSyntax, ArgumentListSyntax> addBaseArguments)
	{
		ConstructorDeclarationSyntax constructorDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ConstructorDeclaration(objectName).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.ParentParameterIdentifier).WithType(CodeGenFacts.ITreeObjectType));
		ArgumentListSyntax argumentListSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.ParentParameterNameSyntax), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NumericLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(objectId))));
		if (constructorParameters != null)
		{
			constructorDeclarationSyntax = constructorDeclarationSyntax.AddParameterListParameters(constructorParameters);
			argumentListSyntax = argumentListSyntax.AddArguments(constructorArguments);
		}
		argumentListSyntax = addBaseArguments(argumentListSyntax);
		constructorDeclarationSyntax = constructorDeclarationSyntax.AddBodyStatements().WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ConstructorInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxKind.BaseConstructorInitializer, argumentListSyntax));
		if (hasConstructorBodyInitializeComponentCall)
		{
			constructorDeclarationSyntax = constructorDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.InitializeComponentMethodNameSyntax))));
		}
		return constructorDeclarationSyntax;
	}

	public static ClassDeclarationSyntax CreateClassDeclaration(string className, TypeSyntax baseClassType)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ClassDeclaration(className).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SealedKeyword)).AddBaseListTypes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SimpleBaseType(baseClassType));
	}

	public static MethodDeclarationSyntax EmitConstructMethod(string objectName, ParameterSyntax[] constructorParameters, ArgumentSyntax[] constructorArguments)
	{
		ArgumentSyntax[] constructorArguments2 = new List<ArgumentSyntax> { Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.ParentParameterNameSyntax) }.Concat(constructorArguments ?? new ArgumentSyntax[0]).ToArray();
		ParameterSyntax[] constructorParameters2 = new List<ParameterSyntax> { Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.ParentParameterIdentifier).WithType(CodeGenFacts.ITreeObjectType) }.Concat(constructorParameters ?? new ParameterSyntax[0]).ToArray();
		return EmitConstructMethod(objectName, constructorArguments2, constructorParameters2);
	}

	public static MethodDeclarationSyntax EmitInitializeComponentMethod(ImmutableArray<VariableSymbol> globalVariables, EmitOptions emitOptions, Action<ArrayBuilder<StatementSyntax>> appendToInitializeComponent = null)
	{
		ArrayBuilder<StatementSyntax> instance = ArrayBuilder<StatementSyntax>.GetInstance();
		foreach (VariableSymbol item in globalVariables)
		{
			if (!item.IsImplicitlyDeclared)
			{
				ExpressionSyntax expressionSyntax = CodeGenerator.EmitVariableInitializer(item, emitOptions);
				if (expressionSyntax != null)
				{
					instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(item.MetadataName)), expressionSyntax)));
				}
			}
		}
		appendToInitializeComponent?.Invoke(instance);
		if (instance.Count == 0)
		{
			instance.Free();
			return null;
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, CodeGenFacts.InitializeComponentMethodIdentifier).AddBodyStatements(instance.ToArrayAndFree());
	}

	public static ClassDeclarationSyntax EmitGlobalVariables(ImmutableArray<VariableSymbol> globalVariables, Microsoft.CodeAnalysis.CSharp.SyntaxKind onClearAccessModifier, ClassDeclarationSyntax classDeclaration, bool nonDebuggableEmit)
	{
		if (globalVariables.Length > 0)
		{
			classDeclaration = classDeclaration.AddMembers(EmitGlobalFieldDeclarations(globalVariables, nonDebuggableEmit));
			MethodDeclarationSyntax methodDeclarationSyntax = EmitOnClearMethod(globalVariables, onClearAccessModifier);
			if (methodDeclarationSyntax != null)
			{
				classDeclaration = classDeclaration.AddMembers(methodDeclarationSyntax);
			}
		}
		return classDeclaration;
	}

	public static NamespaceDeclarationSyntax CreateApplicationObjectNamespaceDeclaration()
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.NamespaceDeclaration(CodeGenFacts.BusinessApplicationNamespaceNameSyntax).AddUsings(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("System")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Microsoft.Dynamics.Nav.Runtime")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Microsoft.Dynamics.Nav.Runtime.Extensions")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Microsoft.Dynamics.Nav.Types")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Microsoft.Dynamics.Nav.Types.Exceptions")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Microsoft.Dynamics.Nav.Types.Metadata")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Microsoft.Dynamics.Nav.Common.Language")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.UsingDirective(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseName("Microsoft.Dynamics.Nav.EventSubscription")));
	}

	public static MethodDeclarationSyntax EmitConstructMethod(string objectName, ArgumentSyntax[] constructorArguments, ParameterSyntax[] constructorParameters)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName(objectName), CodeGenFacts.ConstructMethodIdentifier).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword));
		ArgumentListSyntax argumentListSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList();
		if (constructorArguments != null)
		{
			methodDeclarationSyntax = methodDeclarationSyntax.AddParameterListParameters(constructorParameters);
			argumentListSyntax = argumentListSyntax.AddArguments(constructorArguments);
		}
		return methodDeclarationSyntax.WithBody(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseTypeName(objectName), argumentListSyntax, null))));
	}

	public static ClassDeclarationSyntax EmitEvaluateCaptionClass(bool isExtension, Action<ArrayBuilder<SwitchSectionSyntax>> emitEvaluateCaptionClassSwitchSections, ClassDeclarationSyntax classDecl)
	{
		ArrayBuilder<SwitchSectionSyntax> instance = ArrayBuilder<SwitchSectionSyntax>.GetInstance();
		emitEvaluateCaptionClassSwitchSections?.Invoke(instance);
		classDecl = EmitEvaluateCaptionClassMethodDecl(classDecl, instance.ToArrayAndFree(), isExtension);
		return classDecl;
	}

	public static ClassDeclarationSyntax EmitPermissionsList(ContainerSymbol symbol, ClassDeclarationSyntax classDecl)
	{
		PropertySymbol property = symbol.GetProperty(PropertyKind.Permissions);
		if (property == null)
		{
			return classDecl;
		}
		uint[] indirectPermissionValues = ((BoundPermissionPropertyValue)property.Property.PropertyValue).IndirectPermissionValues;
		ExpressionSyntax[] array = new ExpressionSyntax[indirectPermissionValues.Length];
		for (int i = 0; i < indirectPermissionValues.Length; i++)
		{
			array[i] = CodeGenHelper.LiteralExpression(indirectPermissionValues[i]);
		}
		ArrayTypeSyntax type = CodeGenHelper.ArrayType(CodeGenFacts.ClrUInt32Type, 1);
		InitializerExpressionSyntax initialValue = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InitializerExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ArrayInitializerExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(array));
		FieldDeclarationSyntax fieldDeclarationSyntax = CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("indirectPermissionList"), type, initialValue, Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword, Microsoft.CodeAnalysis.CSharp.SyntaxKind.StaticKeyword);
		PropertyDeclarationSyntax propertyDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(type, "IndirectPermissionList").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(classDecl.Identifier), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("indirectPermissionList"))))));
		return classDecl.AddMembers(fieldDeclarationSyntax, propertyDeclarationSyntax);
	}

	public static ClassDeclarationSyntax AppendOnInvokeMethod(ClassDeclarationSyntax classDeclaration, ApplicationObjectTypeSymbol symbol)
	{
		return AppendOnInvokeMethod(classDeclaration, symbol, symbol.Id);
	}

	public static ClassDeclarationSyntax AppendOnInvokeMethod(ClassDeclarationSyntax classDeclaration, ObjectTypeSymbol symbol, int symbolId)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = EmitOnInvokeMethod(symbol, symbolId);
		if (methodDeclarationSyntax != null)
		{
			classDeclaration = classDeclaration.AddMembers(methodDeclarationSyntax);
		}
		return classDeclaration;
	}

	internal static MethodDeclarationSyntax EmitCallExtensionMethod(MethodDeclarationSyntax methodDecl, string methodName)
	{
		return methodDecl.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.ClrStringType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("result")).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression))))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(methodName))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("fieldNo")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("result")).WithRefOrOutKeyword(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.RefKeyword))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("result"))));
	}

	internal static string GetDecimalPlacesFormatString(ContainerSymbol c)
	{
		BoundDecimalPlacesPropertyValue boundPropertyValue = c.GetBoundPropertyValue<BoundDecimalPlacesPropertyValue>(PropertyKind.DecimalPlaces);
		if (boundPropertyValue != null)
		{
			return GetFormatString(boundPropertyValue, 0);
		}
		return null;
	}

	internal static PropertyDeclarationSyntax EmitObjectNameProperty(string objectName)
	{
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(CodeGenFacts.ClrStringType, CodeGenFacts.ObjectNamePropertyIdentifier).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringLiteralExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Literal(objectName))))));
	}

	internal static ClassDeclarationSyntax EmitGetAutoFormatString(ClassDeclarationSyntax classDecl, PooledDictionary<int, Tuple<int, ExpressionSyntax>> expressions, Microsoft.CodeAnalysis.CSharp.SyntaxKind accessKeyword = Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword, bool callExtensionMethod = true)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrStringType, "GetAutoFormatString").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(accessKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("fieldNo")).WithType(CodeGenFacts.ClrInt32Type));
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("EnsureGlobalVariablesInitialized")))));
		if (callExtensionMethod)
		{
			methodDeclarationSyntax = EmitCallExtensionMethod(methodDeclarationSyntax, "CallGetAutoFormatStringExtensionMethod");
		}
		if (expressions.Count != 0)
		{
			IEnumerable<SwitchSectionSyntax> source = expressions.Select((KeyValuePair<int, Tuple<int, ExpressionSyntax>> e) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(e.Key))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenFacts.AlCompilerInvokeAutoFormatTranslateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(e.Value.Item1)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(e.Value.Item2)))));
			methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("fieldNo")).AddSections(source.ToArray()));
		}
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ClrStringType, CodeGenFacts.EmptyPropertyNameSyntax)));
		return classDecl.AddMembers(methodDeclarationSyntax);
	}

	internal static ClassDeclarationSyntax EmitGetDecimalPlaces(ClassDeclarationSyntax classDecl, PooledDictionary<int, string> formatStrings, Microsoft.CodeAnalysis.CSharp.SyntaxKind accessKeyword = Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword, bool callExtensionMethod = true)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrStringType, "GetDecimalPlaces").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(accessKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("fieldNo")).WithType(CodeGenFacts.ClrInt32Type));
		if (callExtensionMethod)
		{
			methodDeclarationSyntax = EmitCallExtensionMethod(methodDeclarationSyntax, "CallGetDecimalPlacesExtensionMethod");
		}
		if (formatStrings.Count != 0)
		{
			methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("fieldNo")).AddSections(formatStrings.Select((KeyValuePair<int, string> e) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(e.Key))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.LiteralExpression(e.Value)))).ToArray()));
		}
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ClrStringType, CodeGenFacts.EmptyPropertyNameSyntax)));
		return classDecl.AddMembers(methodDeclarationSyntax);
	}

	internal static ClassDeclarationSyntax EmitGetAutoFormatStringForExtensions(ClassDeclarationSyntax classDecl, PooledDictionary<int, Tuple<int, ExpressionSyntax>> expressions)
	{
		if (expressions.Count == 0)
		{
			return classDecl;
		}
		return classDecl.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrStringType, "GetAutoFormatString").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("fieldNo")).WithType(CodeGenFacts.ClrInt32Type), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("handled")).WithType(CodeGenFacts.ClrBoolType).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword)))
			.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("handled"), CodeGenFacts.TrueLiteralExpression)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("fieldNo")).AddSections(expressions.Select((KeyValuePair<int, Tuple<int, ExpressionSyntax>> e) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(e.Key))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenFacts.AlCompilerInvokeAutoFormatTranslateCall.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(e.Value.Item1)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(e.Value.Item2))))).ToArray()).AddSections(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.DefaultSwitchLabel()).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("handled"), CodeGenFacts.FalseLiteralExpression)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ClrStringType, CodeGenFacts.EmptyPropertyNameSyntax))))));
	}

	internal static ClassDeclarationSyntax EmitGetDecimalPlacesForExtensions(ClassDeclarationSyntax classDecl, PooledDictionary<int, string> formatStrings)
	{
		if (formatStrings.Count == 0)
		{
			return classDecl;
		}
		return classDecl.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrStringType, "GetDecimalPlaces").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("fieldNo")).WithType(CodeGenFacts.ClrInt32Type), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("handled")).WithType(CodeGenFacts.ClrBoolType).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword)))
			.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("handled"), CodeGenFacts.TrueLiteralExpression)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("fieldNo")).AddSections(formatStrings.Select((KeyValuePair<int, string> e) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(e.Key))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.LiteralExpression(e.Value)))).ToArray()).AddSections(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.DefaultSwitchLabel()).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("handled"), CodeGenFacts.FalseLiteralExpression)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ClrStringType, CodeGenFacts.EmptyPropertyNameSyntax))))));
	}

	internal static string GetFormatString(BoundDecimalPlacesPropertyValue decimalPlaces, int autoFormatType)
	{
		DebugAssertHelper.Assert(decimalPlaces != null);
		return string.Format(CultureInfo.InvariantCulture, "<Precision,{0}:{1}><Standard format,{2}>", decimalPlaces.Minimum, decimalPlaces.Maximum?.ToString(CultureInfo.InvariantCulture) ?? string.Empty, autoFormatType);
	}

	private static ClassDeclarationSyntax EmitEvaluateCaptionClassMethodDecl(ClassDeclarationSyntax classDecl, SwitchSectionSyntax[] sections, bool isExtensionEmit)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrStringType, "EvaluateCaptionClass").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.FieldNoParameterIdentifier).WithType(CodeGenFacts.ClrInt32Type));
		if (isExtensionEmit)
		{
			methodDeclarationSyntax = methodDeclarationSyntax.AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("handled")).WithType(CodeGenFacts.ClrBoolType).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OutKeyword))).AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("handled"), CodeGenFacts.TrueLiteralExpression)));
		}
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("EnsureGlobalVariablesInitialized")))));
		if (!isExtensionEmit)
		{
			methodDeclarationSyntax = EmitCallExtensionMethod(methodDeclarationSyntax, "CallEvaluateCaptionClassExtensionMethod");
		}
		if (sections.Length != 0)
		{
			methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(CodeGenFacts.FieldNoParameterIdentifier)).AddSections(sections));
		}
		if (isExtensionEmit)
		{
			methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("handled"), CodeGenFacts.FalseLiteralExpression)));
		}
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ClrStringType, CodeGenFacts.EmptyPropertyNameSyntax)));
		return classDecl.AddMembers(methodDeclarationSyntax);
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private static MethodDeclarationSyntax EmitOnInvokeMethod(ObjectTypeSymbol symbol, int objectId)
	{
		ExpressionSyntax expressionSyntax = CodeGenHelper.MemberAccessExpression("args", "Length");
		ArrayBuilder<SwitchSectionSyntax> instance = ArrayBuilder<SwitchSectionSyntax>.GetInstance();
		foreach (Symbol item in from m in symbol.GetMembers()
			where m.Kind == SymbolKind.Method
			select m)
		{
			MethodSymbol methodSymbol = item as MethodSymbol;
			if (!(methodSymbol == null) && (methodSymbol.IsEventSubscriberTrigger() || (!methodSymbol.IsTrigger() && (!methodSymbol.IsLocal || methodSymbol.IsEventSubscriber()) && methodSymbol.MethodKind != MethodKind.BuiltInMethod)))
			{
				instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CaseSwitchLabel(CodeGenHelper.LiteralExpression(methodSymbol.Id))).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NotEqualsExpression, expressionSyntax, CodeGenHelper.LiteralExpression(methodSymbol.ParameterCount)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.NavRuntimeHelpersNameSyntax, CodeGenFacts.NavRuntimeHelpersCompilationErrorNameMethodSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavNCLInvalidNumberOfArgumentsExceptionType).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(methodSymbol.MetadataName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(methodSymbol.ParameterCount)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expressionSyntax)))))), EmitOnInvokeMethodCall(methodSymbol), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BreakStatement()));
			}
		}
		if (instance.Count == 0)
		{
			instance.Free();
			return null;
		}
		instance.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchSection().AddLabels(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.DefaultSwitchLabel()).AddStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.NavRuntimeHelpersNameSyntax, CodeGenFacts.NavRuntimeHelpersCompilationErrorNameMethodSyntax)).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.MemberAccessExpression(CodeGenFacts.LangClassNameSyntax, CodeGenFacts.LangWrongReferenceNameSyntax)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenFacts.OnInvokeMemberIdParameterNameSyntax), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(objectId)))), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BreakStatement()));
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrObjectType, CodeGenFacts.OnInvokeMethodIdentifier).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.OnInvokeMemberIdParameterIdentifier).WithType(CodeGenFacts.ClrInt32Type), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(CodeGenFacts.OnInvokeArgsParameterIdentifier).WithType(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayType(CodeGenFacts.ClrObjectType).AddRankSpecifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArrayRankSpecifier().AddSizes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.OmittedArraySizeExpression()))))
			.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SwitchStatement(CodeGenFacts.OnInvokeMemberIdParameterNameSyntax).AddSections(instance.ToArrayAndFree()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression)));
	}

	private static StatementSyntax EmitOnInvokeMethodCall(MethodSymbol method)
	{
		InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(method.MetadataName));
		if (method.HasParameters())
		{
			ArrayBuilder<ArgumentSyntax> instance = ArrayBuilder<ArgumentSyntax>.GetInstance();
			for (int i = 0; i < method.ParameterCount; i++)
			{
				instance.Add(EmitOnInvokeMethodCallArgument(i, method.Parameters[i]));
			}
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(instance.ToArrayAndFree());
		}
		if (!method.HasReturnValue())
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(invocationExpressionSyntax);
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(invocationExpressionSyntax);
	}

	[SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
	private static ArgumentSyntax EmitOnInvokeMethodCallArgument(int index, ParameterSymbol parameter)
	{
		ElementAccessExpressionSyntax expression = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ElementAccessExpression(CodeGenFacts.OnInvokeArgsParameterNameSyntax).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(index)));
		TypeSyntax typeSyntax = null;
		if (parameter.Type.IsArray())
		{
			typeSyntax = ((Symbol)parameter).GetRuntimeTypeSyntax();
		}
		else if (parameter.ShouldBePassedByRef())
		{
			typeSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName(CodeGenFacts.ByRefTypeIdentifier).AddTypeArgumentListArguments(((Symbol)parameter).GetRuntimeTypeSyntax());
		}
		if (typeSyntax != null)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(typeSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ALCompilerClassNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("SafeCastCheck").AddTypeArgumentListArguments(typeSyntax))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression))));
		}
		if (parameter.Type.NavTypeKind.IsNavValueType())
		{
			bool flag = false;
			SimpleNameSyntax name;
			switch (parameter.Type.NavTypeKind)
			{
			case NavTypeKind.Record:
				name = CodeGenFacts.ALCompilerObjectToExactINavRecordHandleMethodNameSyntax;
				break;
			case NavTypeKind.Codeunit:
				name = CodeGenFacts.ALCompilerObjectToExactNavCodeunitHandleMethodNameSyntax;
				break;
			case NavTypeKind.InStream:
				name = CodeGenFacts.ALCompilerObjectToNavInStreamMethodNameSyntax;
				flag = true;
				break;
			case NavTypeKind.OutStream:
				name = CodeGenFacts.AlCompilerObjectToNavOutStreamMethodNameSyntax;
				flag = true;
				break;
			case NavTypeKind.Option:
				name = CodeGenFacts.AlCompilerObjectToIntegerMethodNameSyntax;
				break;
			default:
				name = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.GenericName("ObjectToExactNavValue").AddTypeArgumentListArguments(((Symbol)parameter).GetRuntimeTypeSyntax());
				break;
			}
			InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.ALCompilerClassNameSyntax, name));
			if (flag)
			{
				invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()));
			}
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(invocationExpressionSyntax);
		}
		InvocationExpressionSyntax objectConversionInvocation = parameter.GetObjectConversionInvocation();
		if (objectConversionInvocation != null)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(((Symbol)parameter).GetRuntimeTypeSyntax(), objectConversionInvocation.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression))));
		}
		throw ExceptionUtilities.UnreachableWithMessage(string.Format(CultureInfo.InvariantCulture, "Parameter of parameter type {0} and type {1}", parameter.ParameterType.GetNavTypeKindSafe(), parameter.Type.GetNavTypeKindSafe()));
	}

	private static MemberDeclarationSyntax[] EmitGlobalFieldDeclarations(ImmutableArray<VariableSymbol> globalVariables, bool nonDebuggableEmit)
	{
		ArrayBuilder<MemberDeclarationSyntax> instance = ArrayBuilder<MemberDeclarationSyntax>.GetInstance();
		foreach (VariableSymbol item in globalVariables.Where((VariableSymbol g) => !g.IsImplicitlyDeclared))
		{
			TypeSyntax type = CodeGenerator.EmitVariableType(item, asGlobal: true);
			FieldDeclarationSyntax fieldDeclarationSyntax = CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(GetFieldName(item)), type, CodeGenerator.EmitVariableFieldInitializer(item), CodeGenerator.EmitFieldDeclarationModifiers(item.Type)).AddAttributeLists(CodeGenHelper.NavNameAttribute(item.Name));
			if (nonDebuggableEmit || item.IsNonDebuggable)
			{
				fieldDeclarationSyntax = fieldDeclarationSyntax.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(CodeGenFacts.DebuggerBrowsableNeverAttribute));
			}
			instance.Add(fieldDeclarationSyntax);
		}
		return instance.ToArrayAndFree();
	}

	internal static string GetFieldName(VariableSymbol globalVariable)
	{
		if (globalVariable.GetNavTypeKind() == NavTypeKind.Dialog)
		{
			return "\\u03b4" + globalVariable.MetadataName;
		}
		return globalVariable.MetadataName;
	}

	private static MethodDeclarationSyntax EmitOnClearMethod(ImmutableArray<VariableSymbol> globalVariables, Microsoft.CodeAnalysis.CSharp.SyntaxKind onClearAccessModifier)
	{
		ArrayBuilder<StatementSyntax> instance = ArrayBuilder<StatementSyntax>.GetInstance();
		foreach (VariableSymbol item in globalVariables)
		{
			if (!item.IsImplicitlyDeclared && item.Type.NavTypeKind != NavTypeKind.TextConst && item.Type.NavTypeKind != NavTypeKind.Label)
			{
				instance.Add(CodeGenerator.EmitVariableClearStatement(item));
			}
		}
		if (instance.Count == 0)
		{
			instance.Free();
			return null;
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, CodeGenFacts.OnClearMethodIdentifier).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(onClearAccessModifier), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddBodyStatements(instance.ToArrayAndFree());
	}
}
