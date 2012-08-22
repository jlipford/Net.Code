using System.Collections.Generic;
using System.Linq;

namespace LumenWorks.Framework.IO.Csv
{
    public class CsvLine
    {
        private readonly bool _isEmpty;
        private readonly List<string> _fields = new List<string>();

        public CsvLine(IEnumerable<string> fields, bool isEmpty)
        {
            _isEmpty = isEmpty;
            _fields.AddRange(fields);
        }

        public bool IsEmpty
        {
            get { return _isEmpty; }
        }

        public string[] Fields
        {
            get
            {
                return  _fields.ToArray();
            }
        }

        public static readonly CsvLine Empty = new CsvLine(Enumerable.Empty<string>(), true);
    }
}