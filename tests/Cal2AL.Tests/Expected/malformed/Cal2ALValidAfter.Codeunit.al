// Converted by Cal2AL from C/AL Codeunit 50014 "Cal2AL Valid After" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00)
codeunit 50014 "Cal2AL Valid After"
{
    trigger OnRun()
    begin
        MESSAGE('after');
    end;

    procedure BrokenCode()
    begin
        IF THEN MESSAGE('x');
    end;
}
