using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;

namespace SqlImportExport.Export.SqlServer
{
    class SqlServerProvider : IProvider
    {
        public SqlServerProvider(string connectionString, string table, IFieldFactory cellFactory)
        {
            ConnectionString = connectionString;
            var tableParts = table.Split('.');
            Schema = tableParts[0];
            Table = tableParts[1];
            TableIdentifier = table;
            CellFactory = cellFactory;
        }

        string ConnectionString { get; }
        string Schema { get; }
        string Table { get; }
        string TableIdentifier { get; }
        IFieldFactory CellFactory { get; }

        public IEnumerable<IColumn> GetColumnDescriptions()
        {
            var rows = new List<IColumn>();
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(ColumnDescriptionQuery, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            rows.Add(new Column
                            {
                                Name = reader["Name"].ToString(),
                                IsNullable = Convert.ToBoolean(reader["IsNullable"]),
                                Type = reader["Type"].ToString(),
                                Length = Convert.ToInt32(reader["Precision"]),
                                IsPartOfPrimaryKey = Convert.ToBoolean(reader["IsPrimaryKey"]),
                                IsUnique = Convert.ToBoolean(reader["IsUnique"]),
                                ReferencedTable = reader["ReferencedTable"] == DBNull.Value ? null : reader["ReferencedTable"].ToString(),
                                ReferencedColumn = reader["ReferencedColumn"] == DBNull.Value ? null : reader["ReferencedColumn"].ToString()
                            });
                        }
                    }
                }
            }
            return rows;
        }

        string ColumnDescriptionQuery
            => $@"
SELECT
	c.name AS Name,
	c.is_nullable AS IsNullable,
	ty.name AS Type,
	CASE ty.name
		WHEN 'nvarchar' THEN c.max_length / 2
		ELSE c.max_length
	END AS Precision,
	CASE WHEN (SELECT i.is_unique 
		FROM sys.index_columns AS ic
		INNER JOIN sys.indexes AS i
			ON i.object_id = ic.object_id AND i.index_id = ic.index_id AND i.is_unique = 1
		WHERE c.column_id = ic.column_id 
		  AND c.object_id = ic.object_id
	) IS NULL THEN 0 ELSE 1 END AS IsUnique,
	CASE WHEN ic.column_id IS NULL THEN 0 ELSE 1 END AS IsPrimaryKey,
	(SELECT s1.name + '.' + t1.name
		FROM sys.foreign_key_columns fkc
		INNER JOIN sys.tables t1
			ON t1.object_id = fkc.referenced_object_id
		INNER JOIN sys.schemas s1
			ON t1.schema_id = s1.schema_id
		WHERE fkc.parent_object_id = t.object_id AND fkc.parent_column_id = c.column_id
	) AS ReferencedTable,
	(SELECT c1.name
		FROM sys.foreign_key_columns AS fkc
		INNER JOIN sys.columns AS c1
			ON fkc.referenced_object_id = c1.object_id AND fkc.referenced_column_id = c1.column_id
		WHERE fkc.parent_object_id = t.object_id AND fkc.parent_column_id = c.column_id
	) AS ReferencedColumn
FROM sys.schemas AS s
INNER JOIN sys.tables AS t
	ON s.schema_id = t.schema_id
INNER JOIN sys.columns AS c
	ON t.object_id = c.object_id
INNER JOIN sys.types AS ty
	ON c.system_type_id = ty.system_type_id AND c.user_type_id = ty.user_type_id
LEFT JOIN sys.indexes AS i
	ON t.object_id = i.object_id AND i.is_primary_key = 1
LEFT JOIN sys.index_columns AS ic
	ON t.object_id = ic.object_id AND i.index_id = ic.index_id AND c.column_id = ic.column_id
WHERE s.name = '{Schema}'
  AND t.name = '{Table}'
ORDER BY c.column_id";

        public int GetCount(string condition)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(GetCountQuery(condition), connection))
                {
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        string GetCountQuery(string condition)
            => $@"SELECT COUNT(0) FROM {TableIdentifier} {GetCondition(condition)}";

        string GetCondition(string condition)
            => string.IsNullOrEmpty(condition) ? string.Empty : $"WHERE {condition}";

        public IRows GetRows(string condition, int startRow, int endRow, IEnumerable<IColumn> columns, IEnumerable<IReference> references)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(RowsQuery(condition, startRow, endRow, columns, references), connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var rows = new List<IRow>();
                        while (reader.Read())
                        {
                            var fields = new List<IField>();
                            foreach (var column in columns)
                            {
                                var refs = references
                                    .Where(x => x.Table == column.ReferencedTable)
                                    .SelectMany(table => table.Fields.Select((field, index) => 
                                        CellFactory.CreateReference(reader, field, $"__ref_{column.Name}_{index}")));
                                fields.Add(CellFactory.Create(reader, column, refs));
                            }
                            rows.Add(new Row(fields));
                        }
                        return new Rows(rows);
                    }
                }
            }
        }

        string RowsQuery(string condition, int startRow, int endRow, IEnumerable<IColumn> columns, IEnumerable<IReference> references)
            => $@"WITH cte AS (
    SELECT 
        *, 
        {ReferencesQueryUnified(ReferencesQueries(columns, references))}
        ROW_NUMBER() OVER (ORDER BY {string.Join(",", columns.Where(x => x.IsUnique).Select(x => x.Name))}) AS RowNumber
    FROM {TableIdentifier} {GetCondition(condition)})
SELECT *
FROM cte
WHERE RowNumber BETWEEN {startRow} AND {endRow}";

        string ReferencesQueryUnified(IEnumerable<string> queries)
            => queries.Count() == 0 ?
                string.Empty :
                $"{string.Join(",\r\n        ", queries)},";

        IEnumerable<string> ReferencesQueries(IEnumerable<IColumn> columns, IEnumerable<IReference> references)
            => references
                .Join(
                    columns.Where(x => x.IsForeignKeyColumn),
                    reference => reference.Table,
                    column => column.ReferencedTable,
                    (reference, column) => new
                    {
                        Reference = reference,
                        Column = column
                    })
                .SelectMany(x => x.Reference.Fields.Select((field, i) => 
                    $"(SELECT {field} FROM {x.Column.ReferencedTable} AS ref WHERE ref.{x.Column.ReferencedColumn} = {x.Column.Name}) AS __ref_{x.Column.Name}_{i}"));
    }
}
