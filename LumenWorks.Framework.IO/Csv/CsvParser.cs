using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LumenWorks.Framework.IO.Csv
{
    class CsvParser : IEnumerable<CsvLine>, IDisposable
    {
        private readonly BufferSplit _bufferSplit;
        private bool _disposed;

        public CsvParser(TextReader textReader, int bufferSize, CsvLayout layOut, CsvBehaviour behaviour)
        {
            _bufferSplit = new BufferSplit(textReader, bufferSize, layOut, behaviour);
            _enumerator = _bufferSplit.GetEnumerator();
            Layout = layOut;
            Behaviour = behaviour;
        }

        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        private CsvLine _cachedLine;
        private bool _initialized;
        private CsvHeader _header;
        private string _defaultHeaderName = "Column";
        private IEnumerator<CsvLine> _enumerator;

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
            get { return _bufferSplit.FieldCount??-1; }
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

            while (_enumerator.MoveNext())
            {
                LineNumber++;

                var readLine = _enumerator.Current;

                yield return readLine;

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
            _bufferSplit.Dispose();
            _disposed = true;
        }

    }
}