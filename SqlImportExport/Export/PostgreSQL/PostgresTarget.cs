using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlImportExport.Export.PostgreSQL
{
    class PostgresTarget : ITarget
    {
        public PostgresTarget(string table)
        {
            TableIdentifier = table;
        }

        string TableIdentifier { get; }

        public string FormattedInsert(IRows rows, IEnumerable<IColumn> columns)
            => $@"{Header(TableIdentifier, columns)}
{rows.ToValueRows()}
{Footer(columns.Where(x => x.IsPartOfPrimaryKey).Select(x => x.Name))}";

        string Header(string tableName, IEnumerable<IColumn> rows)
            => $"INSERT INTO {tableName} ({String.Join(",", rows.Select(x => x.Name))}) VALUES";

        string Footer(IEnumerable<string> uniqueRows)
            => $"ON CONFLICT ({string.Join(",", uniqueRows)}) DO NOTHING;";
    }
}
