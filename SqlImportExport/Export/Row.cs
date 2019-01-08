using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SqlImportExport.Export
{
    class Row : IRow
    {
        public Row(IEnumerable<IField> cells)
        {
            Raw = cells;
        }

        IEnumerable<IField> Raw { get; }

        public IEnumerator<IField> GetEnumerator()
            => Raw.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Raw.GetEnumerator();

        public string ToValueRow()
            => $"({string.Join(",", Raw.Select(x => x.Value))})";
    }
}
