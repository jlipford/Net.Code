using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LumenWorks.Framework.IO.Csv
{
    class CsvParser : IEnumerable<CsvLine>, IDisposable
    {
        private readonly TextReader _textReader;
        private bool _disposed;

        public CsvParser(TextReader textReader, CsvLayout layOut)
        {
            _textReader = textReader;
            Layout = layOut;
        }

        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        private int? _fieldCount;
        private MissingFieldAction _missingFieldAction;
        private bool _skipEmptyLines = true;
        private CsvLine _cachedLine;
        private bool _initialized;
        private CsvHeader _header;

        public CsvLayout Layout { get; set; }

        public CsvHeader Header
        {
            get
            {
                Initialize();
                return _header;
            }
            set { _header = value; }
        }

        public int FieldCount
        {
            get { return _fieldCount??-1; }
        }

        public IEnumerable<CsvLine> ParsedLines()
        {
            Initialize();

            if (_cachedLine != null)
            {
                yield return _cachedLine;
                _cachedLine = null;
            }

            while (_textReader.Peek() > 0)
            {
                LineNumber++;
                var readLine = _textReader.ReadLine();


                if (string.IsNullOrEmpty(readLine) && _skipEmptyLines) continue;
                if (readLine != null && readLine.StartsWith(new string(Layout.Comment, 1))) continue;

                var fields = readLine.Split(Layout).ToList();

                if (!_fieldCount.HasValue) _fieldCount = fields.Count();

                var count = fields.Count();

                if (count < _fieldCount)
                {
                    if (_missingFieldAction == MissingFieldAction.ParseError) 
                        throw new MissingFieldCsvException(readLine, 0, LineNumber - 1, fields.Count() - 1);
                    string s = _missingFieldAction == MissingFieldAction.ReplaceByEmpty ? "" : null;
                    fields = fields.Concat(Enumerable.Repeat(s, _fieldCount.Value - count)).ToList();
                }


                if (string.IsNullOrEmpty(readLine))
                {
                    yield return CsvLine.Empty;
                }
                else
                {
                    yield return new CsvLine(fields);
                }
            }
        }

        private void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            var firstLine = ParsedLines().FirstOrDefault();

            if (Layout.HasHeaders && firstLine != null)
            {
                Header = new CsvHeader(firstLine.Fields);
            }
            else
            {
                _cachedLine = firstLine;
            }
        }

        public IEnumerator<CsvLine> GetEnumerator()
        {
            return ParsedLines().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _textReader.Dispose();
            _disposed = true;
        }

        public void SetMissingFieldAction(MissingFieldAction value)
        {
            _missingFieldAction = value;
        }

        public void SkipEmptyLines(bool value)
        {
            _skipEmptyLines = value;
        }
    }

    class CsvHeader : CsvLine
    {
        public CsvHeader(IEnumerable<string> fields) : base(fields)
        {
        }

        public int this[string headerName]
        {
            get { return 0; }
        }
    }
}