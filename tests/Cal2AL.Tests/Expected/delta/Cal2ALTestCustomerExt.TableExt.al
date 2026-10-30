// Converted by Cal2AL from C/AL delta of "Cal2AL Test Customer"(Table 50000) (Version List=CAL2AL1.00,MOD01; Date=15.10.18; Time=12:00:00)
tableextension 70000000 "Cal2AL Test Customer Ext" extends "Cal2AL Test Customer"
{
    fields
    {
        field(50000; "Loyalty Level"; Option)
        {
            Caption = 'Loyalty Level';
            OptionMembers = "None",Silver,Gold;
        }

        field(50001; "Loyalty Points"; Integer)
        {
            Caption = 'Loyalty Points';

            trigger OnValidate()
            begin
                IF "Loyalty Points" > 1000 THEN
                  "Loyalty Level" := "Loyalty Level"::Gold;
            end;
        }
    }

    var
        LoyaltyMgt: Codeunit "Cal2AL Posting Logic";

    trigger OnModify()
    begin
        IF "Loyalty Points" < 0 THEN
          "Loyalty Points" := 0;
    end;

    procedure AddLoyaltyPoints(Points: Integer)
    begin
        "Loyalty Points" += Points;
        MODIFY(TRUE);
    end;
}
