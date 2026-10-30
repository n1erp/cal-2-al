using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class FileClassTypeSymbol : LanguageClassTypeSymbol
{
	private static class FileCallEmitter
	{
		public static ExpressionSyntax EmitCallWithAutomationAndParams(CodeGenerator generator, BoundCall call)
		{
			BuiltInMethodTypeSymbol obj = call.Method as BuiltInMethodTypeSymbol;
			DebugAssertHelper.Assert(obj != null);
			InvocationExpressionSyntax invocationExpressionSyntax = Microsoft.CodeAnalysis.CSharp.SyntaxFactory.InvocationExpression(CodeGenHelper.MemberAccessExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(obj.StaticClassName), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName(call.Method.MetadataName)));
			if (obj.AddDataError)
			{
				invocationExpressionSyntax = invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(call.IsStatement ? CodeGenFacts.DataErrorThrowErrorExpression : CodeGenFacts.DataErrorTrapErrorExpression));
			}
			invocationExpressionSyntax = generator.EmitArguments(invocationExpressionSyntax, call);
			return invocationExpressionSyntax.AddArgumentListArguments(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.Argument(CodeGenerator.CreateAutomationId(call)));
		}
	}

	internal class DownloadStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return FileCallEmitter.EmitCallWithAutomationAndParams(generator, call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public DownloadStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class DownloadFromStreamStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return FileCallEmitter.EmitCallWithAutomationAndParams(generator, call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public DownloadFromStreamStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class UploadStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return FileCallEmitter.EmitCallWithAutomationAndParams(generator, call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public UploadStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class UploadIntoStreamStaticBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return FileCallEmitter.EmitCallWithAutomationAndParams(generator, call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public UploadIntoStreamStaticBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<FileClassTypeSymbol> instance = new Lazy<FileClassTypeSymbol>(() => new FileClassTypeSymbol("File", isAutoWith: true, (Symbol containingSymbol) => new MethodSymbol[46]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Erase", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Rename", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "OldName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "NewName", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Copy", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FromName", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "ToName", 1)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "GetStamp", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 1, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.TimeType, "Time", 2, isVar: true, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetStamp", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Date", 1),
			new BuiltInParameterSymbol(method, NavCorLib.TimeType, "Time", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Exists", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}),
		new UploadIntoStreamStaticBuiltInMethodTypeSymbol(containingSymbol, "UploadIntoStream", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[5]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "DialogTitle", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FromFolder", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FromFilter", 2),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "FromFile", 3, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 4, isVar: true)
		}),
		new DownloadFromStreamStaticBuiltInMethodTypeSymbol(containingSymbol, "DownloadFromStream", SymbolCompilationScope.DefinedByOwner, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[5]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "DialogTitle", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "ToFolder", 2),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "ToFilter", 3),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "ToFile", 4, isVar: true)
		}),
		new UploadStaticBuiltInMethodTypeSymbol(containingSymbol, "Upload", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[5]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "DialogTitle", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FromFolder", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FromFilter", 2),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FromFile", 3),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "ToFile", 4, isVar: true)
		}),
		new DownloadStaticBuiltInMethodTypeSymbol(containingSymbol, "Download", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[5]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FromFile", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "DialogTitle", 1),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "ToFolder", 2),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "ToFilter", 3),
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "ToFile", 4, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Open", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextEncodingType, "Encoding", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Create", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0),
			new BuiltInParameterSymbol(method, NavCorLib.TextEncodingType, "Encoding", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Close", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Name", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Pos", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Len", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "Read", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Read", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.ByteType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.CharType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BigIntegerType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.GuidType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.CodeType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.LabelType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BigTextType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TimeType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateTimeType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DateFormulaType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DurationType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OptionType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.RecordIdType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", "ALWriteString", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Write", SymbolCompilationScope.Internal, isStatic: false, "ALStream", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.JokerType, "Value", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Seek", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Position", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Trunc", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "WriteMode", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Mode", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TextMode", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Mode", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CreateInStream", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: true, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CreateOutStream", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: true, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "CreateTempFile", SymbolCompilationScope.Internal, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TextEncodingType, "Encoding", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "IsPathTemporary", SymbolCompilationScope.Internal, isStatic: true, "NavFile", null, isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Name", 0)
		}, addCompilationTarget: false, VersionCompatibility.Parse("2.0"))
	}), isThreadSafe: true);

	internal static FileClassTypeSymbol Instance => instance.Value;

	private FileClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.Complex, NavTypeKind.None, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
