// Converted by Cal2AL from C/AL XmlPort 50030 "Cal2AL Customer Export" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
xmlport 50030 "Cal2AL Customer Export"
{
    Caption = 'Customer Export';
    Direction = Export;
    Encoding = UTF8;
    Format = Xml;

    schema
    {
        textelement(Customers)
        {
            tableelement(Customer; "Cal2AL Test Customer")
            {
                SourceTableView = sorting("No.");

                fieldattribute(No; Customer."No.")
                {
                }

                fieldelement(Name; Customer.Name)
                {
                }

                textelement("Balance-LCY")
                {
                    trigger OnBeforePassVariable()
                    begin
                        Customer.CALCFIELDS(Balance);
                        "Balance-LCY" := FORMAT(Customer.Balance,0,9);
                    end;
                }

                trigger OnAfterGetRecord()
                begin
                    RecordCount += 1;
                end;
            }
        }
    }

    var
        RecordCount: Integer;

    trigger OnPostXmlPort()
    begin
        MESSAGE('%1 customers exported.',RecordCount);
    end;
}
