using System.Diagnostics;
using System.Runtime.CompilerServices;
using Microsoft.Dynamics.Nav.CodeAnalysis.InternalSyntax;

namespace Microsoft.Dynamics.Nav.CodeAnalysis.Syntax;

[DebuggerNonUserCode]
[CompilerGenerated]
public sealed class ReportSyntax : ApplicationObjectSyntax
{
	private ObjectIdSyntax _objectId;

	private IdentifierNameSyntax _name;

	private PropertyListSyntax _propertyList;

	private ReportDataSetSectionSyntax _dataSet;

	private RequestPageSyntax _requestPage;

	private ReportLabelsSectionSyntax _labels;

	private SyntaxNode _members;

	public SyntaxToken ReportKeyword => new SyntaxToken(this, ((InternalReportSyntax)Node)._reportKeyword, Position, 0);

	public override ObjectIdSyntax ObjectId => GetRed(ref _objectId, 1);

	public override IdentifierNameSyntax Name => GetRed(ref _name, 2);

	public SyntaxToken OpenBraceToken => new SyntaxToken(this, ((InternalReportSyntax)Node)._openBraceToken, GetChildPosition(3), GetChildIndex(3));

	public override PropertyListSyntax PropertyList => GetRed(ref _propertyList, 4);

	public ReportDataSetSectionSyntax DataSet => GetRed(ref _dataSet, 5);

	public RequestPageSyntax RequestPage => GetRed(ref _requestPage, 6);

	public ReportLabelsSectionSyntax Labels => GetRed(ref _labels, 7);

	public override SyntaxList<MemberSyntax> Members => new SyntaxList<MemberSyntax>(GetRed(ref _members, 8));

	public SyntaxToken CloseBraceToken => new SyntaxToken(this, ((InternalReportSyntax)Node)._closeBraceToken, GetChildPosition(9), GetChildIndex(9));

	internal ReportSyntax(InternalSyntaxNode green, SyntaxNode parent, int position)
		: base(green, parent, position)
	{
	}

	internal override SyntaxNode GetNodeSlot(int index)
	{
		return index switch
		{
			1 => GetRed(ref _objectId, 1), 
			2 => GetRed(ref _name, 2), 
			4 => GetRed(ref _propertyList, 4), 
			5 => GetRed(ref _dataSet, 5), 
			6 => GetRed(ref _requestPage, 6), 
			7 => GetRed(ref _labels, 7), 
			8 => GetRed(ref _members, 8), 
			_ => null, 
		};
	}

	internal override SyntaxNode GetCachedSlot(int index)
	{
		return index switch
		{
			1 => _objectId, 
			2 => _name, 
			4 => _propertyList, 
			5 => _dataSet, 
			6 => _requestPage, 
			7 => _labels, 
			8 => _members, 
			_ => null, 
		};
	}

	public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
	{
		return visitor.VisitReport(this);
	}

	public override void Accept(SyntaxVisitor visitor)
	{
		visitor.VisitReport(this);
	}

	public ReportSyntax Update(SyntaxToken reportKeyword, ObjectIdSyntax objectId, IdentifierNameSyntax name, SyntaxToken openBraceToken, PropertyListSyntax propertyList, ReportDataSetSectionSyntax dataSet, RequestPageSyntax requestPage, ReportLabelsSectionSyntax labels, SyntaxList<MemberSyntax> members, SyntaxToken closeBraceToken)
	{
		if (reportKeyword != ReportKeyword || objectId != ObjectId || name != Name || openBraceToken != OpenBraceToken || propertyList != PropertyList || dataSet != DataSet || requestPage != RequestPage || labels != Labels || members != Members || closeBraceToken != CloseBraceToken)
		{
			ReportSyntax reportSyntax = SyntaxFactory.Report(reportKeyword, objectId, name, openBraceToken, propertyList, dataSet, requestPage, labels, members, closeBraceToken);
			SyntaxAnnotation[] annotations = GetAnnotations();
			if (annotations != null && annotations.Length != 0)
			{
				return reportSyntax.WithAnnotations(annotations);
			}
			return reportSyntax;
		}
		return this;
	}

