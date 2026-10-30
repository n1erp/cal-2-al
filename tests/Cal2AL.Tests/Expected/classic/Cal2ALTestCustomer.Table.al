// Converted by Cal2AL from C/AL Table 50000 "Cal2AL Test Customer" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
table 50000 "Cal2AL Test Customer"
{
    // Cal2AL test fixture: classic table export.
    // Covers triggers, option fields, FlowFields, conditional table relations and keys.

    DataCaptionFields = "No.", Name;
    Permissions = TableData 21 = r,
                  TableData "Cal2AL Test Customer" = rimd;
    Caption = 'Test Customer';
    LookupPageId = "Cal2AL Test Customer List";
    DrillDownPageId = "Cal2AL Test Customer List";

    fields
    {
        field(1; "No."; Code[20])
        {
            Caption = 'No.';
            NotBlank = true;

            trigger OnValidate()
            begin
                IF "No." <> xRec."No." THEN BEGIN
                  SalesSetup.GET;
                  NoSeriesMgt.TestManual(SalesSetup."Customer Nos.");
                  "No. Series" := '';
                END;
            end;
        }

        field(2; Name; Text[50])
        {
            Caption = 'Name';
        }

        field(3; "Customer Type"; Option)
        {
            Caption = 'Customer Type';
            OptionCaption = ' ,Company,Person';
            OptionMembers = " ",Company,Person;
        }

        field(4; "Salesperson Code"; Code[10])
        {
            TableRelation = "Salesperson/Purchaser";
            Caption = 'Salesperson Code';
        }

        field(5; Balance; Decimal)
        {
            FieldClass = FlowField;
            CalcFormula = sum("Detailed Cust. Ledg. Entry".Amount where("Customer No." = field("No."), "Posting Date" = field("Date Filter")));
            Caption = 'Balance';
            Editable = false;
            AutoFormatType = 1;
        }

        field(6; "Date Filter"; Date)
        {
            FieldClass = FlowFilter;
            Caption = 'Date Filter';
        }

        field(7; "Ship-to Code"; Code[10])
        {
            TableRelation = if ("Customer Type" = const(Company)) "Ship-to Address"."Code" where("Customer No." = field("No.")) else if ("Customer Type" = const(Person)) "Alternative Address"."Code" where("Employee No." = field("No."));
            ValidateTableRelation = false;
            Caption = 'Ship-to Code';
        }

        field(8; "Last Date Modified"; Date)
        {
            Caption = 'Last Date Modified';
            Editable = false;
        }

        field(9; "No. Series"; Code[10])
        {
            TableRelation = "No. Series";
            Caption = 'No. Series';
            Editable = false;
        }

        field(10; "Credit Limit (LCY)"; Decimal)
        {
            InitValue = 1000.5;
            Caption = 'Credit Limit (LCY)';
            MinValue = 0;
            AutoFormatType = 1;
        }

        field(11; Blocked; Boolean)
        {
            InitValue = false;
            Caption = 'Blocked';
        }

        field(12; Picture; Blob)
        {
            Caption = 'Picture';
            Subtype = Bitmap;
        }

        field(13; "E-Mail"; Text[80])
        {
            ExtendedDatatype = EMail;
            Caption = 'E-Mail';
        }

        field(14; "Open Entries"; Integer)
        {
            FieldClass = FlowField;
            CalcFormula = count("Cust. Ledger Entry" where("Customer No." = field("No."), Open = const(true)));
            Caption = 'Open Entries';
            Editable = false;
        }

        field(15; "Old Field"; Code[10])
        {
            Enabled = false;
            Caption = 'Old Field';
        }
    }

    keys
    {
        key(Key1; "No.")
        {
            Clustered = true;
        }

        key(Key2; Name)
        {
        }

        key(Key3; "Customer Type", Name)
        {
            SumIndexFields = "Credit Limit (LCY)";
        }

        key(Key4; "Salesperson Code")
        {
            Enabled = false;
        }
    }

    fieldgroups
    {
        fieldgroup(DropDown; "No.", Name, "Customer Type")
        {
        }
    }

    var
        Text000: Label 'You cannot delete %1 %2 because there are still entries.';
        SalesSetup: Record 311;
        NoSeriesMgt: Codeunit 396;

    trigger OnInsert()
    begin
        IF "No." = '' THEN BEGIN
          SalesSetup.GET;
          SalesSetup.TESTFIELD("Customer Nos.");
          NoSeriesMgt.InitSeries(SalesSetup."Customer Nos.",xRec."No. Series",0D,"No.","No. Series");
        END;
        "Last Date Modified" := TODAY;
    end;

    trigger OnModify()
    begin
        "Last Date Modified" := TODAY;
    end;

    trigger OnDelete()
    var
        CustLedgEntry: Record 21;
    begin
        CustLedgEntry.SETRANGE("Customer No.","No.");
        IF NOT CustLedgEntry.ISEMPTY THEN
          ERROR(Text000,TABLECAPTION,"No.");
    end;

    procedure AssistEdit(OldCust: Record "Cal2AL Test Customer"): Boolean
    var
        Cust: Record "Cal2AL Test Customer";
    begin
        WITH Cust DO BEGIN
          Cust := Rec;
          SalesSetup.GET;
          SalesSetup.TESTFIELD("Customer Nos.");
          IF NoSeriesMgt.SelectSeries(SalesSetup."Customer Nos.",OldCust."No. Series","No. Series") THEN BEGIN
            NoSeriesMgt.SetSeries("No.");
            Rec := Cust;
            EXIT(TRUE);
          END;
        END;
    end;

    procedure GetTotalBalance() Total: Decimal
    begin
        /* Sums up the balance
          over all entries */
        CALCFIELDS(Balance);
        Total := Balance; // assign the FlowField
    end;
}
