using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class FilterPageBuilderClassTypeSymbol : LanguageClassTypeSymbol
{
	internal class AddFieldBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			DebugAssertHelper.Assert(call.Method as BuiltInMethodTypeSymbol != null);
			InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(call.ReceiverOpt), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName)), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ArgumentList(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.SeparatedList(new ArgumentSyntax[3]
			{
				Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(call.IsStatement ? CodeGenFacts.DataErrorThrowErrorExpression : CodeGenFacts.DataErrorTrapErrorExpression),
				generator.EmitArgument(call.Method.Parameters[0], call.Arguments[0]),
				Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(call.Arguments[1]), CodeGenFacts.NavFieldRefNumberPropertyNameSyntax))
			})));
			if (call.Arguments.Length == 3)
			{
				invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(generator.EmitArgument(call.Method.Parameters[2], call.Arguments[2]));
			}
			return invocationExpressionSyntax;
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public AddFieldBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<FilterPageBuilderClassTypeSymbol> instance = new Lazy<FilterPageBuilderClassTypeSymbol>(() => new FilterPageBuilderClassTypeSymbol("FilterPageBuilder", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[12]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "AddTable", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "TableNo", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddRecordRef", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Field", 1, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Filter", 2, isVar: false, isOptional: true)
		}),
		new AddFieldBuiltInMethodTypeSymbol(containingSymbol, "AddField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.FieldRefType, "Field", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Filter", 2, isVar: false, isOptional: true)
		}, VersionCompatibility.Parse("2.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "AddFieldNo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALAddField", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "FieldNo", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Filter", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetView", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "View", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetView", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UseNames", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RunModal", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Count", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Name", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "PageCaption", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "PageCaption", 0, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static FilterPageBuilderClassTypeSymbol Instance => instance.Value;

	private FilterPageBuilderClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