	public ReportSyntax WithReportKeyword(SyntaxToken reportKeyword)
	{
		return Update(reportKeyword, ObjectId, Name, OpenBraceToken, PropertyList, DataSet, RequestPage, Labels, Members, CloseBraceToken);
	}

	public ReportSyntax WithObjectId(ObjectIdSyntax objectId)
	{
		return Update(ReportKeyword, objectId, Name, OpenBraceToken, PropertyList, DataSet, RequestPage, Labels, Members, CloseBraceToken);
	}

	public ReportSyntax WithName(IdentifierNameSyntax name)
	{
		return Update(ReportKeyword, ObjectId, name, OpenBraceToken, PropertyList, DataSet, RequestPage, Labels, Members, CloseBraceToken);
	}

	public ReportSyntax WithOpenBraceToken(SyntaxToken openBraceToken)
	{
		return Update(ReportKeyword, ObjectId, Name, openBraceToken, PropertyList, DataSet, RequestPage, Labels, Members, CloseBraceToken);
	}

	public ReportSyntax WithPropertyList(PropertyListSyntax propertyList)
	{
		return Update(ReportKeyword, ObjectId, Name, OpenBraceToken, propertyList, DataSet, RequestPage, Labels, Members, CloseBraceToken);
	}

	public ReportSyntax WithDataSet(ReportDataSetSectionSyntax dataSet)
	{
		return Update(ReportKeyword, ObjectId, Name, OpenBraceToken, PropertyList, dataSet, RequestPage, Labels, Members, CloseBraceToken);
	}

	public ReportSyntax WithRequestPage(RequestPageSyntax requestPage)
	{
		return Update(ReportKeyword, ObjectId, Name, OpenBraceToken, PropertyList, DataSet, requestPage, Labels, Members, CloseBraceToken);
	}

	public ReportSyntax WithLabels(ReportLabelsSectionSyntax labels)
	{
		return Update(ReportKeyword, ObjectId, Name, OpenBraceToken, PropertyList, DataSet, RequestPage, labels, Members, CloseBraceToken);
	}

	public ReportSyntax WithMembers(SyntaxList<MemberSyntax> members)
	{
		return Update(ReportKeyword, ObjectId, Name, OpenBraceToken, PropertyList, DataSet, RequestPage, Labels, members, CloseBraceToken);
	}

	public ReportSyntax WithCloseBraceToken(SyntaxToken closeBraceToken)
	{
		return Update(ReportKeyword, ObjectId, Name, OpenBraceToken, PropertyList, DataSet, RequestPage, Labels, Members, closeBraceToken);
	}

	public ReportSyntax AddPropertyListProperties(params PropertySyntaxOrEmpty[] items)
	{
		return WithPropertyList(PropertyList.WithProperties(PropertyList.Properties.AddRange(items)));
	}

	public ReportSyntax AddDataSetDataItems(params ReportDataItemSyntax[] items)
	{
		ReportDataSetSectionSyntax reportDataSetSectionSyntax = DataSet ?? SyntaxFactory.ReportDataSetSection();
		return WithDataSet(reportDataSetSectionSyntax.WithDataItems(reportDataSetSectionSyntax.DataItems.AddRange(items)));
	}

	public ReportSyntax AddRequestPageMembers(params MemberSyntax[] items)
	{
		RequestPageSyntax requestPageSyntax = RequestPage ?? SyntaxFactory.RequestPage();
		return WithRequestPage(requestPageSyntax.WithMembers(requestPageSyntax.Members.AddRange(items)));
	}

	public ReportSyntax AddLabelsLabels(params ReportLabelBaseSyntax[] items)
	{
		ReportLabelsSectionSyntax reportLabelsSectionSyntax = Labels ?? SyntaxFactory.ReportLabelsSection();
		return WithLabels(reportLabelsSectionSyntax.WithLabels(reportLabelsSectionSyntax.Labels.AddRange(items)));
	}

	public ReportSyntax AddMembers(params MemberSyntax[] items)
	{
		return WithMembers(Members.AddRange(items));
	}
}
