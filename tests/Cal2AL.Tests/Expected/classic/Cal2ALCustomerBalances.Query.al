// Converted by Cal2AL from C/AL Query 50040 "Cal2AL Customer Balances" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
query 50040 "Cal2AL Customer Balances"
{
    Caption = 'Customer Balances';
    OrderBy = descending(Balance);

    elements
    {
        dataitem(Customer; "Cal2AL Test Customer")
        {
            column(No; "No.")
            {
            }

            column(Name; Name)
            {
            }

            dataitem(Cust_Ledger_Entry; 21)
            {
                DataItemLink = "Customer No." = Customer."No.";
                SqlJoinType = InnerJoin;

                filter(Open; Open)
                {
                    ColumnFilter = Open = const(true);
                }

                column(Balance; Amount)
                {
                    Method = Sum;
                }
            }
        }
    }
}
