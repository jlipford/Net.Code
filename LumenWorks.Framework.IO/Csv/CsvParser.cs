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

        public CsvParser(TextReader textReader, CsvLayout layOut, CsvBehaviour behaviour)
        {
            _textReader = textReader;
            Layout = layOut;
            Behaviour = behaviour;
        }

        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        private int? _fieldCount;
        private CsvLine _cachedLine;
        private bool _initialized;
        private CsvHeader _header;
        private string _defaultHeaderName = "Column";

        public CsvLayout Layout { get; private set; }

        public CsvBehaviour Behaviour { get; private set; }

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

        public string DefaultHeaderName
        {
            get { return _defaultHeaderName; }
            set { _defaultHeaderName = value; }
        }

        public IEnumerable<CsvLine> Lines()
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


                if (string.IsNullOrEmpty(readLine) && Behaviour.SkipEmptyLines) continue;
                if (readLine != null && readLine.StartsWith(new string(Layout.Comment, 1))) continue;

                var fields = readLine.Split(Layout, Behaviour).ToList();

                if (!_fieldCount.HasValue) _fieldCount = fields.Count();

                var count = fields.Count();

                if (count < _fieldCount)
                {
                    if (Behaviour.MissingFieldAction == MissingFieldAction.ParseError) 
                        throw new MissingFieldCsvException(readLine, 0, LineNumber - 1, fields.Count() - 1);
                    string s = Behaviour.MissingFieldAction == MissingFieldAction.ReplaceByEmpty ? "" : null;
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

        public void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            var firstLine = Lines().FirstOrDefault();

            if (Layout.HasHeaders && firstLine != null)
            {
                Header = new CsvHeader(firstLine.Fields, DefaultHeaderName);
            }
            else
            {
                _cachedLine = firstLine;
            }
        }

        public IEnumerator<CsvLine> GetEnumerator()
        {
            return Lines().GetEnumerator();
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

    }
}