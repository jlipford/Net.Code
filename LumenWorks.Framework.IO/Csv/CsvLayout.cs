namespace LumenWorks.Framework.IO.Csv
{
	public class CsvLayout
	{
		private readonly char _quote;
		private readonly char _delimiter;
		private readonly ValueTrimmingOptions _trimmingOptions;
		private readonly char _escape;
	    private readonly char _comment;
	    private readonly bool _skipEmptyLines;
	    private readonly MissingFieldAction _missingFieldAction;


	    public CsvLayout(
            char quote = '"', 
            char delimiter = ',', 
            ValueTrimmingOptions trimmingOptions = ValueTrimmingOptions.UnquotedOnly, 
            char escape = '"', 
            char comment = '#', 
            bool skipEmptyLines = true,
            MissingFieldAction missingFieldAction = MissingFieldAction.ParseError)
		{
			_quote = quote;
			_delimiter = delimiter;
			_trimmingOptions = trimmingOptions;
			_escape = escape;
		    _comment = comment;
	        _skipEmptyLines = skipEmptyLines;
	        _missingFieldAction = missingFieldAction;
		}

		public char Quote
		{
			get { return _quote; }
		}

		public char Delimiter
		{
			get { return _delimiter; }
		}

		public ValueTrimmingOptions TrimmingOptions
		{
			get { return _trimmingOptions; }
		}

		public char Escape
		{
			get { return _escape; }
		}

	    public char Comment
	    {
	        get { return _comment; }
	    }

	    public bool SkipEmptyLines
	    {
	        get { return _skipEmptyLines; }
	    }

	    public MissingFieldAction MissingFieldAction
	    {
	        get { return _missingFieldAction; }
	    }
	}
}