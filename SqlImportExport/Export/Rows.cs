using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SqlImportExport.Export
{
    class Rows : IRows
    {
        public Rows(IEnumerable<IRow> rows)
        {
            Raw = rows;
        }

        IEnumerable<IRow> Raw { get; }

        public IEnumerator<IRow> GetEnumerator()
            => Raw.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => Raw.GetEnumerator();

        public string ToValueRows()
            => string.Join(",\r\n", Raw.Select(x => x.ToValueRow()));
    }
}
