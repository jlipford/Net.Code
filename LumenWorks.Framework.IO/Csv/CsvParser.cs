using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

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

        public CsvLayout Layout { get; set; }

        public IEnumerable<CsvLine> ParsedLines()
        {
            if (_disposed) throw new ObjectDisposedException("CsvParser");
            while (_textReader.Peek() > 0)
            {
                LineNumber++;
                var readLine = _textReader.ReadLine();
                if (string.IsNullOrEmpty(readLine))
                {
                    if (!Layout.SkipEmptyLines) yield return CsvLine.Empty;
                }
                else if (!readLine.StartsWith(new string(Layout.Comment, 1)))
                {
                    yield return new CsvLine(readLine.Split(Layout));
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