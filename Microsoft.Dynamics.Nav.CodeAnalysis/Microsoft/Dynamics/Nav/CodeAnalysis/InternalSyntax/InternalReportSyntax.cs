using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.Diagnostics;
using Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

[DebuggerNonUserCode]
[CompilerGenerated]
internal sealed class InternalReportSyntax : InternalApplicationObjectSyntax
{
	internal readonly InternalSyntaxToken _reportKeyword;

	internal readonly InternalObjectIdSyntax _objectId;

	internal readonly InternalIdentifierNameSyntax _name;

	internal readonly InternalSyntaxToken _openBraceToken;

	internal readonly InternalPropertyListSyntax _propertyList;

	internal readonly InternalReportDataSetSectionSyntax _dataSet;

	internal readonly InternalRequestPageSyntax _requestPage;

	internal readonly InternalReportLabelsSectionSyntax _labels;

	internal readonly InternalSyntaxNode _members;

	internal readonly InternalSyntaxToken _closeBraceToken;

	internal override SyntaxNodeDefinition Definition => ReportSyntaxDefinition.Instance.Value;

	public InternalSyntaxToken ReportKeyword => _reportKeyword;

	public override InternalObjectIdSyntax ObjectId => _objectId;

	public override InternalIdentifierNameSyntax Name => _name;

	public InternalSyntaxToken OpenBraceToken => _openBraceToken;

	public override InternalPropertyListSyntax PropertyList => _propertyList;

	public InternalReportDataSetSectionSyntax DataSet => _dataSet;

	public InternalRequestPageSyntax RequestPage => _requestPage;

	public InternalReportLabelsSectionSyntax Labels => _labels;

	public override InternalSyntaxList<InternalMemberSyntax> Members => new InternalSyntaxList<InternalMemberSyntax>(_members);

	public InternalSyntaxToken CloseBraceToken => _closeBraceToken;

	internal InternalReportSyntax(SyntaxKind kind, InternalSyntaxToken reportKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalReportDataSetSectionSyntax dataSet, InternalRequestPageSyntax requestPage, InternalReportLabelsSectionSyntax labels, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, DiagnosticInfo[] diagnostics, SyntaxAnnotation[] annotations)
		: base(kind, diagnostics, annotations)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(reportKeyword);
		_reportKeyword = reportKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (dataSet != null)
		{
			AdjustFlagsAndWidth(dataSet);
			_dataSet = dataSet;
		}
		if (requestPage != null)
		{
			AdjustFlagsAndWidth(requestPage);
			_requestPage = requestPage;
		}
		if (labels != null)
		{
			AdjustFlagsAndWidth(labels);
			_labels = labels;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalReportSyntax(SyntaxKind kind, InternalSyntaxToken reportKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalReportDataSetSectionSyntax dataSet, InternalRequestPageSyntax requestPage, InternalReportLabelsSectionSyntax labels, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken, LexerMode mode)
		: base(kind, mode)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(reportKeyword);
		_reportKeyword = reportKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (dataSet != null)
		{
			AdjustFlagsAndWidth(dataSet);
			_dataSet = dataSet;
		}
		if (requestPage != null)
		{
			AdjustFlagsAndWidth(requestPage);
			_requestPage = requestPage;
		}
		if (labels != null)
		{
			AdjustFlagsAndWidth(labels);
			_labels = labels;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal InternalReportSyntax(SyntaxKind kind, InternalSyntaxToken reportKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalReportDataSetSectionSyntax dataSet, InternalRequestPageSyntax requestPage, InternalReportLabelsSectionSyntax labels, InternalSyntaxNode members, InternalSyntaxToken closeBraceToken)
		: base(kind)
	{
		SlotCount = 10;
		AdjustFlagsAndWidth(reportKeyword);
		_reportKeyword = reportKeyword;
		AdjustFlagsAndWidth(objectId);
		_objectId = objectId;
		AdjustFlagsAndWidth(name);
		_name = name;
		AdjustFlagsAndWidth(openBraceToken);
		_openBraceToken = openBraceToken;
		AdjustFlagsAndWidth(propertyList);
		_propertyList = propertyList;
		if (dataSet != null)
		{
			AdjustFlagsAndWidth(dataSet);
			_dataSet = dataSet;
		}
		if (requestPage != null)
		{
			AdjustFlagsAndWidth(requestPage);
			_requestPage = requestPage;
		}
		if (labels != null)
		{
			AdjustFlagsAndWidth(labels);
			_labels = labels;
		}
		if (members != null)
		{
			AdjustFlagsAndWidth(members);
			_members = members;
		}
		AdjustFlagsAndWidth(closeBraceToken);
		_closeBraceToken = closeBraceToken;
	}

	internal override InternalSyntaxNode GetSlot(int index)
	{
		return index switch
		{
			0 => _reportKeyword, 
			1 => _objectId, 
			2 => _name, 
			3 => _openBraceToken, 
			4 => _propertyList, 
			5 => _dataSet, 
			6 => _requestPage, 
			7 => _labels, 
			8 => _members, 
			9 => _closeBraceToken, 
			_ => null, 
		};
	}

	internal override SyntaxNode CreateRed(SyntaxNode parent, int position)
	{
		return new ReportSyntax(this, parent, position);
	}

	public override TResult Accept<TResult>(InternalSyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReport(this);
	}

	public override void Accept(InternalSyntaxVisitor visitor)
	{
		visitor.VisitReport(this);
	}

	public InternalReportSyntax Update(InternalSyntaxToken reportKeyword, InternalObjectIdSyntax objectId, InternalIdentifierNameSyntax name, InternalSyntaxToken openBraceToken, InternalPropertyListSyntax propertyList, InternalReportDataSetSectionSyntax dataSet, InternalRequestPageSyntax requestPage, InternalReportLabelsSectionSyntax labels, InternalSyntaxList<InternalMemberSyntax> members, InternalSyntaxToken closeBraceToken)
	{
		if (reportKeyword != ReportKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || dataSet != DataSet || requestPage != RequestPage || labels != Labels || members != Members || closeBraceToken != CloseBraceToken)
		{
			return InternalSyntaxFactory.Report(reportKeyword, objectId, name, openBraceToken, propertyList, dataSet, requestPage, labels, members, closeBraceToken).WithDiagnosticsAndAnnotations();
		}
		return this;
	}

	internal override InternalSyntaxNode SetDiagnostics(DiagnosticInfo[] diagnostics)
	{
		return new InternalReportSyntax(Kind, _reportKeyword, _objectId, _name, _openBraceToken, _propertyList, _dataSet, _requestPage, _labels, _members, _closeBraceToken, diagnostics, GetAnnotations());
	}

	internal override InternalSyntaxNode SetAnnotations(SyntaxAnnotation[] annotations)
	{
		return new InternalReportSyntax(Kind, _reportKeyword, _objectId, _name, _openBraceToken, _propertyList, _dataSet, _requestPage, _labels, _members, _closeBraceToken, GetDiagnostics(), annotations);
	}
}
