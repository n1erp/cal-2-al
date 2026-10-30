// Converted by Cal2AL from C/AL Page 50002 "Cal2AL Test Customer List" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
page 50002 "Cal2AL Test Customer List"
{
    Editable = false;
    Caption = 'Test Customers';
    SourceTable = "Cal2AL Test Customer";
    SourceTableView = sorting(Name) where(Blocked = const(false));
    PageType = List;
    CardPageId = "Cal2AL Test Customer Card";

    layout
    {
        area(Content)
        {
            repeater(Control1)
            {
                field("No."; "No.")
                {
                }

                field(Name; Name)
                {
                }

                field(Balance; Balance)
                {
                    BlankZero = true;
                }

                field("E-Mail"; "E-Mail")
                {
                    Visible = false;
                }
            }
        }
    }

    actions
    {
        area(Reporting)
        {
            action("Customer List")
            {
                Caption = 'Customer List';
                RunObject = Report "Cal2AL Customer List";
                Promoted = true;
                Image = "Report";
                PromotedCategory = Report;
            }

            separator(Action3)
            {
            }
        }
    }
}
