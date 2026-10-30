using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class XmlPortCodeBuilder : ObjectCodeBuilder<SourceXmlPortTypeSymbol>
{
	private static readonly ParameterSyntax[] ConstructorParameters = new ParameterSyntax[1] { Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("metadata")).WithType(CodeGenFacts.NCLMetaXmlPortType) };

	private static readonly ArgumentSyntax[] ConstructorArguments = ConstructorParameters.Select((ParameterSyntax p) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(p.Identifier))).ToArray();

	private MethodDeclarationSyntax initializeComponentMethod;

	protected override TypeSyntax BaseClassType { get; } = CodeGenFacts.NavXmlPortType;

	protected override TypeSyntax ClassType { get; }

	public XmlPortCodeBuilder(SourceXmlPortTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		ClassType = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(moduleBuilder.Options);
	}

	public override NamespaceDeclarationSyntax CreateNamespaceDeclaration()
	{
		return ApplicationObjectEmitHelper.CreateApplicationObjectNamespaceDeclaration();
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		initializeComponentMethod = InitializeComponent();
		bool hasConstructorBodyInitializeComponentCall = initializeComponentMethod != null;
		ConstructorDeclarationSyntax constructorDeclarationSyntax = ApplicationObjectEmitHelper.CreateConstructorDeclaration(ClassType.ToString(), Symbol.Id, hasConstructorBodyInitializeComponentCall, ConstructorParameters, ConstructorArguments, (ArgumentListSyntax syntax) => syntax);
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

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddProperties(classDeclaration);
		return classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(ClassType, "currXMLport").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(ClassType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
	}

	public override ClassDeclarationSyntax AddEvaluateCaptionClass(ClassDeclarationSyntax classDeclaration)
	{
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol);
	}

	public override ClassDeclarationSyntax AddFrameworkMethods(ClassDeclarationSyntax classDeclaration)
	{
		ArrayBuilder<StatementSyntax> instance = ArrayBuilder<StatementSyntax>.GetInstance();
		PooledDictionary<int, Tuple<int, ExpressionSyntax>> instance2 = PooledDictionary<int, Tuple<int, ExpressionSyntax>>.GetInstance();
		PooledDictionary<int, string> instance3 = PooledDictionary<int, string>.GetInstance();
		try
		{
			classDeclaration = EmitGlobalTextVariables(classDeclaration);
			classDeclaration = EmitGetTextVariableValue(classDeclaration);
			classDeclaration = EmitSetTextVariableValue(classDeclaration);
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

	public override ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(ClassType.ToString(), ConstructorParameters, ConstructorArguments);
		classDeclaration = classDeclaration.AddMembers(methodDeclarationSyntax);
		return classDeclaration;
	}

	internal ClassDeclarationSyntax AddRequestPageInnerClass(ClassDeclarationSyntax classDecl)
	{
		ICodeEmitter<MemberDeclarationSyntax> emitter = Symbol.RequestPage.GetEmitter(ModuleBuilder);
		return classDecl.AddMembers(emitter.Emit());
	}

	internal static string GetNodeLocalVariableName(int symbolId)
	{
		return "temp_" + symbolId.ToString(CultureInfo.InvariantCulture).MangleUnquotedIdentifierName();
	}

	internal static string GeneratedGuid(XmlPortNodeSymbol node)
	{
		XmlPortTypeSymbol containingSymbolOfKind = node.GetContainingSymbolOfKind<XmlPortTypeSymbol>(SymbolKind.XmlPort);
		return MetadataEmitterHelper.GenerateMemberId(node.Id, containingSymbolOfKind.Id, containingSymbolOfKind.Kind).ToUpperInvariant();
	}

	private MethodDeclarationSyntax InitializeComponent()
	{
		ArrayBuilder<StatementSyntax> builder = ArrayBuilder<StatementSyntax>.GetInstance();
		StatementSyntax[] items;
		try
		{
			builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("BeginInitialization"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList())));
			MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(GlobalVariables, ModuleBuilder.Options);
			if (methodDeclarationSyntax != null)
			{
				builder.AddRange(methodDeclarationSyntax.Body.Statements);
			}
			Symbol.FlattenedNodes.Do((XmlPortNodeSymbol node) =>
			{
				switch (node.SourceTypeKind)
				{
				case XmlPortSourceTypeKind.Table:
					InitializeTableNode(node, builder);
					break;
				case XmlPortSourceTypeKind.Field:
					InitializeFieldNode(node, builder);
					break;
				case XmlPortSourceTypeKind.Text:
					InitializeTextNode(node, builder);
					break;
				}
			});
			builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("RequestOptionsPage"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.RequestPageType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Metadata.RequestFormMetadata"))), null))));
			builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("EndInitialization"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList())));
		}
		finally
		{
			items = builder.ToArrayAndFree();
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, CodeGenFacts.InitializeComponentMethodIdentifier).AddBodyStatements(items);
	}

	private ClassDeclarationSyntax EmitGlobalTextVariables(ClassDeclarationSyntax classDecl)
	{
		GetNodeSymbols(XmlPortSourceTypeKind.Text).Do((XmlPortNodeSymbol member) =>
		{
			classDecl = EmitTextElement(classDecl, member);
		});
		GetNodeSymbols(XmlPortSourceTypeKind.Table).Do((XmlPortNodeSymbol member) =>
		{
			FieldDeclarationSyntax fieldDeclarationSyntax = CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(member.MemberName), CodeGenFacts.NavRecordHandleType);
			if (member.MemberName != member.Name)
			{
				fieldDeclarationSyntax = fieldDeclarationSyntax.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavName")).WithArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(member.Name))))));
			}
			classDecl = classDecl.AddMembers(fieldDeclarationSyntax);
		});
		return classDecl;
	}

	private static ClassDeclarationSyntax EmitTextElement(ClassDeclarationSyntax classDecl, XmlPortNodeSymbol member)
	{
		TextTypeKind? enumPropertyValue = member.GetEnumPropertyValue<TextTypeKind>(PropertyKind.TextType);
		FieldDeclarationSyntax fieldDeclarationSyntax = enumPropertyValue switch
		{
			TextTypeKind.BigText => CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(member.MetadataName), CodeGenFacts.NavBigTextType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavBigTextType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(), null)), 
			TextTypeKind.Text => CodeGenHelper.FieldDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(member.MetadataName), CodeGenFacts.NavTextType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavText"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Default")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(1024))))), 
			_ => throw ExceptionUtilities.UnexpectedValue(enumPropertyValue), 
		};
		if (member.MetadataName != member.Name)
		{
			fieldDeclarationSyntax = fieldDeclarationSyntax.AddAttributeLists(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeList().AddAttributes(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Attribute(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("NavName")).WithArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AttributeArgument(CodeGenHelper.LiteralExpression(member.Name))))));
		}
		classDecl = classDecl.AddMembers(fieldDeclarationSyntax);
		return classDecl;
	}

	private ClassDeclarationSyntax EmitSetTextVariableValue(ClassDeclarationSyntax classDecl)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, "SetTextVariableValue").AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("nodeName")).WithType(CodeGenFacts.ClrStringType), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("value")).WithType(CodeGenFacts.ClrStringType));
		methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword));
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.ClrStringType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("@tmp1")).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("nodeName"))))));
		IEnumerable<XmlPortNodeSymbol> nodeSymbols = GetNodeSymbols(XmlPortSourceTypeKind.Text);
		if (!nodeSymbols.Any())
		{
			return classDecl.AddMembers(methodDeclarationSyntax);
		}
		IEnumerable<IfStatementSyntax> source = nodeSymbols.Select(GetIfStatementForSetTextMember);
		IfStatementSyntax ifStatementSyntax = source.First();
		foreach (IfStatementSyntax item in source.Skip(1))
		{
			ifStatementSyntax = item.WithElse(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ElseClause(ifStatementSyntax));
		}
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(ifStatementSyntax);
		return classDecl.AddMembers(methodDeclarationSyntax);
	}

	private ClassDeclarationSyntax EmitGetTextVariableValue(ClassDeclarationSyntax classDecl)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrObjectType, "GetTextVariableValue").AddParameterListParameters(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("nodeName")).WithType(CodeGenFacts.ClrStringType));
		methodDeclarationSyntax = methodDeclarationSyntax.AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ProtectedKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword));
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.ClrStringType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("@tmp0")).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("nodeName"))))));
		IEnumerable<XmlPortNodeSymbol> nodeSymbols = GetNodeSymbols(XmlPortSourceTypeKind.Text);
		if (!nodeSymbols.IsEmpty())
		{
			IEnumerable<IfStatementSyntax> source = nodeSymbols.Select(GetIfStatementForGetTextMember);
			IfStatementSyntax ifStatementSyntax = source.First();
			foreach (IfStatementSyntax item in source.Skip(1))
			{
				ifStatementSyntax = item.WithElse(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ElseClause(ifStatementSyntax));
			}
			methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(ifStatementSyntax);
		}
		methodDeclarationSyntax = methodDeclarationSyntax.AddBodyStatements(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LiteralExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NullLiteralExpression)));
		return classDecl.AddMembers(methodDeclarationSyntax);
	}

	private IEnumerable<XmlPortNodeSymbol> GetNodeSymbols(XmlPortSourceTypeKind kind)
	{
		return Symbol.FlattenedNodes.Where((XmlPortNodeSymbol n) => n.SourceTypeKind == kind);
	}

	private IfStatementSyntax GetIfStatementForGetTextMember(XmlPortNodeSymbol arg)
	{
		string value = GeneratedGuid(arg);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("@tmp0"), CodeGenHelper.LiteralExpression(value)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(arg.MetadataName)));
	}

	private IfStatementSyntax GetIfStatementForSetTextMember(XmlPortNodeSymbol member)
	{
		TextTypeKind? enumPropertyValue = member.GetEnumPropertyValue<TextTypeKind>(PropertyKind.TextType);
		ObjectCreationExpressionSyntax right = enumPropertyValue switch
		{
			TextTypeKind.BigText => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavBigTextType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("value"))), null), 
			TextTypeKind.Text => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavTextType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(1024)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("value"))), null), 
			_ => throw ExceptionUtilities.UnexpectedValue(enumPropertyValue), 
		};
		string value = GeneratedGuid(member);
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IfStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BinaryExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.EqualsExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("@tmp1"), CodeGenHelper.LiteralExpression(value)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(member.MetadataName), right)));
	}

	private static void InitializeFieldNode(XmlPortNodeSymbol symbol, ArrayBuilder<StatementSyntax> builder)
	{
		string nodeLocalVariableName = GetNodeLocalVariableName(symbol.Id);
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.NavXmlPortFieldNodeType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(nodeLocalVariableName)).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavXmlPortFieldNodeType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(), null))))));
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Add"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(nodeLocalVariableName))))));
		EmitInitializeAddTriggers(symbol, builder, nodeLocalVariableName);
	}

	private static void InitializeTableNode(XmlPortNodeSymbol symbol, ArrayBuilder<StatementSyntax> builder)
	{
		string nodeLocalVariableName = GetNodeLocalVariableName(symbol.Id);
		LiteralExpressionSyntax expression = (((bool?)symbol.GetProperty(PropertyKind.UseTemporary)?.Value == true) ? CodeGenFacts.TrueLiteralExpression : CodeGenFacts.FalseLiteralExpression);
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(symbol.MemberName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavRecordHandleType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression()), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.LiteralExpression(((RecordTypeSymbol)symbol.Type).Id)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("SecurityFiltering.Filtered"))), null))));
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.NavXmlPortTableNodeType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(nodeLocalVariableName)).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavXmlPortTableNodeType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(symbol.MemberName))), null))))));
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Add"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(nodeLocalVariableName))))));
		EmitInitializeAddTriggers(symbol, builder, nodeLocalVariableName);
	}

	private static void InitializeTextNode(XmlPortNodeSymbol symbol, ArrayBuilder<StatementSyntax> builder)
	{
		string nodeLocalVariableName = GetNodeLocalVariableName(symbol.Id);
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.LocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclaration(CodeGenFacts.NavXmlPortTextNodeType).AddVariables(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.VariableDeclarator(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier(nodeLocalVariableName)).WithInitializer(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.EqualsValueClause(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavXmlPortTextNodeType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(), null))))));
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Add"), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(nodeLocalVariableName))))));
		EmitInitializeAddTriggers(symbol, builder, nodeLocalVariableName);
	}

	private static void EmitInitializeAddTriggers(XmlPortNodeSymbol symbol, ArrayBuilder<StatementSyntax> builder, string metadataName = null)
	{
		symbol.Triggers.Do((MethodSymbol t) =>
		{
			builder.Add(EmitInitializeAddTrigger(symbol, t, metadataName));
		});
	}

	private static StatementSyntax EmitInitializeAddTrigger(XmlPortNodeSymbol symbol, MethodSymbol methodSymbol, string metadataName = null)
	{
		string name = methodSymbol.Name;
		metadataName = metadataName ?? symbol.MetadataName;
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.AddAssignmentExpression, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(metadataName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(name)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ObjectCreationExpression(CodeGenFacts.NavTriggerType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList().AddArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(methodSymbol.MetadataName))), null)));
	}
}
