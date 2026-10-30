using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

internal class PageCodeBuilder : PageBaseCodeBuilder<SourcePageTypeSymbol>
{
	private static readonly ParameterSyntax[] ConstructorParameters = new ParameterSyntax[1] { Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Parameter(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Identifier("record")).WithType(CodeGenFacts.NavRecordType) };

	private static readonly ArgumentSyntax[] ConstructorArguments = ConstructorParameters.Select((ParameterSyntax p) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(p.Identifier))).ToArray();

	private MemberDeclarationSyntax initializeComponentMethod;

	protected override MemberDeclarationSyntax InitializeComponentMethod => initializeComponentMethod;

	protected override bool HasRelatedTable => Symbol.RelatedTable != null;

	protected override TypeSyntax BaseClassType { get; } = CodeGenFacts.NavPageType;

	protected override TypeSyntax ClassType { get; }

	private string PageName { get; }

	protected override ImmutableArray<ControlSymbol> FlattenedControls => Symbol.FlattenedControls;

	public PageCodeBuilder(SourcePageTypeSymbol symbol, ModuleBuilder moduleBuilder)
		: base(symbol, moduleBuilder)
	{
		PageName = (ClassType = Symbol.GetApplicationObjectTypeWithSuffixTypeSyntax(ModuleBuilder.Options)).ToString();
	}

	public override NamespaceDeclarationSyntax CreateNamespaceDeclaration()
	{
		return ApplicationObjectEmitHelper.CreateApplicationObjectNamespaceDeclaration();
	}

	public override ClassDeclarationSyntax AddConstructor(ClassDeclarationSyntax classDeclaration)
	{
		initializeComponentMethod = ApplicationObjectEmitHelper.EmitInitializeComponentMethod(Symbol.GlobalVariables, ModuleBuilder.Options, AppendToInitializeComponent);
		bool hasConstructorBodyInitializeComponentCall = InitializeComponentMethod != null;
		ConstructorDeclarationSyntax constructorDeclarationSyntax = ApplicationObjectEmitHelper.CreateConstructorDeclaration(PageName, Symbol.Id, hasConstructorBodyInitializeComponentCall, null, null, (ArgumentListSyntax syntax) => syntax);
		classDeclaration = classDeclaration.AddMembers(constructorDeclarationSyntax);
		ConstructorDeclarationSyntax constructorDeclarationSyntax2 = ApplicationObjectEmitHelper.CreateConstructorDeclaration(PageName, Symbol.Id, hasConstructorBodyInitializeComponentCall, ConstructorParameters, ConstructorArguments, (ArgumentListSyntax syntax) => syntax);
		classDeclaration = classDeclaration.AddMembers(constructorDeclarationSyntax2);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddProperties(ClassDeclarationSyntax classDeclaration)
	{
		classDeclaration = base.AddProperties(classDeclaration);
		classDeclaration = PageEmitHelper.AddRecAndCurrPage(classDeclaration, HasRelatedTable, ClassType);
		classDeclaration = PageEmitHelper.AddUserControlProperties(classDeclaration, FlattenedControls);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddConstructMethod(ClassDeclarationSyntax classDeclaration)
	{
		MethodDeclarationSyntax methodDeclarationSyntax = ApplicationObjectEmitHelper.EmitConstructMethod(PageName, (ParameterSyntax[])null, (ArgumentSyntax[])null);
		classDeclaration = classDeclaration.AddMembers(methodDeclarationSyntax);
		MethodDeclarationSyntax methodDeclarationSyntax2 = ApplicationObjectEmitHelper.EmitConstructMethod(PageName, ConstructorParameters, ConstructorArguments);
		classDeclaration = classDeclaration.AddMembers(methodDeclarationSyntax2);
		return classDeclaration;
	}

	public override ClassDeclarationSyntax AddOnInvoke(ClassDeclarationSyntax classDeclaration)
	{
		return ApplicationObjectEmitHelper.AppendOnInvokeMethod(classDeclaration, Symbol);
	}

	protected override void AppendToInitializeComponent(ArrayBuilder<StatementSyntax> builder)
	{
		base.AppendToInitializeComponent(builder);
		builder.Add(CodeGenFacts.InvokeCallInitializeComponentExtensionMethodStatement);
		builder.Add(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ExpressionStatement(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ThisExpression(), CodeGenFacts.InitializeFormMethodNameSyntax))));
		if (HasTemporaryBookmark())
		{
			builder.Add(CodeGenFacts.BookmarkEqualsBookmarkTemporary);
		}
	}

	protected bool HasTemporaryBookmark()
	{
		bool flag = false;
		bool flag2 = false;
		for (int i = 0; i < Symbol.Triggers.Length; i++)
		{
			TriggerTypeInfo triggerInfo = ((SourceTriggerSymbol)Symbol.Triggers[i]).TriggerInfo;
			if (triggerInfo != null)
			{
				switch (triggerInfo.Kind)
				{
				case TriggerTypeKind.PageOnFindRecord:
					flag = true;
					break;
				case TriggerTypeKind.PageOnNextRecord:
					flag2 = true;
					break;
				}
			}
		}
		return flag & flag2;
	}
}
