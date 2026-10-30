// Converted by Cal2AL from C/AL Table 50050 "Cal2AL New Syntax Item" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
table 50050 "Cal2AL New Syntax Item"
{
    Caption = 'New Syntax Item';
    LookupPageId = "Cal2AL Test Customer List";

    fields
    {
        field(1; "No."; Code[20])
        {
            Caption = 'No.';
        }

        field(2; "Customer No."; Code[20])
        {
            TableRelation = "Cal2AL Test Customer"."No." where("Customer Type" = const(Company));
            Caption = 'Customer No.';
        }

        field(3; "Customer Balance"; Decimal)
        {
            FieldClass = FlowField;
            CalcFormula = lookup("Cal2AL Test Customer".Balance where("No." = field("Customer No.")));
            Caption = 'Customer Balance';
            Editable = false;
        }
    }

    keys
    {
        key(Key1; "No.")
        {
            Clustered = true;
        }
    }

    var
        Cust: Record "Cal2AL Test Customer";

    procedure GetCustomerName(): Text[50]
    begin
        IF Cust.GET("Customer No.") THEN
          EXIT(Cust.Name);
    end;
}
