using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class BigTextClassTypeSymbol : LanguageClassTypeSymbol
{
	internal class AddTextBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			ExpressionSyntax expression = generator.EmitExpression(call.ReceiverOpt);
			InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.NavBigTextNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression));
			invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(call.Arguments.Select((BoundExpression arg) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(arg))).ToArray());
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, generator.EmitExpression(call.ReceiverOpt), invocationExpressionSyntax);
		}

		public AddTextBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class ReadBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			ExpressionSyntax expression = generator.EmitByRefExpression(NavCorLib.BigTextType, call.ReceiverOpt);
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(CodeGenFacts.NavBigTextNameSyntax, Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName))).AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(expression)).AddArgumentListArguments(call.Arguments.Select((BoundExpression arg) => Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(generator.EmitExpression(arg))).ToArray());
		}

		public ReadBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<BigTextClassTypeSymbol> instance = new Lazy<BigTextClassTypeSymbol>(() => new BigTextClassTypeSymbol("BigText", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[8]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Length", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new AddTextBuiltInMethodTypeSymbol(containingSymbol, "AddText", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavBigText", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 1, isVar: false, isOptional: true)
		}),
		new AddTextBuiltInMethodTypeSymbol(containingSymbol, "AddText", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavBigText", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BigTextType, "String", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetSubText", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetSubText", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BigTextType, "Variable", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 1),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Length", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TextPos", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "String", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 0)
		}),
		new ReadBuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.DefinedByOwner, isStatic: false, "NavBigText", null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0)
		})
	}), isThreadSafe: true);

	internal static BigTextClassTypeSymbol Instance => instance.Value;

	private BigTextClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
