using System;
using System.Collections.Generic;
using System.Text;

namespace LumenWorks.Framework.IO.Csv
{
	public class BufferSplit
	{
		private Location whereAmI;
		private int idx;
		private bool wasQuoted;
		private StringBuilder field;
		private StringBuilder mayHaveToBeAdded;

		public IEnumerable<string> Split(char[] buffer, CsvLayout csvLayout)
		{


			Func<StringBuilder, bool, string> Yield = (builder, quoted) =>
			                                          	{
			                                          		var result = builder.ToString();
			                                          		if (csvLayout.TrimmingOptions == ValueTrimmingOptions.All
			                                          		    || (quoted && csvLayout.TrimmingOptions == ValueTrimmingOptions.QuotedOnly)
			                                          		    || (!quoted && csvLayout.TrimmingOptions == ValueTrimmingOptions.UnquotedOnly))
			                                          			result = result.Trim();
			                                          		return result;
			                                          	};

			Func<char?> peekNext = () =>
			                       	{
			                       		if (idx+1 >= buffer.Length) return null;
			                       		return buffer[idx + 1];
			                       	};

			whereAmI = Location.OutsideField;
			idx = 0;
			wasQuoted = false;
			mayHaveToBeAdded = new StringBuilder();
			field = new StringBuilder();
			
			while (idx < buffer.Length)
			{
				char currentChar = buffer[idx];

				switch (whereAmI)
				{
					case Location.OutsideField:
						if (char.IsWhiteSpace(currentChar))
						{
							mayHaveToBeAdded.Append(currentChar);
							break;
						}
						if (currentChar == csvLayout.Quote)
						{
							wasQuoted = true;
							mayHaveToBeAdded.Length = 0;
							whereAmI = Location.InsideQuotedField;
						}
						else
						{
							field.Append(mayHaveToBeAdded);
							mayHaveToBeAdded.Length = 0;
							whereAmI = Location.InsideField;
							continue;
						}
						break;
					case Location.Escaped:
						field.Append(currentChar);
						whereAmI = Location.InsideQuotedField;
						break;
					case Location.InsideQuotedField:
						if (currentChar == csvLayout.Escape && 
						    (peekNext() == csvLayout.Quote || peekNext() == csvLayout.Escape))
						{
							whereAmI = Location.Escaped;
							break; // skip the escape character
						}
						if (currentChar == csvLayout.Quote)
						{
							// there are 2 possibilities: 
							// - either the quote is just part of the field
							//   (e.g. "foo,"bar "baz"", foobar")
							// - or the quote is actually the end of this field
							// => start capturing after the quote; check for delimiter 
							whereAmI = Location.AfterSecondQuote;
							mayHaveToBeAdded.Length = 0;
							mayHaveToBeAdded.Append(currentChar);
						}
						else
						{
							field.Append(currentChar);
						}
						break;

					case Location.AfterSecondQuote:
						if (currentChar == csvLayout.Delimiter)
						{
							// the second quote did mark the end of the field
							whereAmI = Location.OutsideField;
							mayHaveToBeAdded.Length = 0;
							yield return Yield(field, wasQuoted);
							field.Length = 0;
							wasQuoted = false;
							break;
						}

						mayHaveToBeAdded.Append(currentChar);

						

						if (!char.IsWhiteSpace(currentChar))
						{
							// the second quote did NOT mark the end of the field, so we're still 'inside' the field
							field.Append(mayHaveToBeAdded.ToString());
							whereAmI = Location.InsideField;
						}
						break;
					case Location.InsideField:
						if (currentChar == csvLayout.Delimiter)
						{
							yield return Yield(field, wasQuoted);
							field.Length = 0;
							whereAmI = Location.OutsideField;
							break;
						}
						field.Append(currentChar);
						break;
				}
				idx++;
			}

			yield return Yield(field, wasQuoted);
		}
	
	}
}