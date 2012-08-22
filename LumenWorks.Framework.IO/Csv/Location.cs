namespace LumenWorks.Framework.IO.Csv
{
    enum Location
    {
        InsideField,
        InsideQuotedField,
        AfterSecondQuote,
        OutsideField,
        Escaped,
        EndOfLine,
        BeginningOfLine,
        Comment,
        ParseError
    }
}