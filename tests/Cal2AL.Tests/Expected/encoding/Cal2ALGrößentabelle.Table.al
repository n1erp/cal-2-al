// Converted by Cal2AL from C/AL Table 50070 "Cal2AL Größentabelle" (Version List=CAL2AL1.00; Date=12.10.18; Time=12:00:00; Modified=Yes)
table 50070 "Cal2AL Größentabelle"
{
    Caption = 'Size Table';

    fields
    {
        field(1; "Code"; Code[10])
        {
            Caption = 'Code';
        }

        field(2; Beschreibung; Text[50])
        {
            Caption = 'Description';
        }

        field(3; "Maße 'Außen'"; Decimal)
        {
            Caption = 'Outer Size';
        }
    }

    keys
    {
        key(Key1; "Code")
        {
            Clustered = true;
        }
    }
}
