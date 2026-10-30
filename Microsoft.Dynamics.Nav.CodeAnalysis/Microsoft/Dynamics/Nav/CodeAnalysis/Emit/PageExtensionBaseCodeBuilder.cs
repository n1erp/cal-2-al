using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emitter;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal abstract class PageExtensionBaseCodeBuilder<T> : PageBaseCodeBuilder<T> where T : ApplicationObjectExtensionTypeSymbol
{
	private static readonly ParameterSyntax[] ConstructorParameters = new ParameterSyntax[1] { Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("record")).WithType(CodeGenFacts.NavRecordType) };

	private static readonly ArgumentSyntax[] ConstructorArguments = ConstructorParameters.Select((ParameterSyntax p) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(p.Identifier))).ToArray();

	private MemberDeclarationSyntax initializeComponentMethod;

	protected override MemberDeclarationSyntax InitializeComponentMethod => initializeComponentMethod;

	protected override bool HasRelatedTable => ((PageTypeSymbol)Symbol.Target).RelatedTable != null;

	protected override TypeSyntax BaseClassType { get; } = CodeGenFacts.NavPageExtensionType;

	protected override TypeSyntax ClassType { get; }

	protected override bool IsExtensionBuilder { get; } = true;

	protected override Microsoft.CodeAnalysis.CSharp.SyntaxKind OnClearMethodAccessModifier { get; } = Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword;

	public PageExtensionBaseCodeBuilder(T symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		ClassType = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(ModuleBuilder.Options);
	}

	public override NamespaceDeclarationSyntax CreateNamespaceDeclaration()
	{
		return ApplicationObjectEmitHelper.CreateApplicationObjectNamespaceDeclaration();
	}

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddProperties(classDeclaration);
		classDeclaration = PageEmitHelper.AddUserControlProperties(classDeclaration, FlattenedControls);
		TypeSyntax navPageType = CodeGenFacts.NavPageType;
		TypeSyntax navRecordType = CodeGenFacts.NavRecordType;
		return classDeclaration.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(navPageType, "CurrPage").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(navPageType, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ParentObject")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken))))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.NewKeyword)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(navRecordType, "Rec").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(navRecordType, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ParentObject")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("SourceTable")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken))))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.PropertyDeclaration(navRecordType, "xRec").AddAccessorListAccessors(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AccessorDeclaration(Microsoft.CodeAnalysis.CSharp.SyntaxKind.GetAccessorDeclaration, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Block(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ReturnStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(navRecordType, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleMemberAccessExpression, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.BaseExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("ParentObject")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("OldRecord")).WithOperatorToken(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.DotToken))))))).AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PrivateKeyword)));
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		initializeComponentMethod = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(Symbol.GlobalVariables, ModuleBuilder.Options, AppendToInitializeComponent);
		bool hasConstructorBodyInitializeComponentCall = InitializeComponentMethod != null;
		ConstructorDeclarationSyntax constructorDeclarationSyntax = ApplicationObjectEmitHelper.CreateConstructorDeclaration(ClassType.ToString(), Symbol.Id, hasConstructorBodyInitializeComponentCall, ConstructorParameters, ConstructorArguments, (ArgumentListSyntax syntax) => syntax);
		classDeclaration = classDeclaration.AddMembers(constructorDeclarationSyntax);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddFrameworkMethods(ClassDeclarationSyntax classDeclaration)
	{
		ArrayBuilder<StatementSyntax> instance = ArrayBuilder<StatementSyntax>.GetInstance();
		PooledDictionary<int, Tuple<int, ExpressionSyntax>> instance2 = PooledDictionary<int, Tuple<int, ExpressionSyntax>>.GetInstance();
		PooledDictionary<int, string> instance3 = PooledDictionary<int, string>.GetInstance();
		try
		{
			ProcessControls(instance, instance2, instance3);
			classDeclaration = EmitOnMetadataLoaded(classDeclaration, instance);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetAutoFormatStringForExtensions(classDeclaration, instance2);
			classDeclaration = ApplicationObjectEmitHelper.EmitGetDecimalPlacesForExtensions(classDeclaration, instance3);
			return classDeclaration;
		}
		finally
		{
			instance.Free();
			instance2.Free();
			instance3.Free();
		}
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol);
	}

	public override ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(ClassType.ToString(), ConstructorParameters, ConstructorArguments);
		return classDeclaration.AddMembers(methodDeclarationSyntax);
	}

	private static ClassDeclarationSyntax EmitOnMetadataLoaded(ClassDeclarationSyntax classDecl, ArrayBuilder<StatementSyntax> statements)
	{
		return classDecl.AddMembers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.MethodDeclaration(CodeGenFacts.ClrVoidType, "OnMetadataLoaded").AddModifiers(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.PublicKeyword), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Token(Microsoft.CodeAnalysis.CSharp.SyntaxKind.OverrideKeyword)).AddBodyStatements(statements.ToArray()));
	}

	[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
	private void ProcessControls(ArrayBuilder<StatementSyntax> expressions, PooledDictionary<int, Tuple<int, ExpressionSyntax>> autoFormatStrings, PooledDictionary<int, string> decimalPlaceString)
	{
		PooledHashSet<string> instance = PooledHashSet<string>.GetInstance();
		try
		{
			BoundCodeExpressionPropertyValue boundPropertyValue = Symbol.GetBoundPropertyValue<BoundCodeExpressionPropertyValue>(PropertyKind.DataCaptionExpression);
			if (boundPropertyValue != null)
			{
				expressions.Add(PageEmitHelper.CreateRegisterSourceExpressionSyntax("DataCaptionExprCode", "DataCaptionExprCode", boundPropertyValue.Expression, CodeGenFacts.SourceExpressionTypeDataCaptionExpression, ModuleBuilder.CodeGenerator, isWritable: false));
			}
			ImmutableHashSet<Symbol> implicitInDataset = InDataSetVisitor.GetAllImplicitInDatasetVariables(Symbol);
			foreach (VariableSymbol item in Symbol.GlobalVariables.Where((VariableSymbol g) => g.IsInDataSet || implicitInDataset.Contains(g)))
			{
				string prefixedExpressionName = PageMetadataEmitHelper.GetPrefixedExpressionName(item.Name, Symbol);
				expressions.Add(PageEmitHelper.CreateRegisterSourceExpressionSyntax(prefixedExpressionName, prefixedExpressionName, CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(item.MetadataName)), item.Type, isAssignable: true, CodeGenFacts.SourceExpressionTypeVariableIncludedInDataSetExpression));
			}
			foreach (ChangeSymbol item2 in Symbol.GetMembers().OfType<ChangeSymbol>())
			{
				if (item2.ChangeTargetKind != SymbolKind.Control)
				{
					continue;
				}
				if (item2.ChangeKind.IsAdd())
				{
					foreach (SourceControlSymbol item3 in ((SourceChangeAddControlSymbol)item2).AddedSymbols.Cast<SourceControlSymbol>())
					{
						PageEmitHelper.ProcessFieldControlsRecurse(item3, expressions, autoFormatStrings, decimalPlaceString, instance, ModuleBuilder.CodeGenerator);
					}
				}
				else if (item2.ChangeKind.IsModify())
				{
					SourceChangeModifyControlSymbol modifySymbol = (SourceChangeModifyControlSymbol)item2;
					WriteAdditionalRepeaterExpressionsIfAny(expressions, modifySymbol);
				}
			}
		}
		finally
		{
			instance.Free();
		}
	}

	private void WriteAdditionalRepeaterExpressionsIfAny(ArrayBuilder<StatementSyntax> expressions, SourceChangeModifySymbol modifySymbol)
	{
		if (modifySymbol.Target is ControlSymbol { ControlKind: ControlKind.Repeater })
		{
			StatementSyntax statementSyntax = PageEmitHelper.CreateIndentExpressionSyntax(modifySymbol, ModuleBuilder.CodeGenerator);
			if (statementSyntax != null)
			{
				expressions.Add(statementSyntax);
			}
		}
	}
}
