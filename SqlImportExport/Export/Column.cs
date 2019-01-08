namespace SqlImportExport.Export
{
    class Column : IColumn
    {
        public string Name { get; set; }

        public bool IsNullable { get; set; }

        public string Type { get; set; }

        public int Length { get; set; }

        public bool IsUnique { get; set; }

        public string ReferencedTable { get; set; }

        public string ReferencedColumn { get; set; }

        public bool IsForeignKeyColumn
            => !string.IsNullOrEmpty(ReferencedTable) && !string.IsNullOrEmpty(ReferencedColumn);

        public bool IsPartOfPrimaryKey { get; set; }
    }
}