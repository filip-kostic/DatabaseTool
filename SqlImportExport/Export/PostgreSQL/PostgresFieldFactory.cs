using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlImportExport.Export.PostgreSQL
{
    class PostgresFieldFactory : IFieldFactory
    {
        public IField CreateReference(IDataReader reader, string referencedColumn, string column)
        {
            var raw = reader[column];
            if (raw == DBNull.Value)
                return NullCell;

            string type = reader.GetFieldType(reader.GetOrdinal(column)).ToString();

            return GetByType(type, referencedColumn, raw);
        }

        public IField Create(IDataReader reader, IColumn column, IEnumerable<IField> references)
        {
            var raw = reader[column.Name];
            if (raw == DBNull.Value)
                return NullCell;

            if (column.IsForeignKeyColumn && references.Count() > 0)
                return new PostgresSelectField(column, raw, references);

            return GetByType(column.Type, column.Name, raw);
        }

        static IField GetByType(string type, string column, object raw)
        {
            switch (type)
            {
                case "text":
                case "varchar":
                case "nvarchar":
                case "character varying":
                case "System.String":
                case "xml":
                case "date":
                case "datetime":
                case "smalldatetime":
                case "timestamp":
                case "timestamp without time zone":
                case "uniqueidentifier":
                case "uuid":
                case "System.Guid":
                    return new SqlStringCell(column, raw);
                case "bit":
                case "boolean":
                case "System.Boolean":
                    return new PostgresBoolean(column, raw);
                default:
                    return new RawCell(column, raw);
            }
        }

        static IField NullCell = new NullCell();
    }
}
