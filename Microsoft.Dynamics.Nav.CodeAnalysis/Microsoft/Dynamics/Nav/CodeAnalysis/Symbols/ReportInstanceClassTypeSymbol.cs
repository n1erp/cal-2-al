using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Dynamics.Nav.CodeAnalysis.Emit;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;

[SuppressMessage("Microsoft.Maintainability", "CA1501: AvoidExcessiveInheritance")]
[SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
internal sealed class ReportInstanceClassTypeSymbol : LanguageClassTypeSymbol
{
	internal class CreateTotalsBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return EmitEmpty(call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public CreateTotalsBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class NewPagePerRecordBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return EmitEmpty(call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public NewPagePerRecordBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class ShowOutputBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return EmitEmpty(call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public ShowOutputBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class NewPageBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return EmitEmpty(call);
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public NewPageBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	internal class PreviewBuiltInMethodTypeSymbol : BuiltInMethodTypeSymbol, IMethodCallEmitter
	{
		public ExpressionSyntax EmitCall(CodeGenerator generator, BoundCall call)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.SyntaxFactory.AssignmentExpression(Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression, CodeGenHelper.MemberAccessExpression(CodeGenHelper.MemberAccessExpression(CodeGenerator.EmitBaseDotParentExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CurrReport")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("PreviewCanPrint")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ConditionalExpression(CodeGenFacts.FalseLiteralExpression, CodeGenFacts.TrueLiteralExpression, CodeGenHelper.MemberAccessExpression(CodeGenHelper.MemberAccessExpression(CodeGenerator.EmitBaseDotParentExpression(), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("CurrReport")), Microsoft.CodeAnalysis.CSharp.SyntaxFactory.IdentifierName("Preview")))));
		}

		internal override IMethodCallEmitter GetCallEmitter()
		{
			return this;
		}

		public PreviewBuiltInMethodTypeSymbol(Symbol containingSymbol, string name, SymbolCompilationScope compilationScope = SymbolCompilationScope.DefinedByOwner, bool isStatic = false, string staticClassName = null, string metadataName = null, bool isProperty = false, bool addDataError = false, bool addThis = false, bool isLocal = false, bool isObsolete = false, TypeSymbol returnValue = null, bool returnValueIsOptional = false, bool returnValueTypeFromFirstArgument = false, Func<Symbol, ParameterSymbol[]> getParameters = null, VersionCompatibility compatibility = null)
			: base(containingSymbol, name, compilationScope, isStatic, staticClassName, metadataName, isProperty, addDataError, addThis, isLocal, isObsolete, returnValue, returnValueIsOptional, returnValueTypeFromFirstArgument, getParameters, addCompilationTarget: false, compatibility)
		{
		}
	}

	private static readonly Lazy<ReportInstanceClassTypeSymbol> instance = new Lazy<ReportInstanceClassTypeSymbol>(() => new ReportInstanceClassTypeSymbol("ReportInstance", isAutoWith: false, (Symbol containingSymbol) => new MethodSymbol[33]
	{
		new BuiltInMethodTypeSymbol(containingSymbol, "Break", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Break", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Skip", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Skip", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new CreateTotalsBuiltInMethodTypeSymbol(containingSymbol, "CreateTotals", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "CreateTotals", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: true, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Var1", 0, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Var2", 1, isVar: true, isOptional: true, isParams: true)
		}),
		new CreateTotalsBuiltInMethodTypeSymbol(containingSymbol, "CreateTotals", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "CreateTotals", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: true, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.DecimalType, "Vars", 0, isVar: false, isOptional: false, isParams: false, isMember: false, isMemberReference: false, typeFromFirstArgument: false, isArray: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "TotalsCausedBy", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: true, NavCorLib.IntegerType),
		new BuiltInMethodTypeSymbol(containingSymbol, "WordXmlPart", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "WordXmlPart", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "ExtendedFormat", 0, isVar: false, isOptional: true)
		}),
		new ShowOutputBuiltInMethodTypeSymbol(containingSymbol, "ShowOutput", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: true, NavCorLib.BooleanType),
		new ShowOutputBuiltInMethodTypeSymbol(containingSymbol, "ShowOutput", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: true, addDataError: false, addThis: false, isLocal: true, isObsolete: true, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "Value", 0)
		}, VersionCompatibility.Parse("2.3")),
		new BuiltInMethodTypeSymbol(containingSymbol, "PageNo", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "PageNo", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: true, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "NewPageNo", 0, isVar: false, isOptional: true)
		}),
		new NewPageBuiltInMethodTypeSymbol(containingSymbol, "NewPage", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: true, NavCorLib.NoneType, returnValueIsOptional: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Quit", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Quit", isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true),
		new PreviewBuiltInMethodTypeSymbol(containingSymbol, "Preview", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Preview", isProperty: true, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.BooleanType),
		new BuiltInMethodTypeSymbol(containingSymbol, "RunModal", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "RunModal", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, null, suppressTarget: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "Run", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Run", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, null, addCompilationTarget: false, null, suppressTarget: true),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsHtml", SymbolCompilationScope.Internal, isStatic: false, null, "SaveAsHtml", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsXml", SymbolCompilationScope.Internal, isStatic: false, null, "SaveAsXml", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsPdf", SymbolCompilationScope.Internal, isStatic: false, null, "SaveAsPdf", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsExcel", SymbolCompilationScope.Internal, isStatic: false, null, "SaveAsExcel", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAsWord", SymbolCompilationScope.Internal, isStatic: false, null, "SaveAsWord", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "FileName", 0)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "WordLayout", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "WordLayout", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "RDLCLayout", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "RDLCLayout", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.InStreamType, "InStream", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "DefaultLayout", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "DefaultLayout", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.DefaultLayoutType),
		new BuiltInMethodTypeSymbol(containingSymbol, "RunRequestPage", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "RunRequestPage", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "PageParameters", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Execute", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Execute", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Parameters", 0),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 1, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Print", SymbolCompilationScope.Internal, isStatic: false, null, "Print", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[3]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Parameters", 0),
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "PrinterName", 1, isVar: false, isOptional: true),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 2, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SaveAs", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SaveAs", isProperty: false, addDataError: true, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[4]
		{
			new BuiltInParameterSymbol(method, NavCorLib.StringType, "Parameters", 0),
			new BuiltInParameterSymbol(method, NavCorLib.ReportFormatType, "Format", 1),
			new BuiltInParameterSymbol(method, NavCorLib.OutStreamType, "OutStream", 2, isVar: true),
			new BuiltInParameterSymbol(method, NavCorLib.RecordRefType, "RecordRef", 3, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "SetTableView", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "SetTableView", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.TableType, "Record", 0, isVar: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "PrintOnlyIfDetail", SymbolCompilationScope.Internal, isStatic: false, null, "PrintOnlyIfDetail", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "SetPrintOnlyIfDetail", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "UseRequestPage", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "UseRequestForm", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "SetUseRequestPage", 0, isVar: false, isOptional: true)
		}),
		new NewPagePerRecordBuiltInMethodTypeSymbol(containingSymbol, "NewPagePerRecord", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "NewPagePerRecord", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: true, NavCorLib.BooleanType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "SetNewPagePerRecord", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "Language", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "Language", isProperty: true, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.IntegerType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "Language", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "ObjectId", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, "ObjectID", isProperty: false, addDataError: false, addThis: false, isLocal: false, isObsolete: false, NavCorLib.StringType, returnValueIsOptional: false, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[1]
		{
			new BuiltInParameterSymbol(method, NavCorLib.BooleanType, "UseNames", 0, isVar: false, isOptional: true)
		}),
		new BuiltInMethodTypeSymbol(containingSymbol, "PaperSource", SymbolCompilationScope.DefinedByOwner, isStatic: false, null, null, isProperty: false, addDataError: false, addThis: false, isLocal: true, isObsolete: false, NavCorLib.NoneType, returnValueIsOptional: true, returnValueTypeFromFirstArgument: false, (Symbol method) => new ParameterSymbol[2]
		{
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "PaperBinNo", 0),
			new BuiltInParameterSymbol(method, NavCorLib.IntegerType, "PhysicalPage", 1, isVar: false, isOptional: true)
		})
	}), isThreadSafe: true);

	internal static ReportInstanceClassTypeSymbol Instance => instance.Value;

	private static ExpressionSyntax EmitEmpty(BoundCall call)
	{
		if (call.IsStatement)
		{
			return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.ParseExpression("//");
		}
		return Microsoft.CodeAnalysis.CSharp.SyntaxFactory.DefaultExpression(CodeGenFacts.GetRuntimeTypeSyntax(call.Type));
	}

	private ReportInstanceClassTypeSymbol(string name, bool isAutoWith, Func<Symbol, Symbol[]> getMethods)
		: base(name, isAutoWith, TypeCategoryKind.ApplicationObject, NavTypeKind.Report, SymbolCompilationScope.Personalization, getMethods)
	{
	}
}
