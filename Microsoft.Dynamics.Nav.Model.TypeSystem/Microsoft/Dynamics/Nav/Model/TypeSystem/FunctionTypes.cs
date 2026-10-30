namespace Microsoft.Dynamics.Nav.Model.TypeSystem;

[DataType(DataTypes.FunctionType)]
public enum FunctionTypes
{
	Normal,
	Test,
	MessageHandler,
	ConfirmHandler,
	StrMenuHandler,
	PageHandler,
	ModalPageHandler,
	ReportHandler,
	RequestPageHandler
}
