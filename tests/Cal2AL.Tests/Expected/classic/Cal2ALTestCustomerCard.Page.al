// Converted by Cal2AL from C/AL Page 50001 "Cal2AL Test Customer Card" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
page 50001 "Cal2AL Test Customer Card"
{
    Caption = 'Test Customer Card';
    SourceTable = "Cal2AL Test Customer";
    PageType = Card;
    PromotedActionCategories = 'New,Process,Report,Customer';

    layout
    {
        area(Content)
        {
            group(General)
            {
                Caption = 'General';

                field("No."; "No.")
                {
                    Importance = Promoted;

                    trigger OnAssistEdit()
                    begin
                        IF AssistEdit(xRec) THEN
                          CurrPage.UPDATE;
                    end;
                }

                field(Name; Name)
                {
                    ShowMandatory = true;
                }

                field("Customer Type"; "Customer Type")
                {
                }

                field("Salesperson Code"; "Salesperson Code")
                {
                    trigger OnValidate()
                    begin
                        SalespersonCodeOnAfterValidate;
                    end;
                }

                field(Balance; Balance)
                {
                    Visible = BalanceAvailable;

                    trigger OnDrillDown()
                    var
                        DtldCustLedgEntry: Record 379;
                    begin
                        DtldCustLedgEntry.SETRANGE("Customer No.","No.");
                        PAGE.RUN(0,DtldCustLedgEntry);
                    end;
                }

                field(DaysSinceModified; DaysSinceModified)
                {
                    Caption = 'Days Since Last Modified';
                    Editable = false;
                }

                field("Ship-to Code"; "Ship-to Code")
                {
                    trigger OnLookup(var Text: Text): Boolean
                    var
                        ShipTo: Record 222;
                    begin
                        ShipTo.SETRANGE("Customer No.","No.");
                        IF PAGE.RUNMODAL(0,ShipTo) = ACTION::LookupOK THEN BEGIN
                          Text := ShipTo.Code;
                          EXIT(TRUE);
                        END;
                    end;
                }
            }

            group(Invoicing)
            {
                Caption = 'Invoicing';

                field("Credit Limit (LCY)"; "Credit Limit (LCY)")
                {
                    Importance = Additional;
                }

                field(Control11; Name)
                {
                    Caption = 'Name (repeated)';
                }
            }
        }

        area(FactBoxes)
        {
            part(Control1900000008; 9082)
            {
                SubPageLink = "No." = field("No.");
                Visible = true;
            }

            systempart(Control1900383207; Links)
            {
                Visible = false;
            }

            systempart(Control1905767507; Notes)
            {
                Visible = true;
            }
        }
    }

    actions
    {
        area(Navigation)
        {
            group(Customer)
            {
                Caption = '&Customer';
                Image = Customer;

                action("Ledger Entries")
                {
                    ShortcutKey = 'Shift+Ctrl+L';
                    Caption = 'Ledger E&ntries';
                    RunObject = Page 25;
                    RunPageView = sorting("Customer No.");
                    RunPageLink = "Customer No." = field("No.");
                    Promoted = true;
                    Image = CustomerLedger;
                    PromotedCategory = Category4;
                }

                action(Comments)
                {
                    Caption = 'Co&mments';
                    RunObject = Page 124;
                    RunPageLink = "Table Name" = const(Customer), "No." = field("No.");
                    Image = ViewComments;
                }
            }
        }

        area(Processing)
        {
            action(CalculateBalance)
            {
                Caption = 'Calculate Balance';
                ToolTip = 'Calculate the total balance.';
                Promoted = true;
                PromotedIsBig = true;
                Image = Calculate;
                PromotedCategory = Process;

                trigger OnAction()
                var
                    Total: Decimal;
                begin
                    Total := GetTotalBalance;
                    MESSAGE(Text001,Total);
                end;
            }
        }
    }

    var
        [InDataSet]
        BalanceAvailable: Boolean;
        Text001: Label 'Total balance: %1';
        Text002: Label 'The customer has no name. Close anyway?';

    trigger OnOpenPage()
    begin
        SetControlVisibility;
    end;

    trigger OnAfterGetRecord()
    begin
        BalanceAvailable := Balance > 0;
    end;

    trigger OnQueryClosePage(CloseAction: Action): Boolean
    begin
        IF Name = '' THEN
          EXIT(CONFIRM(Text002,FALSE));
    end;

    local procedure SetControlVisibility()
    begin
        BalanceAvailable := "No." <> '';
    end;

    local procedure SalespersonCodeOnAfterValidate()
    begin
        CurrPage.UPDATE(TRUE);
    end;

    local procedure DaysSinceModified(): Integer
    begin
        IF "Last Date Modified" = 0D THEN
          EXIT(0);
        EXIT(TODAY - "Last Date Modified");
    end;
}
