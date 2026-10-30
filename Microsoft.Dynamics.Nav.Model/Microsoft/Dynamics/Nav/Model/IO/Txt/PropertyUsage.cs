using System.Diagnostics;
using Microsoft.Dynamics.Nav.MetaMetaModel;
using Microsoft.Dynamics.Nav.Model.TypeSystem;

namespace Microsoft.Dynamics.Nav.Model.IO.Txt;

[DebuggerDisplay("Element={ElementType}, Property={PropertyType}")]
public class PropertyUsage
{
	internal static PropertyUsage[] Definitions = new PropertyUsage[832]
	{
		new PropertyUsage(ElementType.Modification, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Modification, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Modification, PropertyType.Target, DataTypes.Text),
		new PropertyUsage(ElementType.Modification, PropertyType.ChangeType, DataTypes.Option),
		new PropertyUsage(ElementType.Modification, PropertyType.ResidualHint, DataTypes.Text),
		new PropertyUsage(ElementType.Modification, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Modification, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Modification, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Modification, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.Modification, PropertyType.TargetPageType, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.Target, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.Property, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.ChangeType, DataTypes.Option),
		new PropertyUsage(ElementType.Change, PropertyType.BaselineValue, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.ResultingValue, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.InsertAfter, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.IncludeSiblings, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.AddedChildren, DataTypes.Container)
		{
			ContainerName = "ChangeCollection",
			ContainerChildElementType = ElementType.Change
		},
		new PropertyUsage(ElementType.Change, PropertyType.ResidualHint, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.NewBaselineValue, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.OriginalValue, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.ModifiedValue, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.TargetValue, DataTypes.Text),
		new PropertyUsage(ElementType.Change, PropertyType.ConflictHint, DataTypes.Text),
		new PropertyUsage(ElementType.Variable, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Variable, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Variable, PropertyType.DataType, DataTypes.Option),
		new PropertyUsage(ElementType.Variable, PropertyType.OptionString, DataTypes.Text)
		{
			AttributeId = 10340
		},
		new PropertyUsage(ElementType.Variable, PropertyType.Subtype, DataTypes.Option)
		{
			AttributeId = 15364
		},
		new PropertyUsage(ElementType.Variable, PropertyType.Length, DataTypes.Integer)
		{
			AttributeId = 15363
		},
		new PropertyUsage(ElementType.Variable, PropertyType.Dimensions, DataTypes.Dimensions),
		new PropertyUsage(ElementType.Variable, PropertyType.Temporary, DataTypes.Boolean),
		new PropertyUsage(ElementType.Variable, PropertyType.InDataSet, DataTypes.Boolean)
		{
			AttributeId = 26178
		},
		new PropertyUsage(ElementType.Variable, PropertyType.WithEvents, DataTypes.Boolean)
		{
			AttributeId = 26173
		},
		new PropertyUsage(ElementType.Variable, PropertyType.RunOnClient, DataTypes.Boolean)
		{
			AttributeId = 26182
		},
		new PropertyUsage(ElementType.Variable, PropertyType.SecurityFiltering, DataTypes.Option),
		new PropertyUsage(ElementType.Variable, PropertyType.TextConstString, DataTypes.MLText)
		{
			AttributeId = 26171
		},
		new PropertyUsage(ElementType.Variable, PropertyType.ExternalDataType, DataTypes.Text),
		new PropertyUsage(ElementType.Variable, PropertyType.SuppressDispose, DataTypes.Boolean),
		new PropertyUsage(ElementType.Variable, PropertyType.NonDebuggable, DataTypes.Boolean)
		{
			AttributeId = 26230
		},
		new PropertyUsage(ElementType.Procedure, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Procedure, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Procedure, PropertyType.Attributes, DataTypes.Text),
		new PropertyUsage(ElementType.Procedure, PropertyType.Local, DataTypes.Boolean)
		{
			AttributeId = 26165
		},
		new PropertyUsage(ElementType.Procedure, PropertyType.EventType, DataTypes.Option),
		new PropertyUsage(ElementType.Procedure, PropertyType.EventPublishedObject, DataTypes.Text),
		new PropertyUsage(ElementType.Procedure, PropertyType.EventFunction, DataTypes.Text),
		new PropertyUsage(ElementType.Procedure, PropertyType.TryFunction, DataTypes.Boolean),
		new PropertyUsage(ElementType.Procedure, PropertyType.TestPermissions, DataTypes.Option)
		{
			AttributeId = 26366
		},
		new PropertyUsage(ElementType.Procedure, PropertyType.FunctionVisibility, DataTypes.Option)
		{
			AttributeId = 26220
		},
		new PropertyUsage(ElementType.Procedure, PropertyType.ServiceEnabled, DataTypes.Boolean)
		{
			AttributeId = 26227
		},
		new PropertyUsage(ElementType.Procedure, PropertyType.LineStart, DataTypes.Integer),
		new PropertyUsage(ElementType.Procedure, PropertyType.NonDebuggable, DataTypes.Boolean)
		{
			AttributeId = 26230
		},
		new PropertyUsage(ElementType.Parameter, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Parameter, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Parameter, PropertyType.DataType, DataTypes.Option),
		new PropertyUsage(ElementType.Parameter, PropertyType.OptionString, DataTypes.Text)
		{
			AttributeId = 10340
		},
		new PropertyUsage(ElementType.Parameter, PropertyType.Subtype, DataTypes.Option)
		{
			AttributeId = 15364
		},
		new PropertyUsage(ElementType.Parameter, PropertyType.Length, DataTypes.Integer)
		{
			AttributeId = 15363
		},
		new PropertyUsage(ElementType.Parameter, PropertyType.Dimensions, DataTypes.Dimensions),
		new PropertyUsage(ElementType.Parameter, PropertyType.Temporary, DataTypes.Boolean),
		new PropertyUsage(ElementType.Parameter, PropertyType.AsVar, DataTypes.Boolean),
		new PropertyUsage(ElementType.Parameter, PropertyType.RunOnClient, DataTypes.Boolean)
		{
			AttributeId = 26182
		},
		new PropertyUsage(ElementType.Parameter, PropertyType.SecurityFiltering, DataTypes.Option),
		new PropertyUsage(ElementType.Parameter, PropertyType.TextConstString, DataTypes.MLText)
		{
			AttributeId = 26171
		},
		new PropertyUsage(ElementType.Parameter, PropertyType.ExternalDataType, DataTypes.Text),
		new PropertyUsage(ElementType.Parameter, PropertyType.SuppressDispose, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReturnValue, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.ReturnValue, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.ReturnValue, PropertyType.DataType, DataTypes.Option),
		new PropertyUsage(ElementType.ReturnValue, PropertyType.Length, DataTypes.Integer)
		{
			AttributeId = 15363
		},
		new PropertyUsage(ElementType.ReturnValue, PropertyType.Dimensions, DataTypes.Dimensions),
		new PropertyUsage(ElementType.Event, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Event, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Event, PropertyType.EventSource, DataTypes.Text),
		new PropertyUsage(ElementType.Event, PropertyType.EventSourceId, DataTypes.Integer),
		new PropertyUsage(ElementType.Report, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Report, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Report, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Report, PropertyType.RdlData, DataTypes.Text)
		{
			AttributeId = 18573
		},
		new PropertyUsage(ElementType.Report, PropertyType.HorizontalGrid, DataTypes.Integer)
		{
			AttributeId = 21035
		},
		new PropertyUsage(ElementType.Report, PropertyType.LeftMargin, DataTypes.Integer)
		{
			AttributeId = 18464
		},
		new PropertyUsage(ElementType.Report, PropertyType.EnableExternalImages, DataTypes.Boolean)
		{
			AttributeId = 18470
		},
		new PropertyUsage(ElementType.Report, PropertyType.EnableHyperlinks, DataTypes.Boolean)
		{
			AttributeId = 18471
		},
		new PropertyUsage(ElementType.Report, PropertyType.ProcessingOnly, DataTypes.Boolean)
		{
			AttributeId = 18469
		},
		new PropertyUsage(ElementType.Report, PropertyType.ShowPrintStatus, DataTypes.Boolean)
		{
			AttributeId = 18453
		},
		new PropertyUsage(ElementType.Report, PropertyType.UseReqForm, DataTypes.Boolean)
		{
			AttributeId = 18454
		},
		new PropertyUsage(ElementType.Report, PropertyType.UseSystemPrinter, DataTypes.Boolean)
		{
			AttributeId = 18468
		},
		new PropertyUsage(ElementType.Report, PropertyType.AdditionalSearchTermsML, DataTypes.MLText)
		{
			AttributeId = 55074
		},
		new PropertyUsage(ElementType.Report, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.Report, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.Report, PropertyType.Orientation, DataTypes.Option)
		{
			AttributeId = 18542
		},
		new PropertyUsage(ElementType.Report, PropertyType.PaperSize, DataTypes.Option)
		{
			AttributeId = 18543
		},
		new PropertyUsage(ElementType.Report, PropertyType.TransactionType, DataTypes.Option)
		{
			AttributeId = 4980
		},
		new PropertyUsage(ElementType.Report, PropertyType.BottomMargin, DataTypes.Integer)
		{
			AttributeId = 18463
		},
		new PropertyUsage(ElementType.Report, PropertyType.RightMargin, DataTypes.Integer)
		{
			AttributeId = 18465
		},
		new PropertyUsage(ElementType.Report, PropertyType.TopMargin, DataTypes.Integer)
		{
			AttributeId = 18462
		},
		new PropertyUsage(ElementType.Report, PropertyType.DeviceFontName, DataTypes.Text)
		{
			AttributeId = 18545
		},
		new PropertyUsage(ElementType.Report, PropertyType.UseRequestPage, DataTypes.Boolean)
		{
			AttributeId = 18556
		},
		new PropertyUsage(ElementType.Report, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Report, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Report, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Report, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.Report, PropertyType.UsageCategory, DataTypes.Option)
		{
			AttributeId = 55071
		},
		new PropertyUsage(ElementType.Report, PropertyType.AccessByPermission, DataTypes.Text)
		{
			AttributeId = 5011
		},
		new PropertyUsage(ElementType.Report, PropertyType.PaperSourceDefaultPage, DataTypes.Option)
		{
			AttributeId = 18547
		},
		new PropertyUsage(ElementType.Report, PropertyType.PaperSourceFirstPage, DataTypes.Option)
		{
			AttributeId = 18548
		},
		new PropertyUsage(ElementType.Report, PropertyType.PaperSourceLastPage, DataTypes.Option)
		{
			AttributeId = 18549
		},
		new PropertyUsage(ElementType.Report, PropertyType.PreviewMode, DataTypes.Option)
		{
			AttributeId = 18550
		},
		new PropertyUsage(ElementType.Report, PropertyType.DefaultLayout, DataTypes.Option)
		{
			AttributeId = 18551
		},
		new PropertyUsage(ElementType.Report, PropertyType.WordMergeDataItem, DataTypes.Text)
		{
			AttributeId = 18559
		},
		new PropertyUsage(ElementType.Report, PropertyType.WordLayout, DataTypes.Text)
		{
			AttributeId = 18832
		},
		new PropertyUsage(ElementType.Report, PropertyType.EnableExternalAssemblies, DataTypes.Text)
		{
			AttributeId = 18470
		},
		new PropertyUsage(ElementType.Report, PropertyType.ApplicationArea, DataTypes.Text)
		{
			AttributeId = 10346
		},
		new PropertyUsage(ElementType.Report, PropertyType.PDFFontEmbedding, DataTypes.Boolean)
		{
			AttributeId = 18560
		},
		new PropertyUsage(ElementType.MenuSuite, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.MenuSuite, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.MenuSuite, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.MenuSuite, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.MenuSuite, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.MenuSuite, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.XmlPort, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.InlineSchema, DataTypes.Boolean)
		{
			AttributeId = 54110
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.PreserveWhiteSpace, DataTypes.Boolean)
		{
			AttributeId = 54044
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.UseDefaultNamespace, DataTypes.Boolean)
		{
			AttributeId = 54111
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.UseLax, DataTypes.Boolean)
		{
			AttributeId = 54173
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.UseRequestForm, DataTypes.Boolean)
		{
			AttributeId = 54166
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.DefaultNamespace, DataTypes.Text)
		{
			AttributeId = 54109
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.FieldDelimiter, DataTypes.Text)
		{
			AttributeId = 54167
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.FieldSeparator, DataTypes.Text)
		{
			AttributeId = 54168
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.Format, DataTypes.Text)
		{
			AttributeId = 54166
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.TableSeparator, DataTypes.Text)
		{
			AttributeId = 54170
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.Direction, DataTypes.Option)
		{
			AttributeId = 54036
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.Encoding, DataTypes.Option)
		{
			AttributeId = 54038
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.FormatEvaluate, DataTypes.Option)
		{
			AttributeId = 54043
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.TransactionType, DataTypes.Option)
		{
			AttributeId = 4980
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.UseRequestPage, DataTypes.Boolean)
		{
			AttributeId = 54171
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.TextEncoding, DataTypes.Option)
		{
			AttributeId = 54045
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.FileName, DataTypes.Text)
		{
			AttributeId = 54174
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.XmlPort, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.XmlPort, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.XmlPort, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.XmlPort, PropertyType.DefaultFieldsValidation, DataTypes.Boolean)
		{
			AttributeId = 54041
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.XmlVersionNo, DataTypes.Option)
		{
			AttributeId = 54042
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.PreviewMode, DataTypes.Option)
		{
			AttributeId = 18550
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.ReadState, DataTypes.Option)
		{
			AttributeId = 57605
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.RecordSeparator, DataTypes.Text)
		{
			AttributeId = 54169
		},
		new PropertyUsage(ElementType.XmlPort, PropertyType.Namespaces, DataTypes.Text)
		{
			AttributeId = 54108
		},
		new PropertyUsage(ElementType.ReportSection, PropertyType.SectionType, DataTypes.Option),
		new PropertyUsage(ElementType.ReportSection, PropertyType.SectionHeight, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportSection, PropertyType.SectionWidth, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportSection, PropertyType.KeepWithNext, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportSection, PropertyType.PlaceInBottom, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportSection, PropertyType.PrintOnEveryPage, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportItem, PropertyType.DataItemTable, DataTypes.Text)
		{
			AttributeId = 18474
		},
		new PropertyUsage(ElementType.ReportItem, PropertyType.DataItemVarName, DataTypes.Text),
		new PropertyUsage(ElementType.ReportItem, PropertyType.DataItemIndent, DataTypes.Integer)
		{
			AttributeId = 18472
		},
		new PropertyUsage(ElementType.ReportItem, PropertyType.MaxIteration, DataTypes.Integer)
		{
			AttributeId = 18486
		},
		new PropertyUsage(ElementType.ReportItem, PropertyType.NewPagePerGroup, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportItem, PropertyType.NewPagePerRecord, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportItem, PropertyType.PrintOnlyIfDetail, DataTypes.Boolean)
		{
			AttributeId = 18488
		},
		new PropertyUsage(ElementType.ReportItem, PropertyType.CalcFields, DataTypes.Text),
		new PropertyUsage(ElementType.ReportItem, PropertyType.DataItemLink, DataTypes.Text)
		{
			AttributeId = 18538
		},
		new PropertyUsage(ElementType.ReportItem, PropertyType.DataItemLinkReference, DataTypes.Text)
		{
			AttributeId = 18536
		},
		new PropertyUsage(ElementType.ReportItem, PropertyType.DataItemTableView, DataTypes.Text)
		{
			AttributeId = 18475
		},
		new PropertyUsage(ElementType.ReportItem, PropertyType.GroupTotalFields, DataTypes.Text),
		new PropertyUsage(ElementType.ReportItem, PropertyType.RequestFilterFields, DataTypes.Text)
		{
			AttributeId = 18482
		},
		new PropertyUsage(ElementType.ReportItem, PropertyType.TotalFields, DataTypes.Text),
		new PropertyUsage(ElementType.ReportItem, PropertyType.RequestFilterHeadingML, DataTypes.MLText)
		{
			AttributeId = 18490
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.OptionString, DataTypes.Text)
		{
			AttributeId = 10340
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.SourceExpression, DataTypes.Text)
		{
			AttributeId = 21052
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.ControlType, DataTypes.Option)
		{
			AttributeId = 55040
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.DecimalPlaces, DataTypes.DecimalPlaces)
		{
			AttributeId = 12816
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.Divisor, DataTypes.Integer)
		{
			AttributeId = 15394
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.FontSize, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportControl, PropertyType.ForeColor, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportControl, PropertyType.ControlHeight, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportControl, PropertyType.ParentControl, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportControl, PropertyType.ControlWidth, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportControl, PropertyType.X, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportControl, PropertyType.Y, DataTypes.Integer),
		new PropertyUsage(ElementType.ReportControl, PropertyType.AutoCalcField, DataTypes.Boolean)
		{
			AttributeId = 21057
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.BlankNumbers, DataTypes.Option)
		{
			AttributeId = 15376
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.BlankZero, DataTypes.Boolean)
		{
			AttributeId = 15377
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.Enabled, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportControl, PropertyType.FontBold, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportControl, PropertyType.FontItalic, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportControl, PropertyType.FontUnderline, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportControl, PropertyType.LeaderDots, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportControl, PropertyType.Multiline, DataTypes.Boolean)
		{
			AttributeId = 55121
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.PasswordText, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportControl, PropertyType.Visible, DataTypes.Boolean),
		new PropertyUsage(ElementType.ReportControl, PropertyType.AutoFormatExpression, DataTypes.Text)
		{
			AttributeId = 21078
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.AutoFormatType, DataTypes.Text)
		{
			AttributeId = 21076
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.CaptionClass, DataTypes.Text)
		{
			AttributeId = 21080
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.DataSetFieldName, DataTypes.Text),
		new PropertyUsage(ElementType.ReportControl, PropertyType.FontName, DataTypes.Text),
		new PropertyUsage(ElementType.ReportControl, PropertyType.Format, DataTypes.Text)
		{
			AttributeId = 54166
		},
		new PropertyUsage(ElementType.ReportControl, PropertyType.PadChar, DataTypes.Text),
		new PropertyUsage(ElementType.ReportControl, PropertyType.Border, DataTypes.Option),
		new PropertyUsage(ElementType.ReportControl, PropertyType.BorderWidth, DataTypes.Option),
		new PropertyUsage(ElementType.ReportControl, PropertyType.HorizontalAlign, DataTypes.Option),
		new PropertyUsage(ElementType.ReportControl, PropertyType.HorizontalGlue, DataTypes.Option),
		new PropertyUsage(ElementType.ReportControl, PropertyType.ShapeStyle, DataTypes.Option),
		new PropertyUsage(ElementType.ReportControl, PropertyType.VerticalAlign, DataTypes.Option),
		new PropertyUsage(ElementType.ReportControl, PropertyType.OptionCaptionML, DataTypes.MLText)
		{
			AttributeId = 8632
		},
		new PropertyUsage(ElementType.Package, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Package, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Package, PropertyType.Namespace, DataTypes.Text),
		new PropertyUsage(ElementType.Package, PropertyType.PackageType, DataTypes.Option),
		new PropertyUsage(ElementType.Package, PropertyType.Label, DataTypes.Text),
		new PropertyUsage(ElementType.Package, PropertyType.ReleaseDate, DataTypes.Text),
		new PropertyUsage(ElementType.Package, PropertyType.Version, DataTypes.Text),
		new PropertyUsage(ElementType.Package, PropertyType.ExplicitItems, DataTypes.Boolean),
		new PropertyUsage(ElementType.Package, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Package, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Package, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Package, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.Item, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Item, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Item, PropertyType.ObjectType, DataTypes.Text),
		new PropertyUsage(ElementType.Reference, PropertyType.Reference, DataTypes.Text),
		new PropertyUsage(ElementType.Reference, PropertyType.MinCompatibleVersion, DataTypes.Text),
		new PropertyUsage(ElementType.Reference, PropertyType.MaxCompatibleVersion, DataTypes.Text),
		new PropertyUsage(ElementType.Codeunit, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Codeunit, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Codeunit, PropertyType.CodeunitSubtype, DataTypes.Option)
		{
			AttributeId = 26366
		},
		new PropertyUsage(ElementType.Codeunit, PropertyType.TableNo, DataTypes.ObjectId),
		new PropertyUsage(ElementType.Codeunit, PropertyType.SingleInstance, DataTypes.Boolean)
		{
			AttributeId = 26365
		},
		new PropertyUsage(ElementType.Codeunit, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.Codeunit, PropertyType.TestIsolation, DataTypes.Option)
		{
			AttributeId = 26367
		},
		new PropertyUsage(ElementType.Codeunit, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Codeunit, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Codeunit, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Codeunit, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.Codeunit, PropertyType.EventSubscriberInstance, DataTypes.Text),
		new PropertyUsage(ElementType.Codeunit, PropertyType.TestPermissions, DataTypes.Option)
		{
			AttributeId = 26366
		},
		new PropertyUsage(ElementType.Table, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Table, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Table, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Table, PropertyType.DrillDownPageId, DataTypes.PageId)
		{
			AttributeId = 21064
		},
		new PropertyUsage(ElementType.Table, PropertyType.LookupPageId, DataTypes.PageId)
		{
			AttributeId = 21063
		},
		new PropertyUsage(ElementType.Table, PropertyType.DataPerCompany, DataTypes.Boolean)
		{
			AttributeId = 4935
		},
		new PropertyUsage(ElementType.Table, PropertyType.LinkedInTransaction, DataTypes.Boolean)
		{
			AttributeId = 4984
		},
		new PropertyUsage(ElementType.Table, PropertyType.LinkedObject, DataTypes.Boolean)
		{
			AttributeId = 4983
		},
		new PropertyUsage(ElementType.Table, PropertyType.PasteIsValid, DataTypes.Boolean)
		{
			AttributeId = 15423
		},
		new PropertyUsage(ElementType.Table, PropertyType.TrackingObject, DataTypes.Boolean)
		{
			AttributeId = 5005
		},
		new PropertyUsage(ElementType.Table, PropertyType.DataCaptionFields, DataTypes.Text)
		{
			AttributeId = 4950
		},
		new PropertyUsage(ElementType.Table, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.Table, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.Table, PropertyType.DrillDownFormId, DataTypes.FormId)
		{
			AttributeId = 21064
		},
		new PropertyUsage(ElementType.Table, PropertyType.LookupFormId, DataTypes.FormId)
		{
			AttributeId = 21063
		},
		new PropertyUsage(ElementType.Table, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Table, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Table, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Table, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.Table, PropertyType.TableType, DataTypes.Option)
		{
			AttributeId = 5044
		},
		new PropertyUsage(ElementType.Table, PropertyType.ExternalName, DataTypes.Text)
		{
			AttributeId = 5045
		},
		new PropertyUsage(ElementType.Table, PropertyType.ExternalSchema, DataTypes.Text)
		{
			AttributeId = 5051
		},
		new PropertyUsage(ElementType.Table, PropertyType.ObsoleteState, DataTypes.Option)
		{
			AttributeId = 5052
		},
		new PropertyUsage(ElementType.Table, PropertyType.ObsoleteReason, DataTypes.Text)
		{
			AttributeId = 5053
		},
		new PropertyUsage(ElementType.Table, PropertyType.DataClassification, DataTypes.Option)
		{
			AttributeId = 5056
		},
		new PropertyUsage(ElementType.Table, PropertyType.ReplicateData, DataTypes.Boolean)
		{
			AttributeId = 5057
		},
		new PropertyUsage(ElementType.Table, PropertyType.CompressionType, DataTypes.Option)
		{
			AttributeId = 5062
		},
		new PropertyUsage(ElementType.Page, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Page, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Page, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Page, PropertyType.CardPageId, DataTypes.PageId)
		{
			AttributeId = 55095
		},
		new PropertyUsage(ElementType.Page, PropertyType.SourceTable, DataTypes.ObjectId)
		{
			AttributeId = 21029
		},
		new PropertyUsage(ElementType.Page, PropertyType.TimerUpdate, DataTypes.Integer)
		{
			AttributeId = 55094
		},
		new PropertyUsage(ElementType.Page, PropertyType.AutoSplitKey, DataTypes.Boolean)
		{
			AttributeId = 55120
		},
		new PropertyUsage(ElementType.Page, PropertyType.DelayedInsert, DataTypes.Boolean)
		{
			AttributeId = 21044
		},
		new PropertyUsage(ElementType.Page, PropertyType.DeleteAllowed, DataTypes.Boolean)
		{
			AttributeId = 15412
		},
		new PropertyUsage(ElementType.Page, PropertyType.Editable, DataTypes.Boolean)
		{
			AttributeId = 8509
		},
		new PropertyUsage(ElementType.Page, PropertyType.InsertAllowed, DataTypes.Boolean)
		{
			AttributeId = 15411
		},
		new PropertyUsage(ElementType.Page, PropertyType.LinksAllowed, DataTypes.Boolean)
		{
			AttributeId = 15424
		},
		new PropertyUsage(ElementType.Page, PropertyType.ModifyAllowed, DataTypes.Boolean)
		{
			AttributeId = 15413
		},
		new PropertyUsage(ElementType.Page, PropertyType.MultipleNewLines, DataTypes.Boolean)
		{
			AttributeId = 15410
		},
		new PropertyUsage(ElementType.Page, PropertyType.PopulateAllFields, DataTypes.Boolean)
		{
			AttributeId = 21045
		},
		new PropertyUsage(ElementType.Page, PropertyType.RefreshOnActivate, DataTypes.Boolean)
		{
			AttributeId = 55131
		},
		new PropertyUsage(ElementType.Page, PropertyType.SaveValues, DataTypes.Boolean)
		{
			AttributeId = 15395
		},
		new PropertyUsage(ElementType.Page, PropertyType.ShowFilter, DataTypes.Boolean)
		{
			AttributeId = 55136
		},
		new PropertyUsage(ElementType.Page, PropertyType.SourceTableTemporary, DataTypes.Boolean)
		{
			AttributeId = 55078
		},
		new PropertyUsage(ElementType.Page, PropertyType.DataCaptionExpression, DataTypes.Text)
		{
			AttributeId = 21039
		},
		new PropertyUsage(ElementType.Page, PropertyType.DataCaptionFields, DataTypes.Text)
		{
			AttributeId = 21105
		},
		new PropertyUsage(ElementType.Page, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.Page, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.Page, PropertyType.SourceTableView, DataTypes.Text)
		{
			AttributeId = 21103
		},
		new PropertyUsage(ElementType.Page, PropertyType.PageType, DataTypes.Option)
		{
			AttributeId = 55066
		},
		new PropertyUsage(ElementType.Page, PropertyType.InstructionalTextML, DataTypes.MLText)
		{
			AttributeId = 55119
		},
		new PropertyUsage(ElementType.Page, PropertyType.ActionList, DataTypes.Container)
		{
			AttributeId = 55243,
			ContainerName = "ACTIONS",
			ContainerChildElementType = ElementType.Action
		},
		new PropertyUsage(ElementType.Page, PropertyType.PromotedActionCategoriesML, DataTypes.MLText)
		{
			AttributeId = 55132
		},
		new PropertyUsage(ElementType.Page, PropertyType.AdditionalSearchTermsML, DataTypes.MLText)
		{
			AttributeId = 55074
		},
		new PropertyUsage(ElementType.Page, PropertyType.QueryCategory, DataTypes.Text)
		{
			AttributeId = 57607
		},
		new PropertyUsage(ElementType.Page, PropertyType.CardFormId, DataTypes.FormId)
		{
			AttributeId = 55091
		},
		new PropertyUsage(ElementType.Page, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Page, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Page, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Page, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.Page, PropertyType.UsageCategory, DataTypes.Option)
		{
			AttributeId = 55071
		},
		new PropertyUsage(ElementType.Page, PropertyType.AccessByPermission, DataTypes.Text)
		{
			AttributeId = 5011
		},
		new PropertyUsage(ElementType.Page, PropertyType.ApplicationArea, DataTypes.Text)
		{
			AttributeId = 10346
		},
		new PropertyUsage(ElementType.Page, PropertyType.ODataKeyFields, DataTypes.Text)
		{
			AttributeId = 55176
		},
		new PropertyUsage(ElementType.Page, PropertyType.APIVersion, DataTypes.Text)
		{
			AttributeId = 55178
		},
		new PropertyUsage(ElementType.Page, PropertyType.EntitySetName, DataTypes.Text)
		{
			AttributeId = 55086
		},
		new PropertyUsage(ElementType.Page, PropertyType.EntityName, DataTypes.Text)
		{
			AttributeId = 55087
		},
		new PropertyUsage(ElementType.Page, PropertyType.APIGroup, DataTypes.Text)
		{
			AttributeId = 55085
		},
		new PropertyUsage(ElementType.Page, PropertyType.APIPublisher, DataTypes.Text)
		{
			AttributeId = 55084
		},
		new PropertyUsage(ElementType.Page, PropertyType.ChangeTrackingAllowed, DataTypes.Boolean)
		{
			AttributeId = 55083
		},
		new PropertyUsage(ElementType.Form, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Form, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Form, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Form, PropertyType.ActiveControlOnOpen, DataTypes.Integer),
		new PropertyUsage(ElementType.Form, PropertyType.Height, DataTypes.Integer),
		new PropertyUsage(ElementType.Form, PropertyType.SourceTable, DataTypes.ObjectId)
		{
			AttributeId = 21029
		},
		new PropertyUsage(ElementType.Form, PropertyType.TableBoxId, DataTypes.ObjectId),
		new PropertyUsage(ElementType.Form, PropertyType.TimerInterval, DataTypes.Integer),
		new PropertyUsage(ElementType.Form, PropertyType.Width, DataTypes.Integer)
		{
			AttributeId = 5010
		},
		new PropertyUsage(ElementType.Form, PropertyType.AutoSplitKey, DataTypes.Boolean)
		{
			AttributeId = 55120
		},
		new PropertyUsage(ElementType.Form, PropertyType.DelayedInsert, DataTypes.Boolean)
		{
			AttributeId = 21044
		},
		new PropertyUsage(ElementType.Form, PropertyType.DeleteAllowed, DataTypes.Boolean)
		{
			AttributeId = 15412
		},
		new PropertyUsage(ElementType.Form, PropertyType.Editable, DataTypes.Boolean)
		{
			AttributeId = 55098
		},
		new PropertyUsage(ElementType.Form, PropertyType.InsertAllowed, DataTypes.Boolean)
		{
			AttributeId = 15411
		},
		new PropertyUsage(ElementType.Form, PropertyType.LinksAllowed, DataTypes.Boolean)
		{
			AttributeId = 15424
		},
		new PropertyUsage(ElementType.Form, PropertyType.LookupMode, DataTypes.Boolean),
		new PropertyUsage(ElementType.Form, PropertyType.Maximizable, DataTypes.Boolean),
		new PropertyUsage(ElementType.Form, PropertyType.Minimizable, DataTypes.Boolean),
		new PropertyUsage(ElementType.Form, PropertyType.ModifyAllowed, DataTypes.Boolean)
		{
			AttributeId = 15413
		},
		new PropertyUsage(ElementType.Form, PropertyType.MultipleNewLines, DataTypes.Boolean)
		{
			AttributeId = 15410
		},
		new PropertyUsage(ElementType.Form, PropertyType.PopulateAllFields, DataTypes.Boolean)
		{
			AttributeId = 21045
		},
		new PropertyUsage(ElementType.Form, PropertyType.SavePosAndSize, DataTypes.Boolean),
		new PropertyUsage(ElementType.Form, PropertyType.SaveTableView, DataTypes.Boolean),
		new PropertyUsage(ElementType.Form, PropertyType.SaveValues, DataTypes.Boolean)
		{
			AttributeId = 15395
		},
		new PropertyUsage(ElementType.Form, PropertyType.Sizeable, DataTypes.Boolean),
		new PropertyUsage(ElementType.Form, PropertyType.SourceTableTemporary, DataTypes.Boolean)
		{
			AttributeId = 55078
		},
		new PropertyUsage(ElementType.Form, PropertyType.UpdateOnActivate, DataTypes.Boolean),
		new PropertyUsage(ElementType.Form, PropertyType.CalcFields, DataTypes.Text),
		new PropertyUsage(ElementType.Form, PropertyType.DataCaptionExpression, DataTypes.Text)
		{
			AttributeId = 21039
		},
		new PropertyUsage(ElementType.Form, PropertyType.DataCaptionFields, DataTypes.Text)
		{
			AttributeId = 21105
		},
		new PropertyUsage(ElementType.Form, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.Form, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.Form, PropertyType.SourceTableView, DataTypes.Text)
		{
			AttributeId = 21103
		},
		new PropertyUsage(ElementType.Form, PropertyType.AutoPosition, DataTypes.Option),
		new PropertyUsage(ElementType.Form, PropertyType.BorderStyle, DataTypes.Option),
		new PropertyUsage(ElementType.Form, PropertyType.CaptionBar, DataTypes.Option),
		new PropertyUsage(ElementType.Form, PropertyType.SourceTablePlacement, DataTypes.Option),
		new PropertyUsage(ElementType.Form, PropertyType.BackColor, DataTypes.Integer)
		{
			AttributeId = 8528
		},
		new PropertyUsage(ElementType.Form, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Form, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Form, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Form, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.Dataport, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Dataport, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Dataport, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Dataport, PropertyType.Import, DataTypes.Boolean),
		new PropertyUsage(ElementType.Dataport, PropertyType.UseReqForm, DataTypes.Boolean)
		{
			AttributeId = 18454
		},
		new PropertyUsage(ElementType.Dataport, PropertyType.DataItemSeparator, DataTypes.Text),
		new PropertyUsage(ElementType.Dataport, PropertyType.FieldEndDelimiter, DataTypes.Text),
		new PropertyUsage(ElementType.Dataport, PropertyType.FieldSeparator, DataTypes.Text)
		{
			AttributeId = 54168
		},
		new PropertyUsage(ElementType.Dataport, PropertyType.FieldStartDelimiter, DataTypes.Text),
		new PropertyUsage(ElementType.Dataport, PropertyType.FileFormat, DataTypes.Option),
		new PropertyUsage(ElementType.Dataport, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Dataport, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Dataport, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Dataport, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.MenuNode, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.MenuNodeId, DataTypes.Guid),
		new PropertyUsage(ElementType.MenuNode, PropertyType.RunObjectId, DataTypes.ObjectId)
		{
			AttributeId = 56602
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.Deleted, DataTypes.Boolean)
		{
			AttributeId = 56615
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.Enabled, DataTypes.Boolean)
		{
			AttributeId = 56612
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.IsDepartmentPage, DataTypes.Boolean)
		{
			AttributeId = 56620
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.IsShortcut, DataTypes.Boolean)
		{
			AttributeId = 56610
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.Visible, DataTypes.Boolean)
		{
			AttributeId = 56611
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.FirstChild, DataTypes.Text)
		{
			AttributeId = 56614
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.Image, DataTypes.Text)
		{
			AttributeId = 56608
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.MemberOfMenu, DataTypes.Text)
		{
			AttributeId = 56599
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.NextNodeId, DataTypes.Guid)
		{
			AttributeId = 56613
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.ParentNodeId, DataTypes.Guid)
		{
			AttributeId = 56603
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.DepartmentCategory, DataTypes.Option)
		{
			AttributeId = 56619
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.MenuNodeType, DataTypes.Option)
		{
			AttributeId = 56598
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.RunObjectType, DataTypes.Option)
		{
			AttributeId = 56601
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.AccessByPermission, DataTypes.Text)
		{
			AttributeId = 5011
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.VisibleInDesign, DataTypes.Boolean)
		{
			AttributeId = 56616
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.ApplicationArea, DataTypes.Text)
		{
			AttributeId = 10346
		},
		new PropertyUsage(ElementType.DataportItem, PropertyType.DataItemTable, DataTypes.Text)
		{
			AttributeId = 18474
		},
		new PropertyUsage(ElementType.DataportItem, PropertyType.DataItemVarName, DataTypes.Text),
		new PropertyUsage(ElementType.DataportItem, PropertyType.AutoReplace, DataTypes.Boolean),
		new PropertyUsage(ElementType.DataportItem, PropertyType.AutoSave, DataTypes.Boolean),
		new PropertyUsage(ElementType.DataportItem, PropertyType.AutoUpdate, DataTypes.Boolean),
		new PropertyUsage(ElementType.DataportItem, PropertyType.DataItemTableView, DataTypes.Text)
		{
			AttributeId = 18475
		},
		new PropertyUsage(ElementType.DataportItem, PropertyType.RequestFilterFields, DataTypes.Text)
		{
			AttributeId = 18482
		},
		new PropertyUsage(ElementType.DataportField, PropertyType.SourceExpression, DataTypes.Text)
		{
			AttributeId = 21052
		},
		new PropertyUsage(ElementType.DataportField, PropertyType.StartPos, DataTypes.Integer),
		new PropertyUsage(ElementType.DataportField, PropertyType.Width, DataTypes.Integer)
		{
			AttributeId = 5010
		},
		new PropertyUsage(ElementType.DataportField, PropertyType.Enabled, DataTypes.Boolean),
		new PropertyUsage(ElementType.RequestForm, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.RequestForm, PropertyType.Height, DataTypes.Integer),
		new PropertyUsage(ElementType.RequestForm, PropertyType.SourceTable, DataTypes.ObjectId)
		{
			AttributeId = 21029
		},
		new PropertyUsage(ElementType.RequestForm, PropertyType.Width, DataTypes.Integer)
		{
			AttributeId = 5010
		},
		new PropertyUsage(ElementType.RequestForm, PropertyType.InsertAllowed, DataTypes.Boolean)
		{
			AttributeId = 15411
		},
		new PropertyUsage(ElementType.RequestForm, PropertyType.LookupMode, DataTypes.Boolean),
		new PropertyUsage(ElementType.RequestForm, PropertyType.SaveValues, DataTypes.Boolean)
		{
			AttributeId = 15395
		},
		new PropertyUsage(ElementType.RequestForm, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.MenuItemId, DataTypes.ObjectId),
		new PropertyUsage(ElementType.MenuItem, PropertyType.MenuLevel, DataTypes.Integer),
		new PropertyUsage(ElementType.MenuItem, PropertyType.Ellipsis, DataTypes.Boolean)
		{
			AttributeId = 8561
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.Enabled, DataTypes.Boolean),
		new PropertyUsage(ElementType.MenuItem, PropertyType.UpdateOnAction, DataTypes.Boolean),
		new PropertyUsage(ElementType.MenuItem, PropertyType.Visible, DataTypes.Boolean),
		new PropertyUsage(ElementType.MenuItem, PropertyType.RunFormLink, DataTypes.Text)
		{
			AttributeId = 21116
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.RunFormOnRec, DataTypes.Boolean)
		{
			AttributeId = 21072
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.RunFormView, DataTypes.Text)
		{
			AttributeId = 21115
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.RunObject, DataTypes.Text)
		{
			AttributeId = 21058
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.ShortcutKey, DataTypes.Text)
		{
			AttributeId = 8560
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.MenuItemType, DataTypes.Option),
		new PropertyUsage(ElementType.MenuItem, PropertyType.PushAction, DataTypes.Option),
		new PropertyUsage(ElementType.MenuItem, PropertyType.RunFormLinkType, DataTypes.Option),
		new PropertyUsage(ElementType.MenuItem, PropertyType.ToolTipML, DataTypes.MLText)
		{
			AttributeId = 8631
		},
		new PropertyUsage(ElementType.MenuItem, PropertyType.RunCommand, DataTypes.Text),
		new PropertyUsage(ElementType.Control, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Control, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Control, PropertyType.Level, DataTypes.Integer),
		new PropertyUsage(ElementType.Control, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Control, PropertyType.SourceExpression, DataTypes.Text)
		{
			AttributeId = 21052
		},
		new PropertyUsage(ElementType.Control, PropertyType.SystemPartId, DataTypes.PartType)
		{
			AttributeId = 55107
		},
		new PropertyUsage(ElementType.Control, PropertyType.PagePartId, DataTypes.PageId)
		{
			AttributeId = 55070
		},
		new PropertyUsage(ElementType.Control, PropertyType.ContainerType, DataTypes.Text)
		{
			AttributeId = 55065
		},
		new PropertyUsage(ElementType.Control, PropertyType.ControlType, DataTypes.Option)
		{
			AttributeId = 55040
		},
		new PropertyUsage(ElementType.Control, PropertyType.DecimalPlaces, DataTypes.DecimalPlaces)
		{
			AttributeId = 12816
		},
		new PropertyUsage(ElementType.Control, PropertyType.DrillDownPageId, DataTypes.PageId)
		{
			AttributeId = 55117
		},
		new PropertyUsage(ElementType.Control, PropertyType.LookupPageId, DataTypes.PageId)
		{
			AttributeId = 55116
		},
		new PropertyUsage(ElementType.Control, PropertyType.MaxValue, DataTypes.Text)
		{
			AttributeId = 21093
		},
		new PropertyUsage(ElementType.Control, PropertyType.MinValue, DataTypes.Text)
		{
			AttributeId = 21092
		},
		new PropertyUsage(ElementType.Control, PropertyType.ProviderId, DataTypes.ObjectId)
		{
			AttributeId = 55082
		},
		new PropertyUsage(ElementType.Control, PropertyType.Width, DataTypes.Integer)
		{
			AttributeId = 5010
		},
		new PropertyUsage(ElementType.Control, PropertyType.AssistEdit, DataTypes.Boolean)
		{
			AttributeId = 8586
		},
		new PropertyUsage(ElementType.Control, PropertyType.BlankNumbers, DataTypes.Option)
		{
			AttributeId = 15376
		},
		new PropertyUsage(ElementType.Control, PropertyType.BlankZero, DataTypes.Boolean)
		{
			AttributeId = 15377
		},
		new PropertyUsage(ElementType.Control, PropertyType.DrillDown, DataTypes.Boolean)
		{
			AttributeId = 8585
		},
		new PropertyUsage(ElementType.Control, PropertyType.Editable, DataTypes.Boolean)
		{
			AttributeId = 55098
		},
		new PropertyUsage(ElementType.Control, PropertyType.Enabled, DataTypes.Boolean)
		{
			AttributeId = 55097
		},
		new PropertyUsage(ElementType.Control, PropertyType.HideValue, DataTypes.Boolean)
		{
			AttributeId = 55126
		},
		new PropertyUsage(ElementType.Control, PropertyType.Lookup, DataTypes.Boolean)
		{
			AttributeId = 8584
		},
		new PropertyUsage(ElementType.Control, PropertyType.Multiline, DataTypes.Boolean)
		{
			AttributeId = 55121
		},
		new PropertyUsage(ElementType.Control, PropertyType.NotBlank, DataTypes.Boolean)
		{
			AttributeId = 15369
		},
		new PropertyUsage(ElementType.Control, PropertyType.Numeric, DataTypes.Boolean)
		{
			AttributeId = 15370
		},
		new PropertyUsage(ElementType.Control, PropertyType.ShowAsTree, DataTypes.Boolean)
		{
			AttributeId = 55103
		},
		new PropertyUsage(ElementType.Control, PropertyType.ShowCaption, DataTypes.Boolean)
		{
			AttributeId = 55290
		},
		new PropertyUsage(ElementType.Control, PropertyType.ShowFilter, DataTypes.Boolean)
		{
			AttributeId = 55136
		},
		new PropertyUsage(ElementType.Control, PropertyType.Visible, DataTypes.Boolean)
		{
			AttributeId = 55096
		},
		new PropertyUsage(ElementType.Control, PropertyType.AutoFormatExpression, DataTypes.Text)
		{
			AttributeId = 21078
		},
		new PropertyUsage(ElementType.Control, PropertyType.AutoFormatType, DataTypes.Text)
		{
			AttributeId = 21076
		},
		new PropertyUsage(ElementType.Control, PropertyType.CaptionClass, DataTypes.Text)
		{
			AttributeId = 21080
		},
		new PropertyUsage(ElementType.Control, PropertyType.ChartPartId, DataTypes.Text)
		{
			AttributeId = 55108
		},
		new PropertyUsage(ElementType.Control, PropertyType.ClosingDates, DataTypes.Boolean)
		{
			AttributeId = 15373
		},
		new PropertyUsage(ElementType.Control, PropertyType.ControlAddIn, DataTypes.Text)
		{
			AttributeId = 55135
		},
		new PropertyUsage(ElementType.Control, PropertyType.DateFormula, DataTypes.Boolean)
		{
			AttributeId = 15372
		},
		new PropertyUsage(ElementType.Control, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.Control, PropertyType.ExtendedDataType, DataTypes.Text)
		{
			AttributeId = 5009
		},
		new PropertyUsage(ElementType.Control, PropertyType.FreezeColumnId, DataTypes.Text)
		{
			AttributeId = 55124
		},
		new PropertyUsage(ElementType.Control, PropertyType.GroupType, DataTypes.Option)
		{
			AttributeId = 55109
		},
		new PropertyUsage(ElementType.Control, PropertyType.Image, DataTypes.Text)
		{
			AttributeId = 55287
		},
		new PropertyUsage(ElementType.Control, PropertyType.IndentationColumnName, DataTypes.Text)
		{
			AttributeId = 55101
		},
		new PropertyUsage(ElementType.Control, PropertyType.IndentationControls, DataTypes.Text)
		{
			AttributeId = 55102
		},
		new PropertyUsage(ElementType.Control, PropertyType.PartType, DataTypes.Text)
		{
			AttributeId = 55100
		},
		new PropertyUsage(ElementType.Control, PropertyType.StyleExpression, DataTypes.Text)
		{
			AttributeId = 55134
		},
		new PropertyUsage(ElementType.Control, PropertyType.SubFormLink, DataTypes.Text)
		{
			AttributeId = 21056
		},
		new PropertyUsage(ElementType.Control, PropertyType.SubFormView, DataTypes.Text)
		{
			AttributeId = 21055
		},
		new PropertyUsage(ElementType.Control, PropertyType.TableRelation, DataTypes.Text)
		{
			AttributeId = 21114
		},
		new PropertyUsage(ElementType.Control, PropertyType.ValuesAllowed, DataTypes.Text)
		{
			AttributeId = 21106
		},
		new PropertyUsage(ElementType.Control, PropertyType.Importance, DataTypes.Option)
		{
			AttributeId = 55088
		},
		new PropertyUsage(ElementType.Control, PropertyType.Style, DataTypes.Option)
		{
			AttributeId = 55133
		},
		new PropertyUsage(ElementType.Control, PropertyType.InstructionalTextML, DataTypes.MLText)
		{
			AttributeId = 55119
		},
		new PropertyUsage(ElementType.Control, PropertyType.OptionCaptionML, DataTypes.MLText)
		{
			AttributeId = 8632
		},
		new PropertyUsage(ElementType.Control, PropertyType.ToolTipML, DataTypes.MLText)
		{
			AttributeId = 8631
		},
		new PropertyUsage(ElementType.Control, PropertyType.ActionList, DataTypes.Container)
		{
			AttributeId = 55243,
			ContainerName = "ACTIONS",
			ContainerChildElementType = ElementType.Action
		},
		new PropertyUsage(ElementType.Control, PropertyType.CharAllowed, DataTypes.Text)
		{
			AttributeId = 15371
		},
		new PropertyUsage(ElementType.Control, PropertyType.Layout, DataTypes.Text)
		{
			AttributeId = 55137
		},
		new PropertyUsage(ElementType.Control, PropertyType.BackColor, DataTypes.Integer)
		{
			AttributeId = 8528
		},
		new PropertyUsage(ElementType.Control, PropertyType.DrillDownFormId, DataTypes.FormId)
		{
			AttributeId = 21069
		},
		new PropertyUsage(ElementType.Control, PropertyType.LookupFormId, DataTypes.FormId)
		{
			AttributeId = 21068
		},
		new PropertyUsage(ElementType.Control, PropertyType.QuickEntry, DataTypes.Text)
		{
			AttributeId = 55295
		},
		new PropertyUsage(ElementType.Control, PropertyType.RowSpan, DataTypes.Integer)
		{
			AttributeId = 55138
		},
		new PropertyUsage(ElementType.Control, PropertyType.ColumnSpan, DataTypes.Integer)
		{
			AttributeId = 55139
		},
		new PropertyUsage(ElementType.Control, PropertyType.SubpageLink, DataTypes.Text)
		{
			AttributeId = 21113
		},
		new PropertyUsage(ElementType.Control, PropertyType.SubpageView, DataTypes.Text)
		{
			AttributeId = 21112
		},
		new PropertyUsage(ElementType.Control, PropertyType.AccessByPermission, DataTypes.Text)
		{
			AttributeId = 5011
		},
		new PropertyUsage(ElementType.Control, PropertyType.ShowMandatory, DataTypes.Boolean)
		{
			AttributeId = 55288
		},
		new PropertyUsage(ElementType.Control, PropertyType.UpdatePropagation, DataTypes.Option)
		{
			AttributeId = 55181
		},
		new PropertyUsage(ElementType.Control, PropertyType.ApplicationArea, DataTypes.Text)
		{
			AttributeId = 10346
		},
		new PropertyUsage(ElementType.Control, PropertyType.ODataEdmType, DataTypes.Text)
		{
			AttributeId = 55175
		},
		new PropertyUsage(ElementType.Control, PropertyType.ImplicitType, DataTypes.Text)
		{
			AttributeId = 55179
		},
		new PropertyUsage(ElementType.Control, PropertyType.EntitySetName, DataTypes.Text)
		{
			AttributeId = 55086
		},
		new PropertyUsage(ElementType.Control, PropertyType.EntityName, DataTypes.Text)
		{
			AttributeId = 55087
		},
		new PropertyUsage(ElementType.Action, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Action, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Action, PropertyType.Level, DataTypes.Integer),
		new PropertyUsage(ElementType.Action, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Action, PropertyType.ActionContainerType, DataTypes.Option)
		{
			AttributeId = 55068
		},
		new PropertyUsage(ElementType.Action, PropertyType.ActionType, DataTypes.Option)
		{
			AttributeId = 55093
		},
		new PropertyUsage(ElementType.Action, PropertyType.Ellipsis, DataTypes.Boolean)
		{
			AttributeId = 8561
		},
		new PropertyUsage(ElementType.Action, PropertyType.Enabled, DataTypes.Boolean)
		{
			AttributeId = 55097
		},
		new PropertyUsage(ElementType.Action, PropertyType.InFooterBar, DataTypes.Boolean)
		{
			AttributeId = 55104
		},
		new PropertyUsage(ElementType.Action, PropertyType.IsHeader, DataTypes.Boolean)
		{
			AttributeId = 55128
		},
		new PropertyUsage(ElementType.Action, PropertyType.Promoted, DataTypes.Boolean)
		{
			AttributeId = 55092
		},
		new PropertyUsage(ElementType.Action, PropertyType.PromotedIsBig, DataTypes.Boolean)
		{
			AttributeId = 55105
		},
		new PropertyUsage(ElementType.Action, PropertyType.Visible, DataTypes.Boolean)
		{
			AttributeId = 55096
		},
		new PropertyUsage(ElementType.Action, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.Action, PropertyType.Image, DataTypes.Text)
		{
			AttributeId = 55111
		},
		new PropertyUsage(ElementType.Action, PropertyType.PromotedCategory, DataTypes.Text)
		{
			AttributeId = 55112
		},
		new PropertyUsage(ElementType.Action, PropertyType.RunFormLink, DataTypes.Text)
		{
			AttributeId = 21116
		},
		new PropertyUsage(ElementType.Action, PropertyType.RunFormMode, DataTypes.Option),
		new PropertyUsage(ElementType.Action, PropertyType.RunFormOnRec, DataTypes.Boolean)
		{
			AttributeId = 21072
		},
		new PropertyUsage(ElementType.Action, PropertyType.RunFormView, DataTypes.Text)
		{
			AttributeId = 21115
		},
		new PropertyUsage(ElementType.Action, PropertyType.RunObject, DataTypes.Text)
		{
			AttributeId = 21058
		},
		new PropertyUsage(ElementType.Action, PropertyType.ShortcutKey, DataTypes.Text)
		{
			AttributeId = 8560
		},
		new PropertyUsage(ElementType.Action, PropertyType.ToolTipML, DataTypes.MLText)
		{
			AttributeId = 8631
		},
		new PropertyUsage(ElementType.Action, PropertyType.RunPageLink, DataTypes.Text)
		{
			AttributeId = 21116
		},
		new PropertyUsage(ElementType.Action, PropertyType.RunPageMode, DataTypes.Option)
		{
			AttributeId = 55127
		},
		new PropertyUsage(ElementType.Action, PropertyType.RunPageOnRec, DataTypes.Boolean)
		{
			AttributeId = 21072
		},
		new PropertyUsage(ElementType.Action, PropertyType.RunPageView, DataTypes.Text)
		{
			AttributeId = 21115
		},
		new PropertyUsage(ElementType.Action, PropertyType.AccessByPermission, DataTypes.Text)
		{
			AttributeId = 5011
		},
		new PropertyUsage(ElementType.Action, PropertyType.Scope, DataTypes.Option)
		{
			AttributeId = 55159
		},
		new PropertyUsage(ElementType.Action, PropertyType.ApplicationArea, DataTypes.Text)
		{
			AttributeId = 10346
		},
		new PropertyUsage(ElementType.Action, PropertyType.PromotedOnly, DataTypes.Boolean)
		{
			AttributeId = 55113
		},
		new PropertyUsage(ElementType.Action, PropertyType.Gesture, DataTypes.Option)
		{
			AttributeId = 55182
		},
		new PropertyUsage(ElementType.Field, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Field, PropertyType.FieldNo, DataTypes.Integer)
		{
			AttributeId = 42340
		},
		new PropertyUsage(ElementType.Field, PropertyType.DataType, DataTypes.Option),
		new PropertyUsage(ElementType.Field, PropertyType.OptionString, DataTypes.Text)
		{
			AttributeId = 10340
		},
		new PropertyUsage(ElementType.Field, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Field, PropertyType.DecimalPlaces, DataTypes.DecimalPlaces)
		{
			AttributeId = 12816
		},
		new PropertyUsage(ElementType.Field, PropertyType.MaxValue, DataTypes.Text)
		{
			AttributeId = 15366
		},
		new PropertyUsage(ElementType.Field, PropertyType.MinValue, DataTypes.Text)
		{
			AttributeId = 15365
		},
		new PropertyUsage(ElementType.Field, PropertyType.Width, DataTypes.Integer)
		{
			AttributeId = 5010
		},
		new PropertyUsage(ElementType.Field, PropertyType.AutoIncrement, DataTypes.Boolean)
		{
			AttributeId = 4990
		},
		new PropertyUsage(ElementType.Field, PropertyType.BlankNumbers, DataTypes.Option)
		{
			AttributeId = 15376
		},
		new PropertyUsage(ElementType.Field, PropertyType.BlankZero, DataTypes.Boolean)
		{
			AttributeId = 15377
		},
		new PropertyUsage(ElementType.Field, PropertyType.Compressed, DataTypes.Boolean)
		{
			AttributeId = 4987
		},
		new PropertyUsage(ElementType.Field, PropertyType.Editable, DataTypes.Boolean)
		{
			AttributeId = 15393
		},
		new PropertyUsage(ElementType.Field, PropertyType.Enabled, DataTypes.Boolean)
		{
			AttributeId = 4919
		},
		new PropertyUsage(ElementType.Field, PropertyType.NotBlank, DataTypes.Boolean)
		{
			AttributeId = 15369
		},
		new PropertyUsage(ElementType.Field, PropertyType.Numeric, DataTypes.Boolean)
		{
			AttributeId = 15370
		},
		new PropertyUsage(ElementType.Field, PropertyType.TestTableRelation, DataTypes.Boolean)
		{
			AttributeId = 4970
		},
		new PropertyUsage(ElementType.Field, PropertyType.ValidateTableRelation, DataTypes.Boolean)
		{
			AttributeId = 4969
		},
		new PropertyUsage(ElementType.Field, PropertyType.Volatile, DataTypes.Boolean)
		{
			AttributeId = 4991
		},
		new PropertyUsage(ElementType.Field, PropertyType.AltSearchField, DataTypes.Text)
		{
			AttributeId = 4936
		},
		new PropertyUsage(ElementType.Field, PropertyType.AutoFormatExpression, DataTypes.Text)
		{
			AttributeId = 42441
		},
		new PropertyUsage(ElementType.Field, PropertyType.AutoFormatType, DataTypes.Text)
		{
			AttributeId = 42440
		},
		new PropertyUsage(ElementType.Field, PropertyType.CalcFormula, DataTypes.Text)
		{
			AttributeId = 4918
		},
		new PropertyUsage(ElementType.Field, PropertyType.CaptionClass, DataTypes.Text)
		{
			AttributeId = 42444
		},
		new PropertyUsage(ElementType.Field, PropertyType.ClosingDates, DataTypes.Boolean)
		{
			AttributeId = 15373
		},
		new PropertyUsage(ElementType.Field, PropertyType.DateFormula, DataTypes.Boolean)
		{
			AttributeId = 15372
		},
		new PropertyUsage(ElementType.Field, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.Field, PropertyType.ExtendedDataType, DataTypes.Text)
		{
			AttributeId = 5009
		},
		new PropertyUsage(ElementType.Field, PropertyType.Image, DataTypes.Text)
		{
			AttributeId = 55287
		},
		new PropertyUsage(ElementType.Field, PropertyType.InitValue, DataTypes.Text)
		{
			AttributeId = 4917
		},
		new PropertyUsage(ElementType.Field, PropertyType.TableIdExpression, DataTypes.Text)
		{
			AttributeId = 4985
		},
		new PropertyUsage(ElementType.Field, PropertyType.TableRelation, DataTypes.Text)
		{
			AttributeId = 4923
		},
		new PropertyUsage(ElementType.Field, PropertyType.ValuesAllowed, DataTypes.Text)
		{
			AttributeId = 15381
		},
		new PropertyUsage(ElementType.Field, PropertyType.FieldClass, DataTypes.Option)
		{
			AttributeId = 4915
		},
		new PropertyUsage(ElementType.Field, PropertyType.BlobSubtype, DataTypes.BlobType)
		{
			AttributeId = 15389
		},
		new PropertyUsage(ElementType.Field, PropertyType.OptionCaptionML, DataTypes.MLText)
		{
			AttributeId = 8632
		},
		new PropertyUsage(ElementType.Field, PropertyType.CharAllowed, DataTypes.Text)
		{
			AttributeId = 15371
		},
		new PropertyUsage(ElementType.Field, PropertyType.SqlDataType, DataTypes.Text)
		{
			AttributeId = 10350
		},
		new PropertyUsage(ElementType.Field, PropertyType.Owner, DataTypes.Text)
		{
			AttributeId = 15388
		},
		new PropertyUsage(ElementType.Field, PropertyType.Title, DataTypes.Boolean)
		{
			AttributeId = 15374
		},
		new PropertyUsage(ElementType.Field, PropertyType.AccessByPermission, DataTypes.Text)
		{
			AttributeId = 5011
		},
		new PropertyUsage(ElementType.Field, PropertyType.SignDisplacement, DataTypes.Integer),
		new PropertyUsage(ElementType.Field, PropertyType.ExternalName, DataTypes.Text)
		{
			AttributeId = 5045
		},
		new PropertyUsage(ElementType.Field, PropertyType.ExternalType, DataTypes.Text)
		{
			AttributeId = 5046
		},
		new PropertyUsage(ElementType.Field, PropertyType.ExternalAccess, DataTypes.ExternalAccess)
		{
			AttributeId = 5047
		},
		new PropertyUsage(ElementType.Field, PropertyType.SqlTimestamp, DataTypes.Boolean)
		{
			AttributeId = 4995
		},
		new PropertyUsage(ElementType.Field, PropertyType.OptionOrdinalValues, DataTypes.Boolean)
		{
			AttributeId = 5050
		},
		new PropertyUsage(ElementType.Field, PropertyType.StandardDayTimeUnit, DataTypes.Option)
		{
			AttributeId = 36914
		},
		new PropertyUsage(ElementType.Field, PropertyType.ObsoleteState, DataTypes.Option)
		{
			AttributeId = 5052
		},
		new PropertyUsage(ElementType.Field, PropertyType.ObsoleteReason, DataTypes.Text)
		{
			AttributeId = 5053
		},
		new PropertyUsage(ElementType.Field, PropertyType.DataClassification, DataTypes.Option)
		{
			AttributeId = 5056
		},
		new PropertyUsage(ElementType.Field, PropertyType.ReplicateData, DataTypes.Boolean)
		{
			AttributeId = 5057
		},
		new PropertyUsage(ElementType.Field, PropertyType.Extensible, DataTypes.Boolean)
		{
			AttributeId = 5058
		},
		new PropertyUsage(ElementType.Field, PropertyType.EnumTypeId, DataTypes.Integer)
		{
			AttributeId = 5059
		},
		new PropertyUsage(ElementType.Field, PropertyType.EnumTypeName, DataTypes.Text)
		{
			AttributeId = 5060
		},
		new PropertyUsage(ElementType.Key, PropertyType.Key, DataTypes.Text)
		{
			AttributeId = 4929
		},
		new PropertyUsage(ElementType.Key, PropertyType.Clustered, DataTypes.Boolean)
		{
			AttributeId = 4994
		},
		new PropertyUsage(ElementType.Key, PropertyType.Enabled, DataTypes.Boolean)
		{
			AttributeId = 4932
		},
		new PropertyUsage(ElementType.Key, PropertyType.MaintainSiftIndex, DataTypes.Boolean)
		{
			AttributeId = 4981
		},
		new PropertyUsage(ElementType.Key, PropertyType.MaintainSqlIndex, DataTypes.Boolean)
		{
			AttributeId = 4979
		},
		new PropertyUsage(ElementType.Key, PropertyType.KeyGroups, DataTypes.Text)
		{
			AttributeId = 4931
		},
		new PropertyUsage(ElementType.Key, PropertyType.SiftLevelsToMaintain, DataTypes.Text)
		{
			AttributeId = 4988
		},
		new PropertyUsage(ElementType.Key, PropertyType.SqlIndex, DataTypes.Text)
		{
			AttributeId = 4996
		},
		new PropertyUsage(ElementType.Key, PropertyType.SumIndexFields, DataTypes.Text)
		{
			AttributeId = 4930
		},
		new PropertyUsage(ElementType.Key, PropertyType.ObsoleteState, DataTypes.Option)
		{
			AttributeId = 5052
		},
		new PropertyUsage(ElementType.Key, PropertyType.ObsoleteReason, DataTypes.Text)
		{
			AttributeId = 5053
		},
		new PropertyUsage(ElementType.Key, PropertyType.Unique, DataTypes.Boolean)
		{
			AttributeId = 4982
		},
		new PropertyUsage(ElementType.FieldGroup, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.FieldGroup, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.FieldGroup, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.FieldGroup, PropertyType.Fields, DataTypes.Text),
		new PropertyUsage(ElementType.FormControl, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.OptionString, DataTypes.Text)
		{
			AttributeId = 10340
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.SourceExpression, DataTypes.Text)
		{
			AttributeId = 21052
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.ControlType, DataTypes.Option)
		{
			AttributeId = 55040
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.BorderColor, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.DecimalPlaces, DataTypes.DecimalPlaces)
		{
			AttributeId = 12816
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Divisor, DataTypes.Integer)
		{
			AttributeId = 15394
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.DrillDownPageId, DataTypes.PageId)
		{
			AttributeId = 55117
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.FontSize, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.ForeColor, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.ControlHeight, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.HeadingHeight, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.LookupPageId, DataTypes.PageId)
		{
			AttributeId = 55116
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.MatrixColumnWidth, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.MatrixSourceTable, DataTypes.ObjectId),
		new PropertyUsage(ElementType.FormControl, PropertyType.MaxLength, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.MaxValue, DataTypes.Text)
		{
			AttributeId = 21093
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.MinValue, DataTypes.Text)
		{
			AttributeId = 21092
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.NextControl, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.ParentControl, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.RowHeight, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.SubFormId, DataTypes.PageId),
		new PropertyUsage(ElementType.FormControl, PropertyType.ControlWidth, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.X, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.Y, DataTypes.Integer),
		new PropertyUsage(ElementType.FormControl, PropertyType.AssistEdit, DataTypes.Boolean)
		{
			AttributeId = 8586
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.AutoCalcField, DataTypes.Boolean)
		{
			AttributeId = 21057
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.AutoEnter, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.AutoRepeat, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.BackTransparent, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.BlankNumbers, DataTypes.Option)
		{
			AttributeId = 15376
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.BlankZero, DataTypes.Boolean)
		{
			AttributeId = 15377
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Cancel, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.ClearOnLookup, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.Default, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.DrillDown, DataTypes.Boolean)
		{
			AttributeId = 8585
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.DropDown, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.Editable, DataTypes.Boolean)
		{
			AttributeId = 55098
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Ellipsis, DataTypes.Boolean)
		{
			AttributeId = 8561
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Enabled, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.Focusable, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.FocusOnClick, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.FontBold, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.FontItalic, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.InColumn, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.InColumnHeading, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.InFrame, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.InlineEditing, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.InMatrix, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.InMatrixHeading, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.InPage, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.LeaderDots, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.Lookup, DataTypes.Boolean)
		{
			AttributeId = 8584
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Multiline, DataTypes.Boolean)
		{
			AttributeId = 55121
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.NotBlank, DataTypes.Boolean)
		{
			AttributeId = 15369
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.PasswordText, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.PermanentAssist, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.ShowCaption, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.TopLineOnly, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.UpdateOnAction, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.ValidateTableRelation, DataTypes.Boolean)
		{
			AttributeId = 4969
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Visible, DataTypes.Boolean),
		new PropertyUsage(ElementType.FormControl, PropertyType.AutoFormatExpression, DataTypes.Text)
		{
			AttributeId = 21078
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.AutoFormatType, DataTypes.Text)
		{
			AttributeId = 21076
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Bitmap, DataTypes.Text),
		new PropertyUsage(ElementType.FormControl, PropertyType.BitmapList, DataTypes.Text),
		new PropertyUsage(ElementType.FormControl, PropertyType.CaptionClass, DataTypes.Text)
		{
			AttributeId = 21080
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.ClosingDates, DataTypes.Boolean)
		{
			AttributeId = 15373
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.DateFormula, DataTypes.Boolean)
		{
			AttributeId = 15372
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.FontName, DataTypes.Text),
		new PropertyUsage(ElementType.FormControl, PropertyType.OptionValue, DataTypes.Text),
		new PropertyUsage(ElementType.FormControl, PropertyType.RunFormLink, DataTypes.Text)
		{
			AttributeId = 21116
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.RunFormView, DataTypes.Text)
		{
			AttributeId = 21115
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.RunObject, DataTypes.Text)
		{
			AttributeId = 21058
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.SubFormLink, DataTypes.Text)
		{
			AttributeId = 21056
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.SubFormView, DataTypes.Text)
		{
			AttributeId = 21055
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.TableRelation, DataTypes.Text)
		{
			AttributeId = 21114
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.ValuesAllowed, DataTypes.Text)
		{
			AttributeId = 21106
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.BitmapPos, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.Border, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.BorderStyle, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.BorderWidth, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.HorizontalAlign, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.HorizontalGlue, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.InvalidActionAppearance, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.PushAction, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.RunFormLinkType, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.ShapeStyle, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.VerticalAlign, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.VerticalGlue, DataTypes.Option),
		new PropertyUsage(ElementType.FormControl, PropertyType.OptionCaptionML, DataTypes.MLText)
		{
			AttributeId = 8632
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.PageNamesML, DataTypes.MLText),
		new PropertyUsage(ElementType.FormControl, PropertyType.ToolTipML, DataTypes.MLText)
		{
			AttributeId = 8631
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.Menu, DataTypes.Container)
		{
			ContainerName = "MENUITEMS",
			ContainerChildElementType = ElementType.MenuItem
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.BackColor, DataTypes.Integer)
		{
			AttributeId = 8528
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.DrillDownFormId, DataTypes.FormId)
		{
			AttributeId = 21069
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.LookupFormId, DataTypes.FormId)
		{
			AttributeId = 21068
		},
		new PropertyUsage(ElementType.FormControl, PropertyType.RunCommand, DataTypes.Text),
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.Temporary, DataTypes.Boolean)
		{
			AttributeId = 54066
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.Indentation, DataTypes.Integer)
		{
			AttributeId = 54094
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.SourceTable, DataTypes.ObjectId)
		{
			AttributeId = 54057
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.Width, DataTypes.Integer)
		{
			AttributeId = 54174
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.AutoCalcField, DataTypes.Boolean)
		{
			AttributeId = 54080
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.AutoReplace, DataTypes.Boolean)
		{
			AttributeId = 54070
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.AutoSave, DataTypes.Boolean)
		{
			AttributeId = 54068
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.AutoUpdate, DataTypes.Boolean)
		{
			AttributeId = 54069
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.FieldValidate, DataTypes.Boolean)
		{
			AttributeId = 54079
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.LinkTableForceInsert, DataTypes.Boolean)
		{
			AttributeId = 54067
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.CalcFields, DataTypes.Text)
		{
			AttributeId = 54061
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.ElementId, DataTypes.Text)
		{
			AttributeId = 54087
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.LinkFields, DataTypes.Text)
		{
			AttributeId = 54063
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.LinkTable, DataTypes.Text)
		{
			AttributeId = 54065
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.RequestFilterFields, DataTypes.Text)
		{
			AttributeId = 18482
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.SourceField, DataTypes.Text)
		{
			AttributeId = 54081
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.SourceTableView, DataTypes.Text)
		{
			AttributeId = 54059
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.TextType, DataTypes.Text)
		{
			AttributeId = 54051
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.VariableName, DataTypes.Text)
		{
			AttributeId = 54046
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.XmlPortElementDataType, DataTypes.Option)
		{
			AttributeId = 54078
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.MaxOccurs, DataTypes.Option)
		{
			AttributeId = 54098
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.MinOccurs, DataTypes.Option)
		{
			AttributeId = 54097
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.NodeType, DataTypes.Option)
		{
			AttributeId = 54091
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.Occurrence, DataTypes.Option)
		{
			AttributeId = 54096
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.SourceType, DataTypes.Option)
		{
			AttributeId = 54092
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.RequestFilterHeadingML, DataTypes.MLText)
		{
			AttributeId = 18490
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.Unbound, DataTypes.Boolean)
		{
			AttributeId = 54099
		},
		new PropertyUsage(ElementType.XmlPortElement, PropertyType.NamespacePrefix, DataTypes.Text)
		{
			AttributeId = 54090
		},
		new PropertyUsage(ElementType.XmlPortEvent, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.CardPageId, DataTypes.PageId)
		{
			AttributeId = 55095
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.SourceTable, DataTypes.ObjectId)
		{
			AttributeId = 21029
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.AutoSplitKey, DataTypes.Boolean)
		{
			AttributeId = 55120
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.DeleteAllowed, DataTypes.Boolean)
		{
			AttributeId = 15412
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.Editable, DataTypes.Boolean)
		{
			AttributeId = 55098
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.InsertAllowed, DataTypes.Boolean)
		{
			AttributeId = 15411
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.LinksAllowed, DataTypes.Boolean)
		{
			AttributeId = 15424
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.ModifyAllowed, DataTypes.Boolean)
		{
			AttributeId = 15413
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.MultipleNewLines, DataTypes.Boolean)
		{
			AttributeId = 15410
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.PopulateAllFields, DataTypes.Boolean)
		{
			AttributeId = 21045
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.RefreshOnActivate, DataTypes.Boolean)
		{
			AttributeId = 55131
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.SaveValues, DataTypes.Boolean)
		{
			AttributeId = 15395
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.ShowFilter, DataTypes.Boolean)
		{
			AttributeId = 55136
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.SourceTableTemporary, DataTypes.Boolean)
		{
			AttributeId = 55078
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.DataCaptionExpression, DataTypes.Text)
		{
			AttributeId = 21039
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.DataCaptionFields, DataTypes.Text)
		{
			AttributeId = 21105
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.SourceTableView, DataTypes.Text)
		{
			AttributeId = 21103
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.InstructionalTextML, DataTypes.MLText)
		{
			AttributeId = 55119
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.ActionList, DataTypes.Container)
		{
			AttributeId = 55243,
			ContainerName = "ACTIONS",
			ContainerChildElementType = ElementType.Action
		},
		new PropertyUsage(ElementType.Label, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Label, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Label, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Label, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.OptionString, DataTypes.Text)
		{
			AttributeId = 10340
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.Temporary, DataTypes.Boolean)
		{
			AttributeId = 18541
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.SourceExpression, DataTypes.Text)
		{
			AttributeId = 21052
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.DataItemTable, DataTypes.Text)
		{
			AttributeId = 18474
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.DecimalPlaces, DataTypes.DecimalPlaces)
		{
			AttributeId = 12816
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.Indentation, DataTypes.Integer),
		new PropertyUsage(ElementType.DataItem, PropertyType.MaxIteration, DataTypes.Integer)
		{
			AttributeId = 18486
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.AutoCalcField, DataTypes.Boolean)
		{
			AttributeId = 21057
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.PrintOnlyIfDetail, DataTypes.Boolean)
		{
			AttributeId = 18488
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.AutoFormatExpression, DataTypes.Text)
		{
			AttributeId = 21078
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.AutoFormatType, DataTypes.Text)
		{
			AttributeId = 21076
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.CalcFields, DataTypes.Text)
		{
			AttributeId = 18535
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.DataItemLink, DataTypes.Text)
		{
			AttributeId = 18538
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.DataItemLinkReference, DataTypes.Text)
		{
			AttributeId = 18536
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.DataItemTableView, DataTypes.Text)
		{
			AttributeId = 18475
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.RequestFilterFields, DataTypes.Text)
		{
			AttributeId = 18532
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.OptionCaptionML, DataTypes.MLText)
		{
			AttributeId = 8632
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.RequestFilterHeadingML, DataTypes.MLText)
		{
			AttributeId = 18490
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.ReportDataItemDataType, DataTypes.Option),
		new PropertyUsage(ElementType.DataItem, PropertyType.IncludeCaption, DataTypes.Boolean)
		{
			AttributeId = 18535
		},
		new PropertyUsage(ElementType.DataItem, PropertyType.UniqueId, DataTypes.Text)
		{
			AttributeId = 18558
		},
		new PropertyUsage(ElementType.Query, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.Query, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.Query, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.Query, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.Query, PropertyType.Permissions, DataTypes.Text)
		{
			AttributeId = 4947
		},
		new PropertyUsage(ElementType.Query, PropertyType.TransactionType, DataTypes.Option)
		{
			AttributeId = 4980
		},
		new PropertyUsage(ElementType.Query, PropertyType.OrderBy, DataTypes.Text)
		{
			AttributeId = 57658
		},
		new PropertyUsage(ElementType.Query, PropertyType.TopNumberOfRows, DataTypes.Integer)
		{
			AttributeId = 57604
		},
		new PropertyUsage(ElementType.Query, PropertyType.Date, DataTypes.Text),
		new PropertyUsage(ElementType.Query, PropertyType.Time, DataTypes.Text),
		new PropertyUsage(ElementType.Query, PropertyType.Modified, DataTypes.Boolean),
		new PropertyUsage(ElementType.Query, PropertyType.VersionList, DataTypes.Text),
		new PropertyUsage(ElementType.Query, PropertyType.ReadState, DataTypes.Option)
		{
			AttributeId = 57605
		},
		new PropertyUsage(ElementType.Query, PropertyType.QueryType, DataTypes.Option)
		{
			AttributeId = 57606
		},
		new PropertyUsage(ElementType.Query, PropertyType.APIVersion, DataTypes.Text)
		{
			AttributeId = 55178
		},
		new PropertyUsage(ElementType.Query, PropertyType.EntitySetName, DataTypes.Text)
		{
			AttributeId = 55086
		},
		new PropertyUsage(ElementType.Query, PropertyType.EntityName, DataTypes.Text)
		{
			AttributeId = 55087
		},
		new PropertyUsage(ElementType.Query, PropertyType.APIGroup, DataTypes.Text)
		{
			AttributeId = 55085
		},
		new PropertyUsage(ElementType.Query, PropertyType.APIPublisher, DataTypes.Text)
		{
			AttributeId = 55084
		},
		new PropertyUsage(ElementType.Query, PropertyType.QueryCategory, DataTypes.Text)
		{
			AttributeId = 57607
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.Id, DataTypes.Integer)
		{
			AttributeId = 2817
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.Name, DataTypes.Text)
		{
			AttributeId = 2818
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.Level, DataTypes.Integer),
		new PropertyUsage(ElementType.QueryElement, PropertyType.CaptionML, DataTypes.MLText)
		{
			AttributeId = 8629
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.DataItemTable, DataTypes.Text)
		{
			AttributeId = 18474
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.DataItemLink, DataTypes.Text)
		{
			AttributeId = 57645
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.Description, DataTypes.Text)
		{
			AttributeId = 15386
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.QueryElementType, DataTypes.Option),
		new PropertyUsage(ElementType.QueryElement, PropertyType.DataSource, DataTypes.Text)
		{
			AttributeId = 57753
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.ColumnFilter, DataTypes.Text)
		{
			AttributeId = 57649
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.DataItemTableFilter, DataTypes.Text)
		{
			AttributeId = 57643
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.DataItemLinkType, DataTypes.Option)
		{
			AttributeId = 57651
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.SqlJoinType, DataTypes.Option)
		{
			AttributeId = 57662
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.ReverseSign, DataTypes.Boolean)
		{
			AttributeId = 57663
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.MethodType, DataTypes.Option)
		{
			AttributeId = 57854
		},
		new PropertyUsage(ElementType.QueryElement, PropertyType.Method, DataTypes.Option)
		{
			AttributeId = 57855
		},
		new PropertyUsage(ElementType.Table, PropertyType.IncludeDataInDesc, DataTypes.Boolean)
		{
			Obsolete = true
		},
		new PropertyUsage(ElementType.Codeunit, PropertyType.CFRONTMayUsePermissions, DataTypes.Boolean)
		{
			Obsolete = true
		},
		new PropertyUsage(ElementType.RequestPage, PropertyType.TimerUpdate, DataTypes.Integer)
		{
			AttributeId = 55094,
			Obsolete = true
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.Inherited, DataTypes.Boolean)
		{
			Obsolete = true
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.PositionInGroup, DataTypes.Boolean)
		{
			Obsolete = true
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.ShortcutURL, DataTypes.Boolean)
		{
			Obsolete = true
		},
		new PropertyUsage(ElementType.MenuNode, PropertyType.ShortcutParameters, DataTypes.Boolean)
		{
			Obsolete = true
		}
	};

	public ElementType ElementType { get; private set; }

	public PropertyType PropertyType { get; private set; }

	public DataTypes DataType { get; private set; }

	public int AttributeId { get; set; }

	public string ContainerName { get; set; }

	public ElementType ContainerChildElementType { get; set; }

	public bool Obsolete { get; set; }

	internal PropertyUsage(ElementType elementType, PropertyType propertyType, DataTypes dataType)
	{
		ElementType = elementType;
		PropertyType = propertyType;
		DataType = dataType;
	}
}
