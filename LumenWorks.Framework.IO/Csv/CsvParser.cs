using System.Collections.Generic;
using System.IO;

namespace LumenWorks.Framework.IO.Csv
{
    class CsvParser
    {
        private readonly TextReader _textReader;
        private readonly CsvLayout _layOut;

        public CsvParser(TextReader textReader, CsvLayout layOut)
        {
            _textReader = textReader;
            _layOut = layOut;
        }

        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }

        public IEnumerable<CsvLine> ParsedLines()
        {
            while (_textReader.Peek() > 0)
            {
                var readLine = _textReader.ReadLine();
                if (string.IsNullOrEmpty(readLine))
                {
                    if (!_layOut.SkipEmptyLines) yield return CsvLine.Empty;
                }
                else if (!readLine.StartsWith(new string(_layOut.Comment, 1)))
                {
                    yield return new CsvLine(readLine.Split(_layOut));
                }
            }
        }

    }
}