// Converted by Cal2AL from C/AL Codeunit 50011 "Cal2AL Legacy Interop" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
codeunit 50011 "Cal2AL Legacy Interop"
{
    var
        XlApp: Automation "{00020813-0000-0000-C000-000000000046} 1.6:{00024500-0000-0000-C000-000000000046}:'Microsoft Excel 16.0 Object Library'.Application";
        [WithEvents]
        Timer: DotNet Timer;
        ServerFile: File;

    trigger OnRun()
    begin
        IF ISSERVICETIER THEN
          ExportToExcel;
    end;

    trigger Timer::Elapsed(sender: Variant; e: DotNet ElapsedEventArgs)
    begin
        MESSAGE('Elapsed');
    end;

    procedure ExportToExcel()
    begin
        CREATE(XlApp,FALSE,TRUE);
        XlApp.Visible(TRUE);
        CLEAR(XlApp);
    end;

    procedure ShowCustomer(CustomerNo: Code[20])
    var
        Customer: Record "Cal2AL Test Customer";
    begin
        Customer.GET(CustomerNo);
        FORM.RUNMODAL(FORM::"Customer Card",Customer);
    end;

    procedure WriteServerFile(FileName: Text)
    begin
        ServerFile.CREATE(FileName);
        ServerFile.WRITE('Cal2AL');
        ServerFile.CLOSE;
        SHELL('notepad.exe',FileName);
    end;

    procedure StartTimer()
    begin
        Timer := Timer.Timer(1000);
        Timer.Enabled := TRUE;
    end;
}
