using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LumenWorks.Framework.IO.Csv
{
    public class BufferSplit : IEnumerable<CsvLine>
    {
        private Location _whereAmI;
        private int _idx;
        private bool _wasQuoted;
        private StringBuilder _field;
        private StringBuilder _mayHaveToBeAdded;
        private char[] _buffer;
        private readonly TextReader _textReader;
        private readonly CsvLayout _csvLayout;
        private bool _disposed;
        private int _lineNumber;

        public BufferSplit(TextReader textReader, CsvLayout csvLayout)
        {
            _textReader = textReader;
            _csvLayout = csvLayout;
            _buffer = new char[4];
        }

        public int LineNumber
        {
            get { return _lineNumber; }
        }

        string Yield(StringBuilder builder, bool quoted)
        {
            var result = builder.ToString();
            if (_csvLayout.TrimmingOptions == ValueTrimmingOptions.All
                || (quoted && _csvLayout.TrimmingOptions == ValueTrimmingOptions.QuotedOnly)
                || (!quoted && _csvLayout.TrimmingOptions == ValueTrimmingOptions.UnquotedOnly))
                result = result.Trim();
            return result;
        }

        public IEnumerable<string[]> Split()
        {

            var fields = new List<string>();

            Func<char?> peekNext = () =>
                                    {
                                        if (_idx + 1 >= _buffer.Length) return null;
                                        return _buffer[_idx + 1];
                                    };

            StartLine();

            while (_textReader.Peek() > 0)
            {

                var charsRead = _textReader.ReadBlock(_buffer, 0, _buffer.Length);
                _idx = 0;

                while (_idx < charsRead)
                {
                    char currentChar = _buffer[_idx];

                    switch (_whereAmI)
                    {
                        case Location.BeginningOfLine:
                            if (currentChar == '\r' || currentChar == '\n')
                            {
                                _whereAmI = Location.EndOfLine;
                                continue;
                            }
                            _whereAmI = currentChar == _csvLayout.Comment ? Location.Comment : Location.OutsideField;
                            continue;
                        case Location.Comment:
                            if (currentChar == '\r' || currentChar == '\n')
                            {
                                _whereAmI = Location.EndOfLine;
                                continue;
                            }
                            break;
                        case Location.OutsideField:
                            if (char.IsWhiteSpace(currentChar))
                            {
                                _mayHaveToBeAdded.Append(currentChar);
                                break;
                            }
                            if (currentChar == '\r' || currentChar == '\n')
                            {
                                _whereAmI = Location.EndOfLine;
                                continue;
                            }

                            if (currentChar == _csvLayout.Quote)
                            {
                                _wasQuoted = true;
                                _mayHaveToBeAdded.Length = 0;
                                _whereAmI = Location.InsideQuotedField;
                            }
                            else
                            {
                                _field.Append(_mayHaveToBeAdded);
                                _mayHaveToBeAdded.Length = 0;
                                _whereAmI = Location.InsideField;
                                continue;
                            }
                            break;
                        case Location.EndOfLine:
                            if (currentChar == '\r' && peekNext() == '\n') _idx++;
                            if (_field.Length > 0 || fields.Count > 0)
                            {
                                fields.Add(Yield(_field, _wasQuoted));
                                yield return fields.ToArray();
                            }
                            fields.Clear();
                            StartLine();
                            break;

                        case Location.Escaped:
                            _field.Append(currentChar);
                            _whereAmI = Location.InsideQuotedField;
                            break;
                        case Location.InsideQuotedField:
                            if (currentChar == _csvLayout.Escape &&
                                (peekNext() == _csvLayout.Quote || peekNext() == _csvLayout.Escape))
                            {
                                _whereAmI = Location.Escaped;
                                break; // skip the escape character
                            }
                            if (currentChar == _csvLayout.Quote)
                            {
                                // there are 2 possibilities: 
                                // - either the quote is just part of the field
                                //   (e.g. "foo,"bar "baz"", foobar")
                                // - or the quote is actually the end of this field
                                // => start capturing after the quote; check for delimiter 
                                _whereAmI = Location.AfterSecondQuote;
                                _mayHaveToBeAdded.Length = 0;
                                _mayHaveToBeAdded.Append(currentChar);
                            }
                            else
                            {
                                _field.Append(currentChar);
                            }
                            break;

                        case Location.AfterSecondQuote:
                            // we need to detect if we're actually at the end of a field. This is
                            // the case when the first non-whitespace character is the delimiter
                            // or end of line

                            if (currentChar == _csvLayout.Delimiter || currentChar == '\r' || currentChar == '\n')
                            {
                                // the second quote did mark the end of the field
                                _mayHaveToBeAdded.Length = 0;
                                fields.Add(Yield(_field, _wasQuoted));
                                _field.Length = 0;
                                _wasQuoted = false;
                                if (currentChar == '\r' || currentChar == '\n')
                                {
                                    _whereAmI = Location.EndOfLine;
                                    continue;
                                }
                                _whereAmI = Location.OutsideField;
                                break;
                            }

                            if (currentChar == _csvLayout.Quote)
                            {
                                _field.Append(_mayHaveToBeAdded);
                                _mayHaveToBeAdded.Length = 0;
                                _mayHaveToBeAdded.Append(currentChar);
                                break;
                            }

                            _mayHaveToBeAdded.Append(currentChar);

                            if (!char.IsWhiteSpace(currentChar))
                            {
                                // the second quote did NOT mark the end of the field, so we're still 'inside' the field
                                _field.Append(_mayHaveToBeAdded);
                                _whereAmI = Location.InsideQuotedField;
                            }
                            break;
                        case Location.InsideField:
                            if (currentChar == _csvLayout.Delimiter)
                            {
                                fields.Add(Yield(_field, _wasQuoted));
                                _field.Length = 0;
                                _whereAmI = Location.OutsideField;
                                break;
                            }
                            if (currentChar == '\r' || currentChar == '\n')
                            {
                                _whereAmI = Location.EndOfLine;
                                continue;
                            }
                            _field.Append(currentChar);
                            break;
                    }
                    _idx++;
                    if (_whereAmI == Location.EndOfLine)
                    {
                        break;
                    }
                }


                if (_whereAmI == Location.EndOfLine)
                {
                    var line = fields.ToArray();
                    yield return line;
                }
            }

            if (_whereAmI != Location.EndOfLine)
            {
                fields.Add(Yield(_field, _wasQuoted));
                yield return fields.ToArray();
            }


        }

        private void StartLine()
        {
            _lineNumber = LineNumber + 1;
            _mayHaveToBeAdded = new StringBuilder();
            _whereAmI = Location.BeginningOfLine;
            _wasQuoted = false;
            _field = new StringBuilder();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _textReader.Dispose();
            _disposed = true;
        }

        public IEnumerator<CsvLine> GetEnumerator()
        {
            return Split().Select(x=>new CsvLine(x)).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}