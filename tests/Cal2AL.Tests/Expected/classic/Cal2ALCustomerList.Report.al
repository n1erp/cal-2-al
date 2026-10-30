// Converted by Cal2AL from C/AL Report 50020 "Cal2AL Customer List" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
report 50020 "Cal2AL Customer List"
{
    DefaultLayout = RDLC;
    RDLCLayout = 'layouts/Cal2ALCustomerList.Report.rdlc';
    Caption = 'Customer List';

    dataset
    {
        dataitem(Customer; "Cal2AL Test Customer")
        {
            DataItemTableView = sorting("No.");
            PrintOnlyIfDetail = false;
            RequestFilterFields = "No.", Name, "Customer Type";
            RequestFilterHeading = 'Customer';

            column(COMPANYNAME; COMPANYNAME)
            {
            }

            column(Customer_No; "No.")
            {
                IncludeCaption = true;
            }

            column(Customer_Name; Name)
            {
                IncludeCaption = true;
            }

            column(Customer_Balance; Balance)
            {
                AutoFormatType = 1;
            }

            dataitem(Ledger; 21)
            {
                DataItemTableView = sorting("Customer No.", "Posting Date") where(Open = const(true));
                DataItemLink = "Customer No." = field("No.");

                column(Ledger_Amount; Amount)
                {
                }
            }

            trigger OnAfterGetRecord()
            begin
                CALCFIELDS(Balance);
                IF OnlyWithBalance AND (Balance = 0) THEN
                  CurrReport.SKIP;
            end;
        }
    }

    requestpage
    {
        SaveValues = true;

        layout
        {
            area(Content)
            {
                group(Options)
                {
                    Caption = 'Options';

                    field(OnlyWithBalance; OnlyWithBalance)
                    {
                        Caption = 'Only with Balance';
                    }
                }
            }
        }
    }

    labels
    {
        ReportTitleLbl = 'Customer List';
    }

    var
        CustFilter: Text;
        OnlyWithBalance: Boolean;

    trigger OnPreReport()
    begin
        CustFilter := Customer.GETFILTERS;
    end;
}
