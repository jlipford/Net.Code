namespace LumenWorks.Framework.IO.Csv
{

    public class CsvBehaviour
    {
        private readonly ValueTrimmingOptions _trimmingOptions;

        public CsvBehaviour(ValueTrimmingOptions trimmingOptions = ValueTrimmingOptions.UnquotedOnly)
        {
            _trimmingOptions = trimmingOptions;
        }

        public ValueTrimmingOptions TrimmingOptions
        {
            get { return _trimmingOptions; }
        }
    }

	public class CsvLayout
	{
		private readonly char _quote;
		private readonly char _delimiter;
		private readonly char _escape;
	    private readonly char _comment;
	    private readonly bool _hasHeaders;


	    public CsvLayout(
            char quote = '"', 
            char delimiter = ',', 
            char escape = '"', 
            char comment = '#',
            bool hasHeaders = false)
		{
			_quote = quote;
			_delimiter = delimiter;
			_escape = escape;
		    _comment = comment;
	        _hasHeaders = hasHeaders;
		}

		public char Quote
		{
			get { return _quote; }
		}

		public char Delimiter
		{
			get { return _delimiter; }
		}

		public char Escape
		{
			get { return _escape; }
		}

	    public char Comment
	    {
	        get { return _comment; }
	    }

	    public bool HasHeaders
	    {
	        get { return _hasHeaders; }
	    }
	}
}