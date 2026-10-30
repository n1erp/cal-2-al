namespace Microsoft.Dynamics.Nav.Types;

public enum NavTriggerEventType
{
	None,
	OnBeforeInsertEvent,
	OnAfterInsertEvent,
	OnBeforeModifyEvent,
	OnAfterModifyEvent,
	OnBeforeDeleteEvent,
	OnAfterDeleteEvent,
	OnBeforeRenameEvent,
	OnAfterRenameEvent,
	OnBeforeValidateEvent,
	OnAfterValidateEvent,
	OnOpenPageEvent,
	OnClosePageEvent,
	OnAfterGetRecordEvent,
	OnAfterGetCurrRecordEvent,
	OnNewRecordEvent,
	OnInsertRecordEvent,
	OnModifyRecordEvent,
	OnDeleteRecordEvent,
	OnQueryClosePageEvent,
	OnPageBackgroundTaskCompletedEvent,
	OnPageBackgroundTaskErrorEvent,
	OnBeforeActionEvent,
	OnAfterActionEvent
}
