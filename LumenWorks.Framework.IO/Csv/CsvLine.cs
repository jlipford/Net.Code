using System.Collections.Generic;
using System.Linq;

namespace LumenWorks.Framework.IO.Csv
{
    class CsvLine
    {
        private bool _isEmpty;
        private readonly List<string> _fields = new List<string>();

        public CsvLine(IEnumerable<string> fields)
        {
            _fields.AddRange(fields);
            _isEmpty = !_fields.Any() || (_fields.Count == 1 && _fields[0] == string.Empty);
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
        }

        public string[] Fields
        {
            get { return _fields.ToArray(); }
        }

        private CsvLine(){}

        public static readonly CsvLine Empty = new CsvLine(new[]{string.Empty});
    }
}