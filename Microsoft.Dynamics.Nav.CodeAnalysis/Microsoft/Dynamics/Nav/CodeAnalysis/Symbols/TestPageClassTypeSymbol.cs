using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;
using Microsoft.Dynamics.Nav.CodeAnalysis.Utilities;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class TestPageClassTypeSymbol : LanguageClassTypeSymbol
{
	internal class OKBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return EmitGetBuiltInActionInvoke(generator, call);
		}

		public OKBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class CancelBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return EmitGetBuiltInActionInvoke(generator, call);
		}

		public CancelBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class YesBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return EmitGetBuiltInActionInvoke(generator, call);
		}

		public YesBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class NoBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return EmitGetBuiltInActionInvoke(generator, call);
		}

		public NoBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<TestPageClassTypeSymbol> instance = new Lazy<TestPageClassTypeSymbol>(() => new TestPageClassTypeSymbol("TestPage", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[29]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "OpenNew", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "OpenEdit", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "OpenView", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Close", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "New", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "GetField", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.TestFieldType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Id", 0)
		}, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "ValidationErrorCount", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetValidationError", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Index", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Caption", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Trap", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Next", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Previous", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Prev", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ALPrevious", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, VersionCompatibility.Parse("3.0")),
		new BuiltInMethodTypeSymbol(containingSymbol, "First", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Last", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsExpanded", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Expand", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Expand", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GoToRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Rec", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GoToKey", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 0, isVar: false, isOptional: true, isParams: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FindFirstField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFieldType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FindNextField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFieldType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "FindPreviousField", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TestFieldType, "Field", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: true, typeFromFirstArgument: false, isArray: false, isSameVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Editable", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "View", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TestActionType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Edit", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TestActionType),
		new OKBuiltInMethodTypeSymbol(containingSymbol, "OK", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TestActionType),
		new CancelBuiltInMethodTypeSymbol(containingSymbol, "Cancel", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TestActionType),
		new YesBuiltInMethodTypeSymbol(containingSymbol, "Yes", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TestActionType),
		new NoBuiltInMethodTypeSymbol(containingSymbol, "No", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.TestActionType)
	}), isThreadSafe: true);

	internal static TestPageClassTypeSymbol Instance => instance.Value;

	internal static ExpressionSyntax EmitGetBuiltInActionInvoke(CodeGenerator generator, BoundCall call)
	{
		DebugAssertHelper.Assert(call.Method as BuiltInMethodTypeSymbol != null);
		InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(generator.EmitExpression(call.ReceiverOpt), CodeGenFacts.GetBuiltInActionMethodNameSyntax));
		int value = call.Method.Name switch
		{
			"OK" => 1, 
			"Cancel" => 2, 
			"Yes" => 5, 
			"No" => 6, 
			"Print" => 11, 
			"Preview" => 12, 
			"Schedule" => 20, 
			_ => throw ExceptionUtilities.Unreachable, 
		};
		return invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.CastExpression(CodeGenFacts.FormResultType, CodeGenHelper.LiteralExpression(value))));
	}

	private TestPageClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Extension, getMethods)
	{
	}
}
