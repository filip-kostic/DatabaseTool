using System.Collections.Generic;
using System.Linq;

namespace SqlImportExport.Export.PostgreSQL
{
    class PostgresSelectField : IField
    {
        public PostgresSelectField(IColumn column, object raw, IEnumerable<IField> references)
        {
            Name = column.Name;
            Value = $"(SELECT rfr.{column.ReferencedColumn} FROM {column.ReferencedTable} AS rfr WHERE {string.Join(" AND ", references.Select(x => $"rfr.{x.Name} = {x.Value}"))})";
        }

        public string Name { get; }

        public string Value { get; }
    }
}
