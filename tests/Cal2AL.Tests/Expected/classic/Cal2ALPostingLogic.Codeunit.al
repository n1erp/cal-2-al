// Converted by Cal2AL from C/AL Codeunit 50010 "Cal2AL Posting Logic" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
codeunit 50010 "Cal2AL Posting Logic"
{
    // Cal2AL fixture: large codeunit covering the main C/AL statement types.
    // Table 21 (Cust. Ledger Entry) is a standard table that is not part of this fixture set.

    TableNo = "Cal2AL Test Customer";
    Permissions = TableData 21 = rimd;

    var
        Customer: Record "Cal2AL Test Customer";
        TempBuffer: Record "Cal2AL Test Customer" temporary;
        Window: Dialog;
        Text000: Label 'Processing customer %1...', Comment = '%1 is the customer number';
        Text001: Label 'Nothing to post.';
        Text002: Label 'CAL2AL', Locked = true;
        Amounts: array[12] of Decimal;
        "Key": Integer;
        Status: Option " ", Open, Released;

    trigger OnRun()
    begin
        Customer.COPY(Rec);
        "Code";
        Rec := Customer;
    end;

    local procedure "Code"()
    var
        CustLedgEntry: Record 21;
        i: Integer;
        Total: Decimal;
    begin
        Window.OPEN(Text000);
        Window.UPDATE(1,Customer."No.");
        /* Collect open entries
          into the buffer { nested braces } */
        CustLedgEntry.RESET;
        CustLedgEntry.SETCURRENTKEY("Customer No.","Posting Date");
        CustLedgEntry.SETRANGE("Customer No.",Customer."No.");
        CustLedgEntry.SETFILTER("Posting Date",'..%1',WORKDATE);
        IF CustLedgEntry.FINDSET(TRUE,FALSE) THEN
          REPEAT
            i := DATE2DMY(CustLedgEntry."Posting Date",2);
            Amounts[i] += CustLedgEntry.Amount;
            Total := Total + CustLedgEntry.Amount;
          UNTIL CustLedgEntry.NEXT = 0
        ELSE
          ERROR(Text001);

        FOR i := 1 TO ARRAYLEN(Amounts) DO
          IF Amounts[i] <> 0 THEN BEGIN
            TempBuffer.INIT;
            TempBuffer."No." := FORMAT(i);
            TempBuffer.Name := COPYSTR(Customer.Name,1,MAXSTRLEN(TempBuffer.Name));
            IF NOT TempBuffer.INSERT THEN
              TempBuffer.MODIFY;
          END;

        CASE Customer."Customer Type" OF
          Customer."Customer Type"::Company:
            "Key" := 1;
          Customer."Customer Type"::Person:
            BEGIN
              "Key" := 2;
              Customer.VALIDATE(Name,UPPERCASE(Customer.Name));
            END;
          ELSE
            "Key" := 0;
        END;

        WHILE ("Key" > 0) AND (Total < 0) DO BEGIN
          Total += 100;
          "Key" -= 1;
        END;

        IF Customer."Customer Type" IN [Customer."Customer Type"::Company,Customer."Customer Type"::Person] THEN
          Customer.MODIFY(TRUE);  // keep the record in sync

        CASE TRUE OF
          Total IN [0..100]:
            Status := Status::Open;
          Total > 100:
            Status := Status::Released;
        END;

        IF Customer.GET(Customer."No.") THEN
          Customer.DELETE(TRUE);
        Window.CLOSE;
    end;

    procedure PostDocument(var CustomerRec: Record "Cal2AL Test Customer"; Amount: Decimal; Description: Text[50]) Posted: Boolean
    var
        CustLedgEntry: Record 21;
        EntryNo: Integer;
    begin
        IF Amount = 0 THEN
          EXIT(FALSE);
        CustLedgEntry.LOCKTABLE;
        IF CustLedgEntry.FINDLAST THEN
          EntryNo := CustLedgEntry."Entry No." + 1
        ELSE
          EntryNo := 1;
        CustLedgEntry.INIT;
        CustLedgEntry."Entry No." := EntryNo;
        CustLedgEntry."Customer No." := CustomerRec."No.";
        CustLedgEntry.Amount := Amount;
        CustLedgEntry.Description := Description;
        CustLedgEntry.INSERT;
        CustomerRec.CALCFIELDS(Balance);
        OnAfterPostDocument(CustomerRec,CustLedgEntry);
        Posted := TRUE;
    end;

    [IntegrationEvent(false, false)]
    local procedure OnAfterPostDocument(var CustomerRec: Record "Cal2AL Test Customer"; CustLedgEntry: Record 21)
    begin
    end;

    [EventSubscriber(ObjectType::Table, Database::"Cal2AL Test Customer", 'OnAfterInsertEvent', '', true, true)]
    local procedure HandleCustomerInsert(var Rec: Record "Cal2AL Test Customer"; RunTrigger: Boolean)
    begin
        IF NOT RunTrigger THEN
          EXIT;
        IF Rec.ISTEMPORARY THEN
          EXIT;
        Rec."Last Date Modified" := TODAY;
    end;

    [TryFunction]
    procedure TryValidateName(var CustomerRec: Record "Cal2AL Test Customer"; NewName: Text[50])
    begin
        CustomerRec.VALIDATE(Name,NewName);
    end;

    [BusinessEvent(true)]
    procedure OnBeforeArchive(CustomerNo: Code[20])
    begin
    end;

    procedure IsInFiscalYear(PostingDate: Date; PostingTime: Time): Boolean
    var
        StartDate: Date;
        EndDate: Date;
    begin
        // C/AL date constants are MMDDYY[YY]D; AL needs yyyymmddD.
        StartDate := 20250101D;
        EndDate := 20251231D;
        IF PostingDate = 0D THEN
          EXIT(FALSE);
        IF PostingTime > 235959T THEN
          EXIT(FALSE);
        EXIT((PostingDate >= StartDate) AND (PostingDate <= EndDate));
    end;
}
