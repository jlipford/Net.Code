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

        public CsvLayout Layout { get; set; }

        public IEnumerable<CsvLine> ParsedLines()
        {
            if (_disposed) throw new ObjectDisposedException("CsvParser");
            while (_textReader.Peek() > 0)
            {
                LineNumber++;
                var readLine = _textReader.ReadLine();

                var fields = readLine.Split(Layout).ToList();

                if (!_fieldCount.HasValue) _fieldCount = fields.Count();

                var count = fields.Count();

                if (count < _fieldCount)
                {
                    if (Layout.MissingFieldAction == MissingFieldAction.ParseError) throw new MissingFieldCsvException(readLine, 0, LineNumber - 1, fields.Count() - 1);
                    string s = Layout.MissingFieldAction == MissingFieldAction.ReplaceByEmpty ? "" : null;
                    fields = fields.Concat(Enumerable.Repeat(s, _fieldCount.Value - count)).ToList();
                }


                if (string.IsNullOrEmpty(readLine))
                {
                    if (!Layout.SkipEmptyLines) yield return CsvLine.Empty;
                }
                else if (!readLine.StartsWith(new string(Layout.Comment, 1)))
                {
                    yield return new CsvLine(fields);
                }
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
    }
}