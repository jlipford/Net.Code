namespace LumenWorks.Framework.IO.Csv
{
	public struct CsvLayout
	{
		private readonly char _quote;
		private readonly char _delimiter;
		private readonly ValueTrimmingOptions _trimmingOptions;
		private readonly char _escape;

		public CsvLayout(char quote, char delimiter, ValueTrimmingOptions trimmingOptions = ValueTrimmingOptions.UnquotedOnly, char escape = '"')
		{
			_quote = quote;
			_delimiter = delimiter;
			_trimmingOptions = trimmingOptions;
			_escape = escape;
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
	}
}