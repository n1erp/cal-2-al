// Converted by Cal2AL from C/AL delta of "Cal2AL Test Customer Card"(Page 50001) (Version List=CAL2AL1.00,MOD01; Date=15.10.18; Time=12:00:00)
pageextension 70000001 "Cal2AL Test Customer Card Ext" extends "Cal2AL Test Customer Card"
{
    layout
    {
        addafter("Salesperson Code")
        {
            field("Loyalty Level"; "Loyalty Level")
            {
            }

            field("Loyalty Points"; "Loyalty Points")
            {
                trigger OnValidate()
                begin
                    CurrPage.UPDATE;
                end;
            }
        }

        modify(Balance)
        {
            Visible = true;
        }
    }

    actions
    {
        addafter(CalculateBalance)
        {
            action(AddPoints)
            {
                Caption = 'Add Loyalty Points';
                Image = Add;

                trigger OnAction()
                begin
                    AddLoyaltyPoints(10);
                end;
            }
        }
    }

    var
        LoyaltyVisible: Boolean;

    trigger OnOpenPage()
    begin
        SetLoyaltyVisibility;
    end;

    local procedure SetLoyaltyVisibility()
    begin
        LoyaltyVisible := "Loyalty Points" > 0;
    end;
}
