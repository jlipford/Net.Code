using System.Collections.Generic;
using System.Linq;

namespace LumenWorks.Framework.IO.Csv
{
    public class CsvLine
    {
        private readonly List<string> _fields = new List<string>();

        public CsvLine(IEnumerable<string> fields)
        {
            _fields.AddRange(fields);
        }

        public bool IsEmpty
        {
            get { return _fields.Count == 0 || (_fields.Count == 1 && string.IsNullOrEmpty(_fields[0])); }
        }

        public string[] Fields
        {
            get
            {
                return IsEmpty ? new[] {string.Empty} : _fields.ToArray();
            }
        }

        public static readonly CsvLine Empty = new CsvLine(Enumerable.Empty<string>());
    }
}