using System;

namespace SqlImportExport.Export.PostgreSQL
{
    class PostgresBoolean : IField
    {
        public PostgresBoolean(string name, object data)
        {
            Name = name;
            var raw = Convert.ToBoolean(data);
            Value = raw ? "True" : "False";
        }

        public string Name { get; }

        public string Value { get; }
    }
}