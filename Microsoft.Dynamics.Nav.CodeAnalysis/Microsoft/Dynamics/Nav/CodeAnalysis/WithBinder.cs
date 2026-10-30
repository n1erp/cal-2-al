using System.Collections.Generic;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Symbols;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis;

internal class WithBinder : Binder
{
	private readonly WithStatementSyntax syntax;

	private TypeSymbol withIdType;

	private BoundExpression withId;

	public WithBinder(Binder enclosing, WithStatementSyntax syntax)
		: base(enclosing)
	{
		DebugAssertHelper.Assert(syntax != null);
		this.syntax = syntax;
	}

	internal override BoundWithStatement BindWithParts(DiagnosticBag diagnostics, Binder originalBinder)
	{
		BindWithId(diagnostics, originalBinder);
		bool hasErrors = false;
		if (!withIdType.CanTypeBeUsedInWithStatement())
		{
			Binder.Error(diagnostics, ErrorCode.ERR_ExpressionNotValidForWith, syntax.WithId);
			hasErrors = true;
		}
		BoundStatement statement = BindStatement(syntax.Statement, diagnostics);
		return new BoundWithStatement(syntax, withId, statement, hasErrors);
	}

	protected override void AddLookupSymbolsInfoInSingleBinder(LookupSymbolsInfo info, LookupOptions options, Binder originalBinder)
	{
		if (withId == null)
		{
			DiagnosticBag instance = DiagnosticBag.GetInstance();
			try
			{
				BindWithId(instance, originalBinder);
			}
			finally
			{
				instance.Free();
			}
		}
		if (withIdType != null)
		{
			Binder.AddMemberLookupSymbolsInfo(info, withIdType, options, originalBinder);
		}
	}

	private void BindWithId(DiagnosticBag diagnostics, Binder originalBinder)
	{
		withId = originalBinder.BindValue(syntax.WithId, diagnostics, BindValueKind.RValue);
		withIdType = withId.Type;
	}

	internal override void LookupSymbolsInSingleBinder(LookupResult result, string name, LookupOptions options, Binder originalBinder, bool diagnose, ref HashSet<DiagnosticInfo> useSiteDiagnostics)
	{
		DebugAssertHelper.Assert(result.IsClear);
		options |= LookupOptions.MemberAccess;
		Binder.LookupMembersInternal(result, withIdType, name, options, originalBinder, diagnose, ref useSiteDiagnostics, withId);
	}
}
