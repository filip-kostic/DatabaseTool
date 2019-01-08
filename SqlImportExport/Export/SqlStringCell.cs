namespace SqlImportExport.Export
{
    class SqlStringCell : IField
    {
        public SqlStringCell(string name, object data)
        {
            Name = name;
            Value = $"'{data.ToString().Replace("'", "''")}'";
        }

        public string Name { get; }

        public string Value { get; }
    }
}